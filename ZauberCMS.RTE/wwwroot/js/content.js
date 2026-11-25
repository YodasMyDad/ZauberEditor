import { tidyBreaks } from './breaks.js';
import {
    LAYOUT_TAGS, emptyParagraph, hasOwnContent, hasVisibleContent, hasVisibleText, isBlock, isElement, isText, isTextBlock
} from './dom.js';
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

const isEmptyTextBlock = (node) => isTextBlock(node) && !hasOwnContent(node);

function absorbRun(content, node) {
    const paragraph = wrapRun(content, node);
    const empty = [paragraph.nextElementSibling, paragraph.previousElementSibling].find(isEmptyTextBlock);
    if (!empty) {
        return paragraph;
    }
    empty.replaceChildren(...paragraph.childNodes);
    paragraph.remove();
    return empty;
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
    const paragraph = preserveSelection(content, () => absorbRun(content, top));
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
    tidyBreaks(fragment);
    content.replaceChildren(fragment);
    normalizeRoot(content);
}

export function isContentEmpty(content) {
    if (content.childElementCount > 1) {
        return false;
    }
    const first = content.firstElementChild;
    return !first || (first.localName === 'p' && !hasVisibleContent(first) && !hasVisibleText(content.textContent));
}

function hasContentOutside(content, range, atStart) {
    const outside = content.ownerDocument.createRange();
    outside.selectNodeContents(content);
    if (atStart) {
        outside.setEnd(range.startContainer, range.startOffset);
    } else {
        outside.setStart(range.endContainer, range.endOffset);
    }
    const fragment = outside.cloneContents();
    return hasVisibleContent(fragment) || Boolean(fragment.querySelector('table, hr'));
}

export const coversAllContent = (content, range) => !range.collapsed
    && !hasContentOutside(content, range, true) && !hasContentOutside(content, range, false);

export function resetEmptyContent(content) {
    const paragraph = emptyParagraph(content.ownerDocument);
    content.replaceChildren(paragraph);
    caretInside(content, paragraph, false);
}

export function refreshEmptyState(content) {
    content.classList.toggle(EMPTY_CLASS, isContentEmpty(content));
}
