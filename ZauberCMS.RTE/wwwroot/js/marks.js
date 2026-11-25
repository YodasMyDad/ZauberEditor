import { markFamily } from './config.js';
import {
    CONTAINER_TAGS, FORMAT_TAGS, closestWithin, elementOf, hasAnyContent, hasBlockChild, hasVisibleText, isBlock, isElement,
    isText, removeEmptyInlines, setAttributes, unwrapElement
} from './dom.js';
import { applyRange, caretAt, createRange, preserveSelection, selectedElement } from './selection.js';

const WORD_CHARACTER = /[\p{L}\p{N}_'’-]/u;
const KEPT_ATTRIBUTES = new Set(['class', 'id', 'style']);

function isolateBoundaries(range) {
    const { startContainer, startOffset, endContainer, endOffset } = range;
    if (isText(endContainer) && endOffset > 0 && endOffset < endContainer.length) {
        endContainer.splitText(endOffset);
    }
    if (isText(startContainer) && startOffset > 0 && startOffset < startContainer.length) {
        range.setStart(startContainer.splitText(startOffset), 0);
    }
}

function isEditableText(text, range) {
    const parent = text.parentElement;
    if (!parent || text.length === 0 || CONTAINER_TAGS.has(parent.localName)) {
        return false;
    }
    if (!hasVisibleText(text.data) && isBlock(parent) && hasBlockChild(parent)) {
        return false;
    }
    return range.intersectsNode(text)
        && !(text === range.startContainer && range.startOffset === text.length)
        && !(text === range.endContainer && range.endOffset === 0);
}

function selectedTexts(range) {
    isolateBoundaries(range);
    const root = range.commonAncestorContainer;
    if (isText(root)) {
        return root.length > 0 && range.startOffset < range.endOffset ? [root] : [];
    }
    const texts = [];
    const walker = root.ownerDocument.createTreeWalker(root, NodeFilter.SHOW_TEXT);
    for (let text = walker.nextNode(); text; text = walker.nextNode()) {
        if (isEditableText(text, range)) {
            texts.push(text);
        }
    }
    return texts;
}

function selectTexts(content, texts) {
    const range = createRange(content);
    range.setStart(texts[0], 0);
    range.setEnd(texts.at(-1), texts.at(-1).length);
    applyRange(content, range);
}

function markOf(node, tags, content) {
    for (let element = elementOf(node); element && element !== content && !isBlock(element); element = element.parentElement) {
        if (tags.has(element.localName)) {
            return element;
        }
    }
    return null;
}

function detach(element, node, before) {
    const range = element.ownerDocument.createRange();
    if (before) {
        range.setStart(element, 0);
        range.setEndBefore(node);
    } else {
        range.setStartAfter(node);
        range.setEnd(element, element.childNodes.length);
    }
    if (range.collapsed) {
        return;
    }
    const piece = element.cloneNode(false);
    piece.removeAttribute('id');
    piece.append(range.extractContents());
    removeEmptyInlines(piece);
    if (hasAnyContent(piece)) {
        element[before ? 'before' : 'after'](piece);
    }
}

function removeFrom(node, tags, content) {
    for (let mark = markOf(node, tags, content); mark; mark = markOf(node, tags, content)) {
        detach(mark, node, true);
        detach(mark, node, false);
        unwrapElement(mark);
    }
}

const sameShape = (a, b) => isElement(a) && a.localName === b.localName && a.attributes.length === b.attributes.length
    && [...b.attributes].every(({ name, value }) => a.getAttribute(name) === value);

function mergeSiblings(element) {
    let target = element;
    const previous = target.previousSibling;
    if (sameShape(previous, target)) {
        previous.append(...target.childNodes);
        target.remove();
        target = previous;
    }
    const next = target.nextSibling;
    if (sameShape(next, target)) {
        target.append(...next.childNodes);
        next.remove();
    }
}

function wrapNode(node, tag, attributes) {
    let target = node;
    while (target.parentElement && !isBlock(target.parentElement) && target.parentElement.childNodes.length === 1) {
        target = target.parentElement;
    }
    const wrapper = setAttributes(node.ownerDocument.createElement(tag), attributes);
    target.before(wrapper);
    wrapper.append(target);
    mergeSiblings(wrapper);
}

function applyFamily(content, texts, family) {
    const meaningful = texts.filter((text) => text.data.trim() !== '');
    const marked = (meaningful.length > 0 ? meaningful : texts).every((text) => markOf(text, family.tags, content));
    for (const text of texts) {
        if (marked) {
            removeFrom(text, family.tags, content);
        } else if (!markOf(text, family.tags, content)) {
            wrapNode(text, family.primary, []);
        }
    }
}

function wordAround(range) {
    const { startContainer: text, startOffset: offset } = range;
    if (!range.collapsed || !isText(text)) {
        return null;
    }
    const data = text.data;
    if (!WORD_CHARACTER.test(data[offset - 1] ?? '') || !WORD_CHARACTER.test(data[offset] ?? '')) {
        return null;
    }
    let start = offset;
    let end = offset;
    while (start > 0 && WORD_CHARACTER.test(data[start - 1])) {
        start--;
    }
    while (end < data.length && WORD_CHARACTER.test(data[end])) {
        end++;
    }
    return { text, start, end, offset };
}

function toggleWord(content, word, family) {
    const range = createRange(content);
    range.setStart(word.text, word.start);
    range.setEnd(word.text, word.end);
    const texts = selectedTexts(range);
    applyFamily(content, texts, family);
    caretAt(content, texts[0], word.offset - word.start);
}

export function toggleMark(content, range, config, tag, caretMarks) {
    const family = markFamily(config, tag);
    if (range.collapsed) {
        const word = wordAround(range);
        if (word) {
            toggleWord(content, word, family);
            return true;
        }
        const mark = markOf(range.startContainer, family.tags, content);
        return mark ? caretMarks.exit(range, mark) : caretMarks.enter(range, family.primary);
    }
    const texts = selectedTexts(range);
    if (texts.length === 0) {
        return false;
    }
    applyFamily(content, texts, family);
    selectTexts(content, texts);
    return true;
}

function enclosing(content, range, tag) {
    const selected = selectedElement(range);
    if (selected?.localName === tag) {
        return selected;
    }
    const found = closestWithin(range.startContainer, tag, content);
    return found?.contains(range.endContainer) ? found : null;
}

function replaceAttributes(element, attributes) {
    for (const { name } of [...element.attributes]) {
        if (!KEPT_ATTRIBUTES.has(name)) {
            element.removeAttribute(name);
        }
    }
    setAttributes(element, attributes);
}

export function wrapInline(content, range, tag, attributes) {
    const existing = enclosing(content, range, tag);
    if (existing) {
        replaceAttributes(existing, attributes);
        return true;
    }
    if (range.collapsed) {
        return false;
    }
    const texts = selectedTexts(range);
    if (texts.length === 0) {
        return false;
    }
    const tags = new Set([tag]);
    for (const text of texts) {
        removeFrom(text, tags, content);
        wrapNode(text, tag, attributes);
    }
    selectTexts(content, texts);
    return true;
}

export function unwrapInline(content, range, tags) {
    const selected = selectedElement(range);
    const whole = tags.has(selected?.localName) ? selected : null;
    if (whole || range.collapsed) {
        const target = whole ?? markOf(range.startContainer, tags, content);
        if (!target) {
            return false;
        }
        preserveSelection(content, () => unwrapElement(target));
        return true;
    }
    const texts = selectedTexts(range);
    if (texts.length === 0) {
        return false;
    }
    texts.forEach((text) => removeFrom(text, tags, content));
    selectTexts(content, texts);
    return true;
}

export function clearInline(content, range) {
    const texts = selectedTexts(range);
    if (texts.length === 0) {
        return false;
    }
    texts.forEach((text) => removeFrom(text, FORMAT_TAGS, content));
    selectTexts(content, texts);
    return true;
}
