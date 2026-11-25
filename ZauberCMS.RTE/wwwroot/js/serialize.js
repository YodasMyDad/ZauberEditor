import {
    LAYOUT_TAGS, hasVisibleContent, hasVisibleText, isBlock, isElement, isText, isTextBlock, nodeIndex, removeWithEmptyInlines
} from './dom.js';

export const SELECTED_IMAGE_CLASS = 'rte-image-selected';

const NO_BREAK_SPACE = '\u00A0';
const encoder = new TextEncoder();

const escapeText = (text) => text.replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;').replaceAll(NO_BREAK_SPACE, '&nbsp;');

export function clearImageMark(element) {
    element.classList.remove(SELECTED_IMAGE_CLASS);
    if (element.classList.length === 0) {
        element.removeAttribute('class');
    }
}

function withoutHolder(content, holder) {
    if (!holder || !content.contains(holder)) {
        return content;
    }
    const path = [];
    for (let node = holder; node !== content; node = node.parentNode) {
        path.unshift(nodeIndex(node));
    }
    const copy = content.cloneNode(true);
    removeWithEmptyInlines(path.reduce((node, index) => node.childNodes[index], copy), copy);
    return copy;
}

function render(content, holder, action) {
    const root = withoutHolder(content, holder);
    const marked = [...root.getElementsByClassName(SELECTED_IMAGE_CLASS)];
    marked.forEach(clearImageMark);
    try {
        return action(root);
    } finally {
        for (const element of marked) {
            element.classList.add(SELECTED_IMAGE_CLASS);
        }
    }
}

const outerHtml = (node) => {
    if (isElement(node)) {
        return node.outerHTML;
    }
    return isText(node) ? escapeText(node.data) : '';
};

const isBlank = (node) => (isText(node) && !hasVisibleText(node.data))
    || (isElement(node) && (node.localName === 'p' || node.localName === 'div') && !hasVisibleContent(node))
    || (!isText(node) && !isElement(node));

export function serializeContent(content, holder) {
    return render(content, holder, (root) => {
        const nodes = [...root.childNodes];
        let start = 0;
        let end = nodes.length;
        while (start < end && isBlank(nodes[start])) {
            start++;
        }
        while (end > start && isBlank(nodes[end - 1])) {
            end--;
        }
        return nodes.slice(start, end).map(outerHtml).join('');
    });
}

export function snapshotHtml(content, holder) {
    return render(content, holder, (root) => root.innerHTML);
}

export function contentText(content, holder) {
    return render(content, holder, (root) => {
        const parts = [];
        const walker = root.ownerDocument.createTreeWalker(root, NodeFilter.SHOW_ELEMENT | NodeFilter.SHOW_TEXT);
        for (let node = walker.nextNode(); node; node = walker.nextNode()) {
            if (isText(node)) {
                parts.push(node.data);
            } else if (node.localName === 'br') {
                parts.push('\n');
            } else if (isBlock(node) && parts.length > 0 && !parts.at(-1).endsWith('\n')) {
                parts.push('\n');
            }
        }
        return parts.join('').trim();
    });
}

function tagParts(element) {
    const shell = element.cloneNode(false).outerHTML;
    const close = `</${element.localName}>`;
    return [shell.slice(0, shell.length - close.length), close];
}

function formatNode(node, depth) {
    const indent = '    '.repeat(depth);
    if (isText(node)) {
        const text = node.data.trim();
        return text ? indent + escapeText(text) : '';
    }
    if (!isElement(node)) {
        return '';
    }
    if (!LAYOUT_TAGS.has(node.localName) || isTextBlock(node)) {
        return indent + node.outerHTML;
    }
    const [open, close] = tagParts(node);
    const inner = [...node.childNodes].map((child) => formatNode(child, depth + 1)).filter(Boolean);
    return [indent + open, ...inner, indent + close].join('\n');
}

export function formatHtml(content, holder) {
    return render(content, holder, (root) => [...root.childNodes].map((node) => formatNode(node, 0)).filter(Boolean).join('\n'));
}

export function toUtf8(text) {
    return encoder.encode(text);
}

export function jsonBytes(value) {
    return value === null ? new Uint8Array(0) : encoder.encode(JSON.stringify(value));
}

export function fitsInline(text, limit) {
    if (text.length * 3 <= limit) {
        return true;
    }
    return text.length <= limit && encoder.encode(text).length <= limit;
}
