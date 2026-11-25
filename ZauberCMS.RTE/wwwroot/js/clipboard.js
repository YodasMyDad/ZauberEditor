import { isBlock, removeEmptyInlines, retag, unwrapElement } from './dom.js';
import { textFragment } from './insert.js';
import { prepareOfficeHtml } from './office.js';
import { createRange } from './selection.js';
import { SELECTED_IMAGE_CLASS, clearImageMark } from './serialize.js';

const IMAGE_TYPE = /^image\//;
const MEGABYTE = 1024 * 1024;
const DRAG_TYPE = 'application/x-zauber-rte';
const ALIGNMENTS = new Set(['left', 'center', 'right', 'justify']);
const IMAGE_SIZE = /^\d+(?:\.\d+)?(?:px|%)$/;
const INDENT = /^\d+(?:\.\d+)?em$/;
const TEXT_STYLES = ['color', 'background-color'];

export function normalizeFragment(fragment, config, sanitizer) {
    for (const [alias, primary] of config.aliases) {
        if (alias !== primary && sanitizer.allowsTag(primary)) {
            fragment.querySelectorAll(alias).forEach((element) => retag(element, primary));
        }
    }
    fragment.querySelectorAll(`.${SELECTED_IMAGE_CLASS}`).forEach(clearImageMark);
    removeEmptyInlines(fragment);
    return fragment;
}

function editorStyles(element) {
    const { style } = element;
    const kept = [['text-align', style.getPropertyValue('text-align')]].filter(([, value]) => ALIGNMENTS.has(value));
    if (element.localName === 'img') {
        kept.push(...['width', 'height'].map((name) => [name, style.getPropertyValue(name)]).filter(([, value]) => IMAGE_SIZE.test(value)));
    }
    if (element.localName === 'span') {
        kept.push(...TEXT_STYLES.map((name) => [name, style.getPropertyValue(name)]).filter(([, value]) => value !== ''));
    }
    const indent = style.getPropertyValue('margin-inline-start');
    if (isBlock(element) && INDENT.test(indent)) {
        kept.push(['margin-inline-start', indent]);
    }
    return kept;
}

function keepEditorStyles(fragment) {
    for (const element of fragment.querySelectorAll('[style]')) {
        const kept = editorStyles(element);
        element.removeAttribute('style');
        kept.forEach(([name, value]) => element.style.setProperty(name, value));
    }
    fragment.querySelectorAll('br.Apple-interchange-newline').forEach((marker) => marker.remove());
    for (const span of fragment.querySelectorAll('span')) {
        if (span.attributes.length === 0) {
            unwrapElement(span);
        }
    }
    return fragment;
}

function cleanPastedHtml(html, ctx) {
    const parsed = new DOMParser().parseFromString(html, 'text/html');
    prepareOfficeHtml(parsed.body);
    return normalizeFragment(keepEditorStyles(ctx.sanitizer.clean(parsed.body.innerHTML)), ctx.config, ctx.sanitizer);
}

function readAsDataUrl(file) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => resolve(reader.result);
        reader.onerror = () => reject(reader.error);
        reader.readAsDataURL(file);
    });
}

function extensionOf(name) {
    return /\.[^./\\]+$/.exec(name ?? '')?.[0].toLowerCase() ?? null;
}

function typeExtensions(mime) {
    const subtype = mime.slice('image/'.length).toLowerCase().replace(/\+.*$/, '');
    return subtype === 'jpeg' ? ['.jpeg', '.jpg'] : [`.${subtype}`];
}

function isAllowedType(file, allowed) {
    const extension = extensionOf(file.name);
    return typeExtensions(file.type).some((type) => allowed.has(type)) && (extension === null || allowed.has(extension));
}

function canEmbedImages(ctx) {
    return ctx.config.image.allowBase64Upload && ctx.config.policy.allowDataUrls && ctx.sanitizer.allowsTag('img');
}

function keep(ctx, files, test, message) {
    const kept = files.filter(test);
    if (kept.length < files.length) {
        ctx.channel.rejected(message);
    }
    return kept;
}

function acceptedImages(ctx, files) {
    const { allowedTypes, maxFileSizeBytes } = ctx.config.image;
    const typed = keep(ctx, files, (file) => isAllowedType(file, allowedTypes),
        `Only ${[...allowedTypes].join(', ')} images can be added.`);
    return keep(ctx, typed, (file) => file.size <= maxFileSizeBytes,
        `Images larger than the ${Number((maxFileSizeBytes / MEGABYTE).toFixed(1))} MB limit were not added.`);
}

async function imagesFragment(ctx, files) {
    const fragment = ctx.content.ownerDocument.createDocumentFragment();
    for (const url of await Promise.all(files.map(readAsDataUrl))) {
        if (ctx.sanitizer.isAllowedAttribute('img', 'src', url)) {
            const image = ctx.content.ownerDocument.createElement('img');
            image.src = url;
            fragment.append(image);
        }
    }
    return fragment;
}

function pointRange(content, x, y) {
    const doc = content.ownerDocument;
    const position = doc.caretPositionFromPoint?.(x, y);
    const range = position ? createRange(content) : doc.caretRangeFromPoint?.(x, y);
    if (position) {
        range.setStart(position.offsetNode, position.offset);
        range.collapse(true);
    }
    return range && content.contains(range.startContainer) ? range : null;
}

async function insertImages(ctx, actions, files, range) {
    if (!canEmbedImages(ctx)) {
        ctx.channel.rejected('Images cannot be pasted or dropped into this editor.');
        return;
    }
    const accepted = acceptedImages(ctx, files);
    if (accepted.length > 0) {
        actions.insert(await imagesFragment(ctx, accepted), 'paste', range);
    }
}

function plainTextOf(data) {
    const text = data.getData('text/plain');
    if (text) {
        return text;
    }
    const html = data.getData('text/html');
    return html ? new DOMParser().parseFromString(html, 'text/html').body.textContent ?? '' : '';
}

function pasteAsText(ctx, actions, data, range) {
    const text = plainTextOf(data);
    if (text) {
        actions.insert(textFragment(ctx.content.ownerDocument, text), 'paste', range);
    }
}

async function insertTransfer(ctx, actions, data, range) {
    const files = [...(data.files ?? [])].filter((file) => IMAGE_TYPE.test(file.type));
    const html = data.getData('text/html');
    const text = data.getData('text/plain');
    if (files.length > 0 && (canEmbedImages(ctx) || (!html && !text))) {
        await insertImages(ctx, actions, files, range);
        return;
    }
    if (html) {
        actions.insert(cleanPastedHtml(html, ctx), 'paste', range);
        return;
    }
    if (text) {
        actions.insert(textFragment(ctx.content.ownerDocument, text), 'paste', range);
    }
}

export function bindClipboard(ctx, actions) {
    const { content, signal, channel } = ctx;
    const options = { signal };
    const dragToken = `${Date.now()}-${Math.random()}`;
    const report = (failure) => channel.error(failure?.message ?? String(failure));
    content.addEventListener('paste', channel.guard((event) => {
        event.preventDefault();
        if (ctx.config.readOnly || !event.clipboardData) {
            return;
        }
        const range = ctx.ranges.current()?.cloneRange() ?? null;
        if (ctx.plainPaste || ctx.pasteTextOnce) {
            ctx.pasteTextOnce = false;
            pasteAsText(ctx, actions, event.clipboardData, range);
            return;
        }
        insertTransfer(ctx, actions, event.clipboardData, range).catch(report);
    }), options);
    content.addEventListener('dragstart', (event) => {
        event.dataTransfer?.setData(DRAG_TYPE, dragToken);
    }, options);
    content.addEventListener('drop', channel.guard((event) => {
        if (event.dataTransfer?.getData(DRAG_TYPE) === dragToken && !ctx.config.readOnly) {
            return;
        }
        event.preventDefault();
        if (ctx.config.readOnly || !event.dataTransfer) {
            return;
        }
        const range = pointRange(content, event.clientX, event.clientY);
        insertTransfer(ctx, actions, event.dataTransfer, range).catch(report);
    }), options);
}
