import { BLOCK_TAGS, LIST_TAGS } from './dom.js';

const TAG_NAME = /^[a-z][a-z0-9-]*$/;
const STYLE_NAME = /^-{0,2}[a-z][a-z0-9-]*$/;
const BLOCK_TYPES = new Set(['p', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'pre', 'blockquote', 'div', 'ul', 'ol']);
const WRAPPER_TAGS = new Set(['address', 'article', 'aside', 'blockquote', 'details', 'div', 'figure', 'footer', 'header', 'main', 'nav', 'section']);

function reject(ctx, message) {
    ctx.channel.error(message);
    return null;
}

function tagName(tag) {
    const name = String(tag ?? '').toLowerCase();
    if (!TAG_NAME.test(name)) {
        throw new Error(`Tag '${tag}' is not valid.`);
    }
    return name;
}

function allowed(ctx, name, tags = [name]) {
    const denied = tags.filter((tag) => !ctx.sanitizer.allowsTag(tag));
    return denied.length > 0 ? reject(ctx, `Tags not allowed by the HTML policy: ${denied.join(', ')}.`) : name;
}

export function inlineTag(ctx, tag) {
    return allowed(ctx, tagName(tag));
}

export function wrapperName(tag) {
    const name = tagName(tag);
    if (BLOCK_TAGS.has(name) && !WRAPPER_TAGS.has(name)) {
        throw new Error(`Tag '${tag}' cannot wrap a selection.`);
    }
    return name;
}

export function wrapperTag(ctx, tag) {
    return allowed(ctx, wrapperName(tag));
}

export function blockType(ctx, type) {
    const name = String(type ?? '').toLowerCase();
    if (!BLOCK_TYPES.has(name)) {
        throw new Error(`Block type '${type}' is not supported.`);
    }
    return allowed(ctx, name, LIST_TAGS.has(name) ? [name, 'li'] : [name]);
}

export function validAttributes(ctx, tag, attributes) {
    const entries = Object.entries(attributes ?? {}).map(([name, value]) => [name.toLowerCase(), value]);
    const rejected = entries.filter(([name, value]) => !ctx.sanitizer.isAllowedAttribute(tag, name, value));
    return rejected.length > 0
        ? reject(ctx, `Attributes not allowed by the HTML policy: ${rejected.map(([name]) => name).join(', ')}.`)
        : entries;
}

export function validStyles(ctx, styles) {
    const entries = Object.entries(styles ?? {})
        .filter(([name, value]) => STYLE_NAME.test(name) && (value === null || typeof value === 'string'));
    const rejected = entries.filter(([name, value]) => value && !ctx.sanitizer.isAllowedAttribute('p', 'style', `${name}: ${value}`));
    return rejected.length > 0
        ? reject(ctx, `Styles not allowed by the HTML policy: ${rejected.map(([name]) => name).join(', ')}.`)
        : entries;
}
