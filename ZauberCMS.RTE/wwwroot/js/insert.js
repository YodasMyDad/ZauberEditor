import {
    CELL_TAGS, MEDIA_SELECTOR, blockOf, cellOf, createBreak, ensurePlaceholder, hasVisibleContent, isBlock, isList, isText,
    maxOffset, mergeBlocks, removeEmptyInlines, removeTrailingBreak, splitAt, wrapInlineRuns
} from './dom.js';
import { applyRange, caretAt, createRange, edgePosition, isRangeEmpty, pointAt, startNode, unitsBefore } from './selection.js';
import { RETAGGABLE_TAGS } from './text-blocks.js';

export function deleteSelection(content, range) {
    if (range.collapsed) {
        return range;
    }
    const { startContainer: node, startOffset: offset } = range;
    const startBlock = blockOf(node, content);
    const endBlock = blockOf(range.endContainer, content);
    const sameCell = cellOf(node, content) === cellOf(range.endContainer, content);
    range.deleteContents();
    const units = startBlock && unitsBefore(startBlock, node, offset);
    const separate = startBlock && endBlock && startBlock !== endBlock && endBlock.isConnected
        && !startBlock.contains(endBlock) && !endBlock.contains(startBlock);
    if (separate && sameCell) {
        mergeBlocks(startBlock, endBlock, content);
    }
    if (startBlock?.isConnected) {
        ensurePlaceholder(startBlock);
    }
    if (content.contains(node)) {
        range.setStart(node, Math.min(offset, maxOffset(node)));
    } else if (startBlock?.isConnected) {
        range.setStart(...pointAt(startBlock, units));
    }
    range.collapse(true);
    return range;
}

export function restOfBlockEmpty(block, node, offset) {
    const probe = block.ownerDocument.createRange();
    probe.setStart(node, offset);
    probe.setEnd(block, block.childNodes.length);
    if (!isRangeEmpty(probe)) {
        return false;
    }
    const rest = probe.cloneContents();
    const breaks = rest.querySelectorAll('br').length;
    return !rest.querySelector(MEDIA_SELECTOR)
        && breaks <= (block.lastChild?.localName === 'br' ? 1 : 0);
}

export function insertBreak(content, range) {
    const collapsed = deleteSelection(content, range);
    const block = blockOf(startNode(collapsed), content);
    const doc = content.ownerDocument;
    if (block?.localName === 'pre') {
        insertNewline(content, collapsed, block);
        return true;
    }
    const br = createBreak(doc);
    collapsed.insertNode(br);
    if (block && restOfBlockEmpty(block, br.parentNode, Array.prototype.indexOf.call(br.parentNode.childNodes, br) + 1)) {
        br.after(createBreak(doc));
    }
    const caret = createRange(content);
    caret.setStartAfter(br);
    caret.collapse(true);
    applyRange(content, caret);
    return true;
}

export function insertNewline(content, range, pre) {
    const atEnd = restOfBlockEmpty(pre, range.startContainer, range.startOffset);
    const text = content.ownerDocument.createTextNode(atEnd && !pre.textContent.endsWith('\n') ? '\n\n' : '\n');
    range.insertNode(text);
    caretAt(content, text, 1);
}

export function textFragment(doc, text) {
    const fragment = doc.createDocumentFragment();
    const normalized = String(text ?? '').replace(/\r\n?/g, '\n');
    if (!normalized.includes('\n')) {
        fragment.append(doc.createTextNode(normalized));
        return fragment;
    }
    for (const paragraphText of normalized.split(/\n{2,}/)) {
        const paragraph = doc.createElement('p');
        paragraphText.split('\n').forEach((line, index) => {
            if (index > 0) {
                paragraph.append(createBreak(doc));
            }
            if (line) {
                paragraph.append(doc.createTextNode(line));
            }
        });
        ensurePlaceholder(paragraph);
        fragment.append(paragraph);
    }
    return fragment;
}

function placeCaretAfter(content, node) {
    if (isText(node)) {
        caretAt(content, node, node.length);
        return;
    }
    const caret = createRange(content);
    caret.setStartAfter(node);
    caret.collapse(true);
    applyRange(content, caret);
}

function insertInline(content, range, fragment) {
    const block = blockOf(startNode(range), content);
    const placeholder = block && !hasVisibleContent(block);
    const last = fragment.lastChild;
    if (!block) {
        const paragraph = content.ownerDocument.createElement('p');
        paragraph.append(fragment);
        range.insertNode(paragraph);
    } else {
        range.insertNode(fragment);
        if (placeholder && block.lastChild !== last) {
            removeTrailingBreak(block);
        }
    }
    placeCaretAfter(content, last);
}

function toItem(node) {
    if (node.localName === 'li') {
        return node;
    }
    const item = node.ownerDocument.createElement('li');
    if (RETAGGABLE_TAGS.has(node.localName)) {
        item.append(...node.childNodes);
    } else {
        item.append(node);
    }
    return item;
}

function cleanPieces(...pieces) {
    for (const piece of pieces) {
        removeEmptyInlines(piece);
        if (!hasVisibleContent(piece)) {
            piece.remove();
        }
    }
}

const isParagraph = (node) => node.localName === 'p';

function insertIntoList(content, range, item, nodes) {
    const items = nodes.flatMap((node) => (isList(node) ? [...node.children] : [toItem(node)]));
    const tail = splitAt(item, range.startContainer, range.startOffset);
    if (isParagraph(nodes[0])) {
        mergeBlocks(item, items.shift(), content);
    }
    item.after(...items);
    const last = items.at(-1) ?? item;
    const caret = edgePosition(last, true);
    if (isParagraph(nodes.at(-1)) && hasVisibleContent(tail)) {
        mergeBlocks(last, tail, content);
    }
    cleanPieces(...(last === item ? [tail] : [item, tail]));
    caretAt(content, ...caret);
}

function insertBlocks(content, range, block, nodes) {
    const tail = splitAt(block, range.startContainer, range.startOffset);
    block.after(...nodes);
    let first = nodes[0];
    let last = nodes.at(-1);
    if (first.localName === 'p' && hasVisibleContent(block)) {
        mergeBlocks(block, first, content);
        last = last === first ? block : last;
        first = block;
    }
    const caret = edgePosition(last, true);
    if ((last === block || last.localName === 'p') && hasVisibleContent(tail)) {
        mergeBlocks(last, tail, content);
    }
    if (first === block) {
        cleanPieces(tail);
    } else {
        cleanPieces(block, tail);
    }
    caretAt(content, ...caret);
}

export function replaceSelection(content, range, text) {
    if (range.collapsed || blockOf(range.startContainer, content) === blockOf(range.endContainer, content)) {
        return false;
    }
    if (text) {
        return insertFragment(content, range, textFragment(content.ownerDocument, text));
    }
    const collapsed = deleteSelection(content, range);
    caretAt(content, collapsed.startContainer, collapsed.startOffset);
    return true;
}

export function insertFragment(content, range, fragment) {
    const collapsed = deleteSelection(content, range);
    if (!fragment.hasChildNodes()) {
        return false;
    }
    if (!Array.prototype.some.call(fragment.childNodes, isBlock)) {
        insertInline(content, collapsed, fragment);
        return true;
    }
    wrapInlineRuns(fragment);
    let block = blockOf(startNode(collapsed), content);
    if (block?.localName === 'pre') {
        insertInline(content, collapsed, content.ownerDocument.createTextNode(fragment.textContent));
        return true;
    }
    const nodes = [...fragment.children];
    if (block?.localName === 'li') {
        insertIntoList(content, collapsed, block, nodes);
        return true;
    }
    if (block && (CELL_TAGS.has(block.localName) || !RETAGGABLE_TAGS.has(block.localName))) {
        const point = [collapsed.startContainer, collapsed.startOffset];
        wrapInlineRuns(block);
        collapsed.setStart(...point);
        collapsed.collapse(true);
        block = blockOf(startNode(collapsed), content);
    }
    if (!block || !RETAGGABLE_TAGS.has(block.localName)) {
        collapsed.insertNode(fragment);
        placeCaretAfter(content, nodes.at(-1));
        return true;
    }
    insertBlocks(content, collapsed, block, nodes);
    return true;
}
