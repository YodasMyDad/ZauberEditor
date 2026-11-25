import {
    CELL_TAGS, closestWithin, emptyParagraph, ensurePlaceholder, hasVisibleContent, isBlock, isList, removeEmptyInlines,
    removeWithEmptyAncestors, retag, setAttributes, splitAt, unique, wrapInlineRuns
} from './dom.js';
import { caretInside, preserveSelection } from './selection.js';
import { asTextBlock, textBlocksInRange } from './text-blocks.js';

const itemOf = (block, content) => (block.localName === 'li' ? block : closestWithin(block, 'li', content));

function cloneList(list) {
    const copy = list.cloneNode(false);
    copy.removeAttribute('id');
    return copy;
}

function removeIfEmpty(list) {
    if (list.children.length === 0) {
        list.remove();
    }
}

function mergeAdjacentLists(list) {
    const previous = list.previousElementSibling;
    let target = list;
    if (previous?.localName === list.localName) {
        previous.append(...list.childNodes);
        list.remove();
        target = previous;
    }
    const next = target.nextElementSibling;
    if (next?.localName === target.localName) {
        target.append(...next.childNodes);
        next.remove();
    }
}

function nestFollowing(item) {
    const list = item.parentElement;
    const following = [];
    for (let sibling = item.nextElementSibling; sibling; sibling = sibling.nextElementSibling) {
        following.push(sibling);
    }
    if (following.length > 0) {
        const nested = cloneList(list);
        nested.append(...following);
        item.append(nested);
    }
}

function liftItem(item) {
    const list = item.parentElement;
    const outer = list.parentElement;
    nestFollowing(item);
    if (outer?.localName === 'li') {
        outer.after(item);
        removeIfEmpty(list);
        return item;
    }
    wrapInlineRuns(item);
    if (!item.firstElementChild || isList(item.firstElementChild)) {
        item.prepend(emptyParagraph(item.ownerDocument));
    }
    const blocks = [...item.children];
    list.after(...blocks);
    item.remove();
    removeIfEmpty(list);
    blocks.filter(isList).forEach(mergeAdjacentLists);
    ensurePlaceholder(blocks[0]);
    return blocks[0];
}

export function liftToBlock(item, type, attributes) {
    let node = item;
    while (node.localName === 'li' && isList(node.parentElement)) {
        node = liftItem(node);
    }
    const block = node.localName === type || node.localName !== 'p' ? node : retag(node, type);
    return setAttributes(block, attributes);
}

function sinkItem(item) {
    const previous = item.previousElementSibling;
    if (previous?.localName !== 'li') {
        return;
    }
    let nested = previous.lastElementChild;
    if (!isList(nested)) {
        nested = cloneList(item.parentElement);
        previous.append(nested);
    }
    nested.append(item);
}

function selectedItems(content, range) {
    const items = unique(textBlocksInRange(content, range).map((block) => itemOf(block, content)).filter(Boolean));
    return items.filter((item) => !items.some((other) => other !== item && other.contains(item)));
}

export function indentItems(content, range) {
    const items = selectedItems(content, range);
    preserveSelection(content, () => items.forEach(sinkItem));
    return items.length > 0;
}

export function outdentItems(content, range) {
    const items = selectedItems(content, range);
    preserveSelection(content, () => items.forEach(liftItem));
    return items.length > 0;
}

function listify(content, blocks, type) {
    const groups = [];
    for (const block of blocks.map((candidate) => (CELL_TAGS.has(candidate.localName) ? asTextBlock(candidate) : candidate))) {
        const group = groups.at(-1);
        if (group && group.at(-1).nextElementSibling === block) {
            group.push(block);
        } else {
            groups.push([block]);
        }
    }
    for (const group of groups) {
        const list = content.ownerDocument.createElement(type);
        group[0].before(list);
        for (const block of group) {
            const item = content.ownerDocument.createElement('li');
            const style = block.getAttribute('style');
            if (style) {
                item.setAttribute('style', style);
            }
            item.append(...block.childNodes);
            list.append(item);
            block.remove();
        }
        mergeAdjacentLists(list);
    }
}

export function toggleList(content, range, type) {
    const blocks = textBlocksInRange(content, range);
    if (blocks.length === 0) {
        return false;
    }
    const items = blocks.map((block) => itemOf(block, content));
    if (items.every((item) => item?.parentElement.localName === type)) {
        preserveSelection(content, () => unique(items).forEach((item) => liftToBlock(item, 'p', [])));
        return true;
    }
    preserveSelection(content, () => {
        for (const list of unique(items.filter(Boolean).map((item) => item.parentElement))) {
            if (list.localName !== type) {
                mergeAdjacentLists(retag(list, type));
            }
        }
        listify(content, items.flatMap((item, index) => (item ? [] : [blocks[index]])), type);
    });
    return true;
}

const isEmptyItem = (item) => Array.prototype.every.call(item.childNodes, (child) => isList(child) || !hasVisibleContent(child));

function tidyItem(item) {
    removeEmptyInlines(item);
    const blocks = [...item.children].filter((child) => isBlock(child) && !isList(child));
    if (blocks.length === 0) {
        ensurePlaceholder(item);
        return;
    }
    const empty = blocks.filter((block) => !hasVisibleContent(block));
    empty.slice(empty.length === blocks.length ? 1 : 0).forEach((block) => block.remove());
    ensurePlaceholder(blocks.find((block) => block.isConnected));
}

export function enterInListItem(content, range, item) {
    if (isEmptyItem(item)) {
        caretInside(content, liftItem(item), false);
        return;
    }
    const tail = splitAt(item, range.startContainer, range.startOffset);
    tidyItem(item);
    tidyItem(tail);
    caretInside(content, tail, false);
}

function outermostList(item, content) {
    let list = item.parentElement;
    let parent = list.parentElement;
    while (parent && parent !== content && !CELL_TAGS.has(parent.localName)) {
        if (isList(parent)) {
            list = parent;
        }
        parent = parent.parentElement;
    }
    return list;
}

function pruneEdgeItems(list, atEnd) {
    for (let items = list.querySelectorAll('li'); items.length > 0; items = list.querySelectorAll('li')) {
        const item = atEnd ? items[items.length - 1] : items[0];
        if (hasVisibleContent(item)) {
            return;
        }
        removeWithEmptyAncestors(item, list.parentNode);
    }
    list.remove();
}

export function placeBesideList(content, range, item, nodes) {
    const list = outermostList(item, content);
    const tail = splitAt(list, range.startContainer, range.startOffset);
    list.after(...nodes);
    pruneEdgeItems(list, true);
    pruneEdgeItems(tail, false);
    return tail;
}

export function outdentAtCaret(content, item) {
    const lifted = liftItem(item);
    caretInside(content, lifted, false);
}
