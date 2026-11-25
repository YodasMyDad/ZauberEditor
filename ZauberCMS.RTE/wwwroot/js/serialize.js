import { hasStrayBreaks, removeStrayBreaks } from './breaks.js';
import {
    ATOMIC_SELECTOR, EMBED_SELECTOR, LAYOUT_TAGS, hasVisibleContent, hasVisibleText, isAtomic, isBlock, isElement, isEmbed, isText,
    isTextBlock, nodeIndex, removeWithEmptyInlines, retag
} from './dom.js';
import { bareEmbed } from './embeds.js';

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

function cleanCopy(content, holder) {
    const root = withoutHolder(content, holder);
    if (!hasStrayBreaks(root)) {
        return root;
    }
    const copy = root === content ? content.cloneNode(true) : root;
    removeStrayBreaks(copy);
    return copy;
}

function render(content, holder, action, clean = true) {
    const base = clean ? cleanCopy(content, holder) : withoutHolder(content, holder);
    const root = base === content && content.querySelector(EMBED_SELECTOR) ? content.cloneNode(true) : base;
    root.querySelectorAll(EMBED_SELECTOR).forEach(bareEmbed);
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

const isStray = (node) => !isElement(node) && !(isText(node) && hasVisibleText(node.data));
const isEmptyBlock = (node) => (node.localName === 'p' || node.localName === 'div') && !hasVisibleContent(node);

function outputNodes(root) {
    const nodes = [...root.childNodes].filter((node) => !isStray(node));
    return nodes.every(isEmptyBlock) ? [] : nodes;
}

function collectFlow(parent, parts) {
    for (const child of parent.childNodes) {
        if (isText(child)) {
            parts.push(child);
        } else if (!isElement(child)) {
            continue;
        } else if (child.localName === 'pre' || child.localName === 'br') {
            parts.push('\n');
        } else if (isBlock(child)) {
            parts.push('\n');
            collectFlow(child, parts);
            parts.push('\n');
        } else if (isAtomic(child)) {
            parts.push('x');
        } else {
            collectFlow(child, parts);
        }
    }
}

const isFilled = (character) => character !== undefined && /\S/.test(character);

function relaxSpaces(root) {
    const parts = ['\n'];
    collectFlow(root, parts);
    parts.push('\n');
    const pieces = parts.map((part) => (typeof part === 'string' ? part : part.data));
    const flow = pieces.join('');
    let offset = 0;
    parts.forEach((part, index) => {
        const start = offset;
        offset += pieces[index].length;
        if (typeof part !== 'string' && part.data.includes(NO_BREAK_SPACE)) {
            const last = part.data.length - 1;
            part.data = part.data.replace(/\u00A0/g, (match, at) => ((at === 0 || at === last) && isFilled(flow[start + at - 1]) && isFilled(flow[start + at + 1]) ? ' ' : match));
        }
    });
}

function trimPreNewline(pre) {
    pre.normalize();
    const last = pre.lastChild;
    if (isText(last) && last.data.endsWith('\n') && !last.data.endsWith('\n\n')) {
        last.data = last.data.slice(0, -1);
    }
}

function emptyBlankBlocks(root) {
    for (const block of root.querySelectorAll('p, h1, h2, h3, h4, h5, h6, li, td, th, div, address')) {
        if (isTextBlock(block) && !block.querySelector(ATOMIC_SELECTOR) && /^\s*$/.test(block.textContent)) {
            block.replaceChildren();
        }
    }
}

function tidyOutput(root, aliases) {
    for (const [alias, primary] of aliases) {
        if (alias !== primary) {
            root.querySelectorAll(alias).forEach((element) => retag(element, primary));
        }
    }
    root.querySelectorAll('pre').forEach(trimPreNewline);
    root.normalize();
    relaxSpaces(root);
    emptyBlankBlocks(root);
}

function renderOutput(content, holder, aliases, action) {
    return render(content, holder, (root) => {
        const copy = root === content ? content.cloneNode(true) : root;
        tidyOutput(copy, aliases);
        return action(outputNodes(copy));
    });
}

export function serializeContent(content, holder, aliases) {
    return renderOutput(content, holder, aliases, (nodes) => nodes.map(outerHtml).join(''));
}

export function snapshotHtml(content, holder) {
    return render(content, holder, (root) => root.innerHTML, false);
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
    if (!LAYOUT_TAGS.has(node.localName) || isTextBlock(node) || isEmbed(node)) {
        return indent + node.outerHTML;
    }
    const [open, close] = tagParts(node);
    const inner = [...node.childNodes].map((child) => formatNode(child, depth + 1)).filter(Boolean);
    return [indent + open, ...inner, indent + close].join('\n');
}

export function formatHtml(content, holder, aliases) {
    return renderOutput(content, holder, aliases, (nodes) => nodes.map((node) => formatNode(node, 0)).filter(Boolean).join('\n'));
}

export function toUtf8(text) {
    return encoder.encode(text);
}

export function jsonBytes(value) {
    return encoder.encode(JSON.stringify(value ?? null));
}

const isHighSurrogate = (code) => code >= 0xd800 && code <= 0xdbff;

export function utf8Chunks(text, limit) {
    const buffer = new Uint8Array(limit);
    const chunks = [];
    let bytes = 0;
    for (let start = 0; start < text.length;) {
        let end = Math.min(start + limit, text.length);
        if (end < text.length && isHighSurrogate(text.charCodeAt(end - 1))) {
            end--;
        }
        const { read, written } = encoder.encodeInto(text.slice(start, end), buffer);
        chunks.push(text.slice(start, start + read));
        start += read;
        bytes += written;
    }
    return { chunks: chunks.length > 0 ? chunks : [''], bytes };
}
