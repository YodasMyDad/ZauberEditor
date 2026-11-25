import { LAYOUT_TAGS, emptyParagraph, hasVisibleContent, hasVisibleText, isBlock, isElement, isText, isTextBlock } from './dom.js';
import { caretInside, preserveSelection, rangeIn } from './selection.js';

const EMPTY_CLASS = 'rte-empty';
const LAYOUT_SELECTOR = [...LAYOUT_TAGS].join(', ');

function wrapRun(content, node) {
    let first = node;
    while (first.previousSibling && !isBlock(first.previousSibling)) {
        first = first.previousSibling;
    }
    const paragraph = content.ownerDocument.createElement('p');
    first.before(paragraph);
    while (paragraph.nextSibling && !isBlock(paragraph.nextSibling)) {
        paragraph.append(paragraph.nextSibling);
    }
    return paragraph;
}

export function normalizeRoot(content) {
    for (const node of [...content.childNodes]) {
        if (!node.isConnected || node.parentNode !== content || isBlock(node)) {
            continue;
        }
        if (isText(node) ? !hasVisibleText(node.data) : !isElement(node)) {
            node.remove();
            continue;
        }
        wrapRun(content, node);
    }
    if (!content.firstChild) {
        content.append(emptyParagraph(content.ownerDocument));
    }
}

function topLevelNode(content, range) {
    const { startContainer, startOffset } = range;
    if (startContainer === content) {
        return content.childNodes[startOffset] ?? content.childNodes[startOffset - 1] ?? null;
    }
    let node = startContainer;
    while (node.parentNode !== content) {
        node = node.parentNode;
        if (!node) {
            return null;
        }
    }
    return node;
}

export function normalizeAtCaret(content) {
    if (!content.firstChild) {
        const paragraph = emptyParagraph(content.ownerDocument);
        content.append(paragraph);
        caretInside(content, paragraph, false);
        return;
    }
    const range = rangeIn(content);
    const top = range && topLevelNode(content, range);
    if (!top || isBlock(top) || !(isElement(top) || isText(top))) {
        return;
    }
    const atRoot = range.startContainer === content;
    const paragraph = preserveSelection(content, () => wrapRun(content, top));
    if (atRoot) {
        caretInside(content, paragraph, true);
    }
}

function removeLayoutWhitespace(fragment) {
    for (const element of fragment.querySelectorAll(LAYOUT_SELECTOR)) {
        if (!isTextBlock(element)) {
            [...element.childNodes].filter((child) => isText(child) && !hasVisibleText(child.data)).forEach((child) => child.remove());
        }
    }
}

export function replaceContent(content, fragment) {
    removeLayoutWhitespace(fragment);
    content.replaceChildren(fragment);
    normalizeRoot(content);
}

function isContentEmpty(content) {
    if (content.childElementCount > 1) {
        return false;
    }
    const first = content.firstElementChild;
    return !first || (first.localName === 'p' && !hasVisibleContent(first) && !hasVisibleText(content.textContent));
}

export function refreshEmptyState(content) {
    content.classList.toggle(EMPTY_CLASS, isContentEmpty(content));
}
