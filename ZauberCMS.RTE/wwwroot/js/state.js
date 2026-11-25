import { primaryMark } from './config.js';
import { CELL_TAGS, HEADING_TAGS, LIST_TAGS, ZWSP, elementOf, isBlock, isEmbed, isText } from './dom.js';
import { indentLevel } from './indent.js';
import { FrameScheduler } from './scheduler.js';
import { selectedElement, startNode } from './selection.js';

const MAX_SCANNED_TEXTS = 200;
const ALIGNMENTS = new Map([['center', 'center'], ['right', 'right'], ['end', 'right'], ['justify', 'justify']]);
const EMPTY_FACTS = { marks: [], block: 'p', alignment: 'left', inLink: false, inImage: false, inEmbed: false, inTable: false, canOutdent: false };

function alignmentOf(element) {
    if (!isBlock(element)) {
        return null;
    }
    return element.style.textAlign || element.getAttribute('align') || null;
}

function marksOf(node, content, config) {
    const marks = new Set();
    for (let element = elementOf(node); element && element !== content; element = element.parentElement) {
        const mark = primaryMark(config, element.localName);
        if (mark) {
            marks.add(mark);
        }
    }
    return marks;
}

function marksAcross(content, range, config, fallback) {
    const root = range.commonAncestorContainer;
    const walker = content.ownerDocument.createTreeWalker(isText(root) ? root.parentNode : root, NodeFilter.SHOW_TEXT);
    let common = null;
    let scanned = 0;
    for (let text = walker.nextNode(); text && scanned < MAX_SCANNED_TEXTS; text = walker.nextNode()) {
        if (!range.intersectsNode(text) || text.data.replaceAll(ZWSP, '').trim() === ''
            || (text === range.startContainer && range.startOffset === text.length)
            || (text === range.endContainer && range.endOffset === 0)) {
            continue;
        }
        scanned++;
        const marks = marksOf(text, content, config);
        common = common ? new Set([...common].filter((mark) => marks.has(mark))) : marks;
    }
    return common ?? fallback;
}

function inspect(content, range, config, selectedImage) {
    const image = selectedImage ?? (selectedElement(range)?.localName === 'img' ? selectedElement(range) : null);
    const node = image ?? startNode(range);
    let textBlock = null;
    let list = null;
    let quote = false;
    let alignment = null;
    let inLink = false;
    let inTable = false;
    let canOutdent = false;
    for (let element = elementOf(node); element && element !== content; element = element.parentElement) {
        const tag = element.localName;
        alignment ??= alignmentOf(element);
        textBlock ??= HEADING_TAGS.has(tag) || tag === 'pre' ? tag : null;
        list ??= LIST_TAGS.has(tag) ? tag : null;
        quote ||= tag === 'blockquote';
        inLink ||= tag === 'a';
        inTable ||= CELL_TAGS.has(tag);
        canOutdent ||= tag === 'li' || indentLevel(element) > 0;
    }
    const startMarks = marksOf(node, content, config);
    const marks = range.collapsed || image ? startMarks : marksAcross(content, range, config, startMarks);
    return {
        marks: [...marks].sort(),
        block: textBlock ?? list ?? (quote ? 'blockquote' : 'p'),
        alignment: ALIGNMENTS.get(alignment?.toLowerCase()) ?? 'left',
        inLink,
        inImage: Boolean(image),
        inEmbed: isEmbed(selectedElement(range)),
        inTable,
        canOutdent
    };
}

function computeState(ctx) {
    const range = ctx.ranges.latest();
    const facts = range ? inspect(ctx.content, range, ctx.config, ctx.images.selected) : EMPTY_FACTS;
    return {
        ...facts,
        hasSelection: Boolean(range && !range.collapsed),
        hasFocus: ctx.ranges.focused,
        canUndo: ctx.history.canUndo || ctx.changes.dirty,
        canRedo: ctx.history.canRedo && !ctx.changes.dirty,
        fullscreen: ctx.fullscreen.active,
        plainPaste: ctx.plainPaste,
        prefersDark: ctx.darkScheme?.matches === true,
        words: ctx.changes.counts.words,
        characters: ctx.changes.counts.characters
    };
}

const sameState = (a, b) => Object.keys(b).every((key) => (Array.isArray(b[key])
    ? a[key].length === b[key].length && b[key].every((value, index) => a[key][index] === value)
    : a[key] === b[key]));

export class StateTracker {
    #ctx;
    #frame;
    #last = null;

    constructor(ctx) {
        this.#ctx = ctx;
        this.#frame = new FrameScheduler(ctx.channel.guard(() => this.#push()));
    }

    get engaged() {
        return this.#last?.hasFocus === true || this.#last?.hasSelection === true;
    }

    schedule() {
        this.#frame.schedule();
    }

    snapshot() {
        return computeState(this.#ctx);
    }

    dispose() {
        this.#frame.cancel();
    }

    #push() {
        const snapshot = this.snapshot();
        if (this.#last && sameState(this.#last, snapshot)) {
            return;
        }
        this.#last = snapshot;
        this.#ctx.channel.stateChanged(snapshot);
    }
}
