import { EMBED_SELECTOR } from './dom.js';

export const SELECTED_EMBED_CLASS = 'rte-embed-selected';

const ATTRIBUTES = ['data-embed', 'data-embed-id', 'data-embed-url'];
const NAME = /^[a-z0-9][a-z0-9-]{0,31}$/;
const ID = /^[A-Za-z0-9_-]{1,64}$/;
const SCHEME = /^[a-z][a-z0-9+.-]*:/i;
const WEB_URL = /^https?:\/\//i;
const MAX_URL_LENGTH = 2048;
const WRAPPERS = new Set(['p', 'div', 'span', 'figure', 'center']);

const cssString = (text) => `"${CSS.escape(text)}"`;

function tryCompile(pattern) {
    try {
        return /\(\?<id>/.test(pattern) ? new RegExp(pattern, 'iu') : null;
    } catch {
        return null;
    }
}

function compile(pattern, invalid) {
    const expression = tryCompile(pattern);
    if (!expression) {
        invalid.push(pattern);
    }
    return expression;
}

export function createProviders(list) {
    const invalid = [];
    const providers = (Array.isArray(list) ? list : [])
        .filter((entry) => typeof entry?.name === 'string' && NAME.test(entry.name))
        .map((entry) => ({
            name: entry.name,
            displayName: typeof entry.displayName === 'string' && entry.displayName ? entry.displayName : entry.name,
            patterns: (Array.isArray(entry.patterns) ? entry.patterns : []).map((pattern) => compile(String(pattern), invalid)).filter(Boolean),
            thumbnail: typeof entry.thumbnail === 'string' && entry.thumbnail.includes('{id}') ? entry.thumbnail : null
        }))
        .filter((provider) => provider.patterns.length > 0);
    return { providers, invalid };
}

export function normalizeUrl(url) {
    const trimmed = typeof url === 'string' ? url.trim() : '';
    if (!trimmed || trimmed.length > MAX_URL_LENGTH) {
        return null;
    }
    let absolute = `https://${trimmed}`;
    if (trimmed.startsWith('//')) {
        absolute = `https:${trimmed}`;
    } else if (SCHEME.test(trimmed)) {
        absolute = trimmed;
    }
    try {
        return /^https?:$/.test(new URL(absolute).protocol) ? absolute : null;
    } catch {
        return null;
    }
}

export function matchEmbed(providers, url) {
    const absolute = normalizeUrl(url);
    if (!absolute) {
        return null;
    }
    for (const provider of providers) {
        for (const pattern of provider.patterns) {
            const id = pattern.exec(absolute)?.groups?.id;
            if (id && ID.test(id)) {
                return { provider, id, url: absolute };
            }
        }
    }
    return null;
}

function createPlaceholder(doc, match) {
    const figure = doc.createElement('figure');
    figure.setAttribute('data-embed', match.provider.name);
    figure.setAttribute('data-embed-id', match.id);
    figure.setAttribute('data-embed-url', match.url);
    return figure;
}

export function placeholderHtml(doc, match) {
    return createPlaceholder(doc, match).outerHTML;
}

export function embedInfo(figure) {
    return {
        provider: figure.getAttribute('data-embed'),
        id: figure.getAttribute('data-embed-id'),
        url: figure.getAttribute('data-embed-url')
    };
}

export function bareEmbed(figure) {
    for (const { name } of [...figure.attributes]) {
        if (!ATTRIBUTES.includes(name)) {
            figure.removeAttribute(name);
        }
    }
    figure.replaceChildren();
}

function thumbnailUrl(provider, id) {
    if (!provider.thumbnail) {
        return null;
    }
    try {
        const url = new URL(provider.thumbnail.replaceAll('{id}', id));
        return url.protocol === 'https:' ? url.href : null;
    } catch {
        return null;
    }
}

function decorate(figure, provider, info) {
    figure.contentEditable = 'false';
    figure.setAttribute('role', 'img');
    figure.setAttribute('aria-label', `${provider.displayName} video: ${info.url ?? info.id}`);
    figure.style.setProperty('--rte-embed-label', cssString(provider.displayName));
    figure.style.setProperty('--rte-embed-caption', cssString(info.url ?? info.id));
    const thumbnail = thumbnailUrl(provider, info.id);
    if (thumbnail) {
        figure.style.setProperty('--rte-embed-thumbnail', `url(${cssString(thumbnail)})`);
    }
}

function prepare(figure, providers) {
    const info = embedInfo(figure);
    const provider = providers.find((candidate) => candidate.name === info.provider?.trim().toLowerCase());
    if (!provider || !ID.test(info.id ?? '')) {
        figure.remove();
        return false;
    }
    const url = info.url && WEB_URL.test(info.url.trim()) && normalizeUrl(info.url) ? info.url.trim() : null;
    for (const { name } of [...figure.attributes]) {
        figure.removeAttribute(name);
    }
    figure.replaceChildren();
    figure.setAttribute('data-embed', provider.name);
    figure.setAttribute('data-embed-id', info.id);
    if (url) {
        figure.setAttribute('data-embed-url', url);
    }
    decorate(figure, provider, { id: info.id, url });
    return true;
}

export function prepareEmbeds(fragment, providers) {
    let removed = 0;
    for (const figure of [...fragment.querySelectorAll(EMBED_SELECTOR)]) {
        if (!prepare(figure, providers)) {
            removed++;
        }
    }
    return removed;
}

function frameHost(src) {
    try {
        return new URL(src).hostname || null;
    } catch {
        return null;
    }
}

function soleWrapper(frame) {
    let node = frame;
    for (let parent = node.parentElement; WRAPPERS.has(parent?.localName) && parent.childElementCount === 1
        && parent.textContent.trim() === ''; parent = node.parentElement) {
        node = parent;
    }
    return node;
}

export function convertFrames(html, providers) {
    if (!/<iframe/i.test(html)) {
        return { html, removed: [] };
    }
    const doc = new DOMParser().parseFromString(html, 'text/html');
    const removed = [];
    for (const frame of [...doc.body.querySelectorAll('iframe')]) {
        const src = frame.getAttribute('src') ?? '';
        const match = matchEmbed(providers, src);
        if (match) {
            soleWrapper(frame).replaceWith(createPlaceholder(doc, match));
        } else {
            removed.push(frameHost(normalizeUrl(src) ?? '') ?? 'an unknown site');
            frame.remove();
        }
    }
    return { html: doc.body.innerHTML, removed };
}

export function embedTransfer(range) {
    const fragment = range.cloneContents();
    if (!fragment.querySelector(EMBED_SELECTOR)) {
        return null;
    }
    const holder = range.startContainer.ownerDocument.createElement('div');
    holder.append(fragment);
    holder.querySelectorAll(EMBED_SELECTOR).forEach(bareEmbed);
    const html = holder.innerHTML;
    for (const figure of holder.querySelectorAll(EMBED_SELECTOR)) {
        figure.replaceWith(`\n${figure.getAttribute('data-embed-url') ?? ''}\n`);
    }
    return { html, text: holder.textContent.trim() };
}
