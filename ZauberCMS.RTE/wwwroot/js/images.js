import { closestWithin, isElement } from './dom.js';
import { startDrag } from './drag.js';
import { FrameScheduler } from './scheduler.js';
import { nodeRange, selectedElement, startNode } from './selection.js';
import { SELECTED_IMAGE_CLASS, clearImageMark } from './serialize.js';

const HANDLES = [['nw', -1, -1], ['ne', 1, -1], ['sw', -1, 1], ['se', 1, 1]];
const MIN_SIZE = 20;

const clamp = (value, minimum, maximum) => Math.min(Math.max(value, minimum), maximum ?? Infinity);
const toInteger = (value) => {
    const parsed = Number.parseInt(value ?? '', 10);
    return Number.isFinite(parsed) && parsed > 0 ? parsed : null;
};

export function imageAt(content, range) {
    const selected = selectedElement(range);
    if (selected?.localName === 'img') {
        return selected;
    }
    return closestWithin(startNode(range), 'img', content);
}

export function imageInfo(image) {
    return {
        src: image.getAttribute('src') ?? '',
        alt: image.getAttribute('alt'),
        title: image.getAttribute('title'),
        width: toInteger(image.getAttribute('width')) ?? toInteger(image.style.width),
        height: toInteger(image.getAttribute('height')) ?? toInteger(image.style.height)
    };
}

function scaledSize(start, delta, signs, keepRatio, limits) {
    const width = start.width + signs[0] * delta.deltaX;
    const height = start.height + signs[1] * delta.deltaY;
    if (!keepRatio) {
        return [clamp(width, MIN_SIZE, limits.maxWidth), clamp(height, MIN_SIZE, limits.maxHeight)];
    }
    const lower = Math.max(MIN_SIZE / start.width, MIN_SIZE / start.height);
    const upper = Math.min((limits.maxWidth ?? Infinity) / start.width, (limits.maxHeight ?? Infinity) / start.height);
    const scale = clamp(Math.max(width / start.width, height / start.height), lower, upper);
    return [start.width * scale, start.height * scale];
}

export class ImageTool {
    #ctx;
    #image = null;
    #overlay = null;
    #session = null;
    #frame;

    constructor(ctx) {
        this.#ctx = ctx;
        this.#frame = new FrameScheduler(ctx.channel.guard(() => this.#place()));
        const options = { signal: ctx.signal };
        ctx.content.addEventListener('click', ctx.channel.guard((event) => {
            if (event.target.localName === 'img' && !ctx.config.readOnly) {
                this.select(event.target);
            }
        }), options);
        ctx.content.addEventListener('scroll', () => this.#frame.schedule(), { ...options, passive: true });
    }

    get selected() {
        return this.#image?.isConnected ? this.#image : null;
    }

    select(image) {
        if (this.#image !== image) {
            this.deselect();
            this.#image = image;
            image.classList.add(SELECTED_IMAGE_CLASS);
            if (this.#resizable()) {
                this.#show();
            }
        }
        this.#ctx.ranges.remember(nodeRange(this.#ctx.content, image));
        return true;
    }

    sync(range) {
        if (this.#image && selectedElement(range) !== this.#image) {
            this.deselect();
        }
    }

    deselect() {
        const image = this.#image;
        if (!image) {
            return;
        }
        this.#image = null;
        clearImageMark(image);
        this.#session?.abort();
        this.#session = null;
        this.#overlay?.remove();
        this.#overlay = null;
        this.#frame.cancel();
    }

    dispose() {
        this.deselect();
    }

    #show() {
        const doc = this.#ctx.content.ownerDocument;
        const session = new AbortController();
        const signal = AbortSignal.any([this.#ctx.signal, session.signal]);
        const overlay = doc.createElement('div');
        overlay.className = 'rte-image-resize-handles';
        overlay.style.position = 'absolute';
        for (const [name, ...signs] of HANDLES) {
            const handle = doc.createElement('div');
            handle.className = 'rte-image-resize-handle';
            handle.dataset.handle = name;
            handle.addEventListener('pointerdown', this.#ctx.channel.guard((event) => this.#resize(event, handle, signs, signal)), { signal });
            overlay.append(handle);
        }
        this.#ctx.root.append(overlay);
        const observer = new ResizeObserver(() => this.#frame.schedule());
        observer.observe(this.#image);
        observer.observe(this.#ctx.content);
        const mutations = new MutationObserver((records) => {
            this.#clearCopies(records);
            if (!this.#image?.isConnected) {
                this.deselect();
            }
        });
        mutations.observe(this.#ctx.content, { childList: true, subtree: true });
        signal.addEventListener('abort', () => {
            observer.disconnect();
            mutations.disconnect();
        }, { once: true });
        this.#overlay = overlay;
        this.#session = session;
        this.#place();
    }

    #resizable() {
        const { sanitizer } = this.#ctx;
        return sanitizer.isAllowedAttribute('img', 'width', '1') && sanitizer.isAllowedAttribute('img', 'height', '1');
    }

    #clearCopies(records) {
        for (const node of records.flatMap((record) => [...record.addedNodes])) {
            const marked = isElement(node) ? [node, ...node.querySelectorAll(`.${SELECTED_IMAGE_CLASS}`)] : [];
            marked.filter((element) => element !== this.#image && element.classList.contains(SELECTED_IMAGE_CLASS)).forEach(clearImageMark);
        }
    }

    #place() {
        const image = this.selected;
        if (!image || !this.#overlay) {
            return;
        }
        const root = this.#ctx.root;
        const rootBox = root.getBoundingClientRect();
        const box = image.getBoundingClientRect();
        const view = this.#ctx.content.getBoundingClientRect();
        const style = this.#overlay.style;
        style.left = `${box.left - rootBox.left - root.clientLeft}px`;
        style.top = `${box.top - rootBox.top - root.clientTop}px`;
        style.width = `${box.width}px`;
        style.height = `${box.height}px`;
        this.#overlay.hidden = box.bottom < view.top || box.top > view.bottom;
    }

    #resize(event, handle, signs, signal) {
        if (event.button !== 0 || this.#ctx.config.readOnly) {
            return;
        }
        const image = this.#image;
        const box = image.getBoundingClientRect();
        const start = { width: box.width, height: box.height };
        let size = null;
        startDrag(event, handle, signal, {
            move: (delta) => {
                const keepRatio = this.#ctx.config.image.maintainAspectRatio !== delta.event.shiftKey;
                size = scaledSize(start, delta, signs, keepRatio, this.#ctx.config.image).map(Math.round);
                this.#applySize(image, size);
                this.#place();
            },
            end: () => {
                if (size) {
                    this.#ctx.changes.commit('imageResize');
                    this.#ctx.channel.imageResized(size[0], size[1]);
                    this.#ctx.state.schedule();
                }
            }
        });
    }

    #applySize(image, [width, height]) {
        image.setAttribute('width', String(width));
        image.setAttribute('height', String(height));
        if (image.style.width) {
            image.style.width = `${width}px`;
        }
        if (image.style.height) {
            image.style.height = `${height}px`;
        }
    }
}
