export const ZWSP = '\u200B';

export const BLOCK_TAGS = new Set([
    'address', 'article', 'aside', 'blockquote', 'dd', 'details', 'div', 'dl', 'dt', 'figcaption', 'figure',
    'footer', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'header', 'hr', 'li', 'main', 'nav', 'ol', 'p', 'pre',
    'section', 'summary', 'table', 'tbody', 'td', 'tfoot', 'th', 'thead', 'tr', 'ul'
]);

export const CONTAINER_TAGS = new Set(['ul', 'ol', 'dl', 'table', 'thead', 'tbody', 'tfoot', 'tr', 'hr']);
export const LIST_TAGS = new Set(['ul', 'ol']);
export const CELL_TAGS = new Set(['td', 'th']);
export const HEADING_TAGS = new Set(['h1', 'h2', 'h3', 'h4', 'h5', 'h6']);
export const LAYOUT_TAGS = new Set(['ul', 'ol', 'table', 'thead', 'tbody', 'tfoot', 'tr', 'blockquote', 'figure', 'dl']);
export const FORMAT_TAGS = new Set([
    'abbr', 'b', 'cite', 'code', 'del', 'em', 'font', 'i', 'ins', 'kbd', 'mark', 'q', 's', 'small', 'span', 'strike',
    'strong', 'sub', 'sup', 'u'
]);
export const EMBED_SELECTOR = 'figure[data-embed]';
export const MEDIA_SELECTOR = `img, video, audio, iframe, embed, object, svg, canvas, hr, ${EMBED_SELECTOR}`;
export const ATOMIC_SELECTOR = `br, ${MEDIA_SELECTOR}`;

const ATOMIC_TAGS = new Set(['br', 'img', 'hr', 'video', 'audio', 'iframe', 'embed', 'object', 'svg', 'canvas']);
const PRUNABLE_TAGS = new Set(['ul', 'ol', 'li', 'blockquote', 'div']);
const INLINE_FORMAT_SELECTOR = ['a', ...FORMAT_TAGS].join(', ');
const VISIBLE_TEXT = new RegExp(`[^\t\n\r ${ZWSP}]`);

export const isElement = (node) => node?.nodeType === 1;
export const isText = (node) => node?.nodeType === 3;
export const isBlock = (node) => isElement(node) && BLOCK_TAGS.has(node.localName);
export const isList = (node) => isElement(node) && LIST_TAGS.has(node.localName);
export const isEmbed = (node) => isElement(node) && node.localName === 'figure' && node.hasAttribute('data-embed');
export const isAtomic = (node) => isElement(node) && (ATOMIC_TAGS.has(node.localName) || isEmbed(node));
export const isAtomicBlock = (node) => node?.localName === 'hr' || isEmbed(node);
export const isContainer = (element) => CONTAINER_TAGS.has(element.localName) || isEmbed(element);
export const hasVisibleText = (text) => VISIBLE_TEXT.test(text);
export const elementOf = (node) => (isElement(node) ? node : node?.parentElement ?? null);
export const unique = (items) => [...new Set(items)];
export const nodeIndex = (node) => Array.prototype.indexOf.call(node.parentNode.childNodes, node);
export const maxOffset = (node) => (isText(node) ? node.length : node.childNodes.length);

export function closestWithin(node, selector, boundary) {
    const found = elementOf(node)?.closest(selector);
    return found && found !== boundary && boundary.contains(found) ? found : null;
}

export function cellOf(node, content) {
    return closestWithin(node, 'td, th', content);
}

export function blockOf(node, content) {
    for (let element = elementOf(node); element && element !== content; element = element.parentElement) {
        if (isBlock(element) && !isContainer(element)) {
            return element;
        }
    }
    return null;
}

export function hasBlockChild(element) {
    return Array.prototype.some.call(element.children, isBlock);
}

export function isTextBlock(element) {
    if (!isBlock(element) || isContainer(element)) {
        return false;
    }
    let hasBlock = false;
    for (const child of element.childNodes) {
        if (isBlock(child)) {
            hasBlock = true;
        } else if (isElement(child) || (isText(child) && hasVisibleText(child.data))) {
            return true;
        }
    }
    return !hasBlock;
}

export function hasVisibleContent(node) {
    if (isElement(node) && node.matches(MEDIA_SELECTOR)) {
        return true;
    }
    return hasVisibleText(node.textContent) || Boolean(node.querySelector?.(MEDIA_SELECTOR));
}

export function hasOwnContent(element) {
    for (const child of element.childNodes) {
        const visible = isText(child)
            ? hasVisibleText(child.data)
            : isElement(child) && !isBlock(child) && hasVisibleContent(child);
        if (visible) {
            return true;
        }
    }
    return false;
}

export function hasAnyContent(node) {
    return node.textContent !== '' || Boolean(node.querySelector?.(ATOMIC_SELECTOR));
}

export function removeWithEmptyInlines(node, boundary) {
    let parent = node.parentElement;
    node.remove();
    while (parent && parent !== boundary && !isBlock(parent) && !hasAnyContent(parent)) {
        const next = parent.parentElement;
        parent.remove();
        parent = next;
    }
}

export function createBreak(doc) {
    return doc.createElement('br');
}

export function emptyParagraph(doc) {
    const paragraph = doc.createElement('p');
    paragraph.append(createBreak(doc));
    return paragraph;
}

export function removeEmptyInlines(root) {
    const candidates = [...root.querySelectorAll(INLINE_FORMAT_SELECTOR)].reverse();
    for (const element of candidates) {
        if (!hasAnyContent(element)) {
            element.remove();
        }
    }
}

export function ensurePlaceholder(element) {
    removeEmptyInlines(element);
    if (hasOwnContent(element)) {
        return;
    }
    const hasBreak = Array.prototype.some.call(element.childNodes, (child) => child.localName === 'br');
    if (!hasBreak) {
        element.prepend(createBreak(element.ownerDocument));
    }
}

export function placeholderOf(block) {
    if (!block || hasOwnContent(block)) {
        return null;
    }
    const breaks = [...block.querySelectorAll('br')].filter((br) => !blockOf(br, block));
    return breaks.length === 1 ? breaks[0] : null;
}

export function removeTrailingBreak(element) {
    let last = element.lastChild;
    while (isText(last) && !hasVisibleText(last.data)) {
        last = last.previousSibling;
    }
    if (last?.localName === 'br') {
        last.remove();
    }
}

export function unwrapElement(element) {
    element.replaceWith(...element.childNodes);
}

export function retag(element, tag) {
    const replacement = element.ownerDocument.createElement(tag);
    for (const { name, value } of element.attributes) {
        if (name !== 'level') {
            replacement.setAttribute(name, value);
        }
    }
    replacement.append(...element.childNodes);
    element.replaceWith(replacement);
    return replacement;
}

export function setAttributes(element, attributes) {
    for (const [name, value] of attributes) {
        element.setAttribute(name, value);
    }
    return element;
}

export function wrapInlineRuns(parent, tag = 'p') {
    const doc = parent.ownerDocument;
    let run = null;
    for (const node of [...parent.childNodes]) {
        if (isBlock(node)) {
            run = null;
            continue;
        }
        if (!run && (!isElement(node) && !(isText(node) && hasVisibleText(node.data)))) {
            continue;
        }
        if (!run) {
            run = doc.createElement(tag);
            node.before(run);
        }
        run.append(node);
    }
}

export function splitAt(element, node, offset) {
    const range = element.ownerDocument.createRange();
    range.setStart(node, offset);
    range.setEnd(element, element.childNodes.length);
    const tail = element.cloneNode(false);
    tail.removeAttribute('id');
    tail.append(range.extractContents());
    element.after(tail);
    return tail;
}

export function removeWithEmptyAncestors(node, content) {
    let parent = node.parentElement;
    node.remove();
    while (parent && parent !== content && PRUNABLE_TAGS.has(parent.localName)
        && parent.children.length === 0 && !hasVisibleText(parent.textContent)) {
        const next = parent.parentElement;
        parent.remove();
        parent = next;
    }
}

export function mergeBlocks(target, source, content) {
    removeTrailingBreak(target);
    const children = [...source.childNodes];
    const inline = children.filter((child) => !isBlock(child));
    const blocks = children.filter(isBlock);
    target.append(...inline);
    if (target.localName === 'li') {
        target.append(...blocks);
    } else {
        target.after(...blocks);
    }
    removeWithEmptyAncestors(source, content);
    ensurePlaceholder(target);
}

export function convertNewlines(element, toBreaks) {
    const doc = element.ownerDocument;
    if (!toBreaks) {
        for (const br of [...element.querySelectorAll('br')]) {
            br.replaceWith(doc.createTextNode('\n'));
        }
        return;
    }
    const walker = doc.createTreeWalker(element, NodeFilter.SHOW_TEXT);
    const texts = [];
    for (let text = walker.nextNode(); text; text = walker.nextNode()) {
        texts.push(text);
    }
    for (const text of texts.filter((candidate) => candidate.data.includes('\n'))) {
        const parts = text.data.split('\n');
        const nodes = parts.flatMap((part, index) => (index === 0 ? [] : [createBreak(doc)]).concat(part ? [doc.createTextNode(part)] : []));
        text.replaceWith(...nodes);
    }
}
