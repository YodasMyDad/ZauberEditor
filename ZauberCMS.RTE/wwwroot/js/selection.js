import { ATOMIC_SELECTOR, ZWSP, isAtomic, isElement, isText, maxOffset, nodeIndex } from './dom.js';

export function rangeIn(content) {
    const selection = content.ownerDocument.getSelection();
    if (!selection?.rangeCount) {
        return null;
    }
    const range = selection.getRangeAt(0);
    return content.contains(range.commonAncestorContainer) ? range : null;
}

export function containsRange(content, range) {
    return Boolean(range) && content.contains(range.startContainer) && content.contains(range.endContainer);
}

export function createRange(content) {
    return content.ownerDocument.createRange();
}

export function applyRange(content, range) {
    const selection = content.ownerDocument.getSelection();
    selection.removeAllRanges();
    selection.addRange(range);
    return range;
}

export function caretAt(content, node, offset) {
    const range = createRange(content);
    range.setStart(node, offset);
    range.collapse(true);
    return applyRange(content, range);
}

export function edgePosition(element, atEnd) {
    if (!element.hasChildNodes()) {
        return [element, 0];
    }
    let node = element;
    while (node.hasChildNodes()) {
        const child = atEnd ? node.lastChild : node.firstChild;
        if (isAtomic(child)) {
            const before = !atEnd || child.localName === 'br';
            return [node, nodeIndex(child) + (before ? 0 : 1)];
        }
        node = child;
    }
    return isText(node) ? [node, atEnd ? node.length : 0] : [node.parentNode, nodeIndex(node) + (atEnd ? 1 : 0)];
}

export function caretInside(content, element, atEnd) {
    const [node, offset] = edgePosition(element, atEnd);
    return caretAt(content, node, offset);
}

export function nodeRange(content, node) {
    const range = createRange(content);
    range.selectNode(node);
    return range;
}

export function selectedElement(range) {
    const { startContainer, startOffset, endContainer, endOffset } = range;
    if (startContainer !== endContainer || !isElement(startContainer) || endOffset - startOffset !== 1) {
        return null;
    }
    const child = startContainer.childNodes[startOffset];
    return isElement(child) ? child : null;
}

export function startNode(range) {
    const { startContainer, startOffset } = range;
    if (!isElement(startContainer) || !startContainer.hasChildNodes()) {
        return startContainer;
    }
    return startContainer.childNodes[Math.min(startOffset, startContainer.childNodes.length - 1)];
}

function leafPoint(node, offset, atEnd) {
    const child = isElement(node) ? node.childNodes[atEnd ? offset - 1 : offset] : null;
    if (isText(child)) {
        return [child, atEnd ? child.length : 0];
    }
    return isElement(child) && !isAtomic(child) && child.hasChildNodes() ? edgePosition(child, atEnd) : [node, offset];
}

function savedPoint(node, offset, atEnd, descend) {
    const [leaf, leafOffset] = descend ? leafPoint(node, offset, atEnd) : [node, offset];
    return [leaf, leafOffset, isElement(leaf) ? leaf.childNodes[leafOffset] ?? leaf.lastChild : null];
}

function restoredPoint(content, [node, offset, child]) {
    if (content.contains(node)) {
        return [node, Math.min(offset, maxOffset(node))];
    }
    return child && content.contains(child) ? [child.parentNode, nodeIndex(child)] : null;
}

export function preserveSelection(content, action) {
    const range = rangeIn(content);
    const saved = range && [
        savedPoint(range.startContainer, range.startOffset, false, !range.collapsed),
        savedPoint(range.endContainer, range.endOffset, true, !range.collapsed)
    ];
    const result = action();
    const start = saved && restoredPoint(content, saved[0]);
    const end = saved && restoredPoint(content, saved[1]);
    if (start && end) {
        const restored = createRange(content);
        restored.setStart(...start);
        restored.setEnd(...end);
        applyRange(content, restored);
    }
    return result;
}

export function isRangeEmpty(range) {
    return range.toString().replaceAll(ZWSP, '') === '';
}

function elementPath(content, element) {
    const path = [];
    for (let current = element; current !== content; current = current.parentElement) {
        path.unshift(Array.prototype.indexOf.call(current.parentElement.children, current));
    }
    return path;
}

export function unitsBefore(element, node, offset) {
    const range = element.ownerDocument.createRange();
    range.setStart(element, 0);
    range.setEnd(node, offset);
    return range.toString().length + range.cloneContents().querySelectorAll(ATOMIC_SELECTOR).length;
}

function pointMark(content, node, offset) {
    const element = isElement(node) ? node : node.parentElement;
    return { path: elementPath(content, element), units: unitsBefore(element, node, offset) };
}

export function pointAt(element, units) {
    const walker = element.ownerDocument.createTreeWalker(element, NodeFilter.SHOW_ELEMENT | NodeFilter.SHOW_TEXT, {
        acceptNode: (node) => (isText(node) || isAtomic(node) ? NodeFilter.FILTER_ACCEPT : NodeFilter.FILTER_SKIP)
    });
    let start = 0;
    let last = null;
    for (let node = walker.nextNode(); node; node = walker.nextNode()) {
        const length = isText(node) ? node.length : 1;
        if (units < start + length) {
            return isText(node) ? [node, units - start] : [node.parentNode, nodeIndex(node)];
        }
        start += length;
        last = node;
    }
    return isText(last) && last.parentNode === element ? [last, last.length] : [element, element.childNodes.length];
}

function resolvePoint(content, mark) {
    let element = content;
    for (const index of mark.path) {
        const child = element.children[index];
        if (!child) {
            return [element, element.childNodes.length];
        }
        element = child;
    }
    return pointAt(element, mark.units);
}

export function bookmark(content, range) {
    if (!containsRange(content, range)) {
        return null;
    }
    return {
        start: pointMark(content, range.startContainer, range.startOffset),
        end: pointMark(content, range.endContainer, range.endOffset)
    };
}

export function resolveBookmark(content, mark) {
    if (!mark) {
        return null;
    }
    const range = createRange(content);
    range.setStart(...resolvePoint(content, mark.start));
    range.setEnd(...resolvePoint(content, mark.end));
    return range;
}
