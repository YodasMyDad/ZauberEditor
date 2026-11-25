import {
    ATOMIC_SELECTOR, CELL_TAGS, HEADING_TAGS, blockOf, cellOf, closestWithin, emptyParagraph, ensurePlaceholder, hasOwnContent,
    hasVisibleContent, isTextBlock, isText, maxOffset, mergeBlocks, removeTrailingBreak, removeWithEmptyAncestors, retag,
    splitAt, wrapInlineRuns
} from './dom.js';
import { deleteSelection, insertBreak, insertNewline, restOfBlockEmpty } from './insert.js';
import { enterInListItem, indentItems, outdentAtCaret, outdentItems } from './lists.js';
import { caretAt, caretInside, isRangeEmpty, startNode } from './selection.js';
import { moveToCell } from './tables.js';
import { RETAGGABLE_TAGS } from './text-blocks.js';

function probe(block, range, toStart) {
    const result = block.ownerDocument.createRange();
    if (toStart) {
        result.setStart(block, 0);
        result.setEnd(range.startContainer, range.startOffset);
    } else {
        result.setStart(range.startContainer, range.startOffset);
        result.setEnd(block, block.childNodes.length);
    }
    return result;
}

function atBlockStart(block, range) {
    const before = probe(block, range, true);
    return isRangeEmpty(before) && !before.cloneContents().querySelector(ATOMIC_SELECTOR);
}

function listItemOf(block) {
    if (block.localName === 'li') {
        return block;
    }
    const parent = block.parentElement;
    return parent?.localName === 'li' && RETAGGABLE_TAGS.has(block.localName) ? parent : null;
}

function adjacentTextBlock(content, block, backwards) {
    const walker = content.ownerDocument.createTreeWalker(content, NodeFilter.SHOW_ELEMENT, {
        acceptNode: (node) => (isTextBlock(node) && !node.contains(block) ? NodeFilter.FILTER_ACCEPT : NodeFilter.FILTER_SKIP)
    });
    walker.currentNode = block;
    return backwards ? walker.previousNode() : walker.nextNode();
}

function splitTextBlock(content, range, block) {
    let tail = splitAt(block, range.startContainer, range.startOffset);
    ensurePlaceholder(block);
    ensurePlaceholder(tail);
    if (HEADING_TAGS.has(tail.localName) && !hasOwnContent(tail)) {
        tail = retag(tail, 'p');
    }
    caretInside(content, tail, false);
}

function exitPre(content, pre) {
    pre.normalize();
    const last = pre.lastChild;
    if (isText(last)) {
        last.data = last.data.replace(/\n+$/, '');
    }
    ensurePlaceholder(pre);
    const paragraph = emptyParagraph(content.ownerDocument);
    pre.after(paragraph);
    caretInside(content, paragraph, false);
}

function enterInPre(content, range, pre) {
    const after = probe(pre, range, false).toString();
    const before = probe(pre, range, true).toString();
    if (/^\n*$/.test(after) && before.endsWith('\n')) {
        exitPre(content, pre);
        return;
    }
    insertNewline(content, range, pre);
}

export function handleEnter(content, range) {
    const collapsed = deleteSelection(content, range);
    let block = blockOf(startNode(collapsed), content);
    if (!block) {
        return insertBreak(content, collapsed);
    }
    const item = listItemOf(block);
    if (item) {
        enterInListItem(content, collapsed, item);
        return true;
    }
    if (block.localName === 'pre') {
        enterInPre(content, collapsed, block);
        return true;
    }
    if (CELL_TAGS.has(block.localName)) {
        return insertBreak(content, collapsed);
    }
    if (!RETAGGABLE_TAGS.has(block.localName)) {
        const point = [collapsed.startContainer, collapsed.startOffset];
        wrapInlineRuns(block);
        collapsed.setStart(...point);
        collapsed.collapse(true);
        block = blockOf(startNode(collapsed), content);
    }
    splitTextBlock(content, collapsed, block);
    return true;
}

function caretAtEnd(block) {
    removeTrailingBreak(block);
    const last = block.lastChild;
    return isText(last) ? [last, last.length] : [block, block.childNodes.length];
}

function removeRule(rule) {
    rule.remove();
    return true;
}

function liftOutOfQuote(content, block) {
    const quote = block.parentElement;
    quote.before(block);
    if (quote.children.length === 0) {
        quote.remove();
    }
    caretInside(content, block, false);
    return true;
}

function mergeInto(content, target, source) {
    const [node, offset] = caretAtEnd(target);
    mergeBlocks(target, source, content);
    caretAt(content, node, Math.min(offset, maxOffset(node)));
    return true;
}

export function handleBackspace(content, range) {
    const block = range.collapsed ? blockOf(startNode(range), content) : null;
    if (!block || !atBlockStart(block, range)) {
        return false;
    }
    const item = listItemOf(block);
    if (item && atBlockStart(item, range)) {
        outdentAtCaret(content, item);
        return true;
    }
    if (CELL_TAGS.has(block.localName)) {
        return true;
    }
    if (block.parentElement?.localName === 'blockquote' && block.parentElement !== content && !block.previousElementSibling) {
        return liftOutOfQuote(content, block);
    }
    if (block.previousElementSibling?.localName === 'hr') {
        return removeRule(block.previousElementSibling);
    }
    const previous = adjacentTextBlock(content, block, true);
    if (!previous) {
        return false;
    }
    if (cellOf(previous, content) !== cellOf(block, content)) {
        if (!cellOf(block, content) && !hasVisibleContent(block)) {
            removeWithEmptyAncestors(block, content);
            caretInside(content, previous, true);
        }
        return true;
    }
    return mergeInto(content, previous, block);
}

export function handleDelete(content, range) {
    const block = range.collapsed ? blockOf(startNode(range), content) : null;
    if (!block || !restOfBlockEmpty(block, range.startContainer, range.startOffset)) {
        return false;
    }
    if (CELL_TAGS.has(block.localName)) {
        return true;
    }
    if (block.nextElementSibling?.localName === 'hr') {
        return removeRule(block.nextElementSibling);
    }
    const next = adjacentTextBlock(content, block, false);
    if (!next) {
        return false;
    }
    if (cellOf(next, content) !== cellOf(block, content)) {
        if (!cellOf(block, content) && !hasVisibleContent(block)) {
            removeWithEmptyAncestors(block, content);
            caretInside(content, next, false);
        }
        return true;
    }
    return mergeInto(content, block, next);
}

export function handleTab(content, range, backwards) {
    const cell = cellOf(range.startContainer, content);
    const item = closestWithin(range.startContainer, 'li', content);
    if (item && (!cell || cell.contains(item))) {
        return backwards ? outdentItems(content, range) : indentItems(content, range);
    }
    return cell ? moveToCell(content, cell, backwards) : false;
}
