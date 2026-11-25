import { blockOf, ensurePlaceholder, isText, removeEmptyInlines, unique } from './dom.js';
import { createRange, pointAt, unitsBefore } from './selection.js';

const HIGHLIGHT = 'rte-find';
const SYNTAX = /[\^$.*+?()[\]{}|/]/g;
const NONE = { count: 0, current: 0 };

function pattern(text, matchCase) {
    return new RegExp(text.replace(SYNTAX, '\$&'), matchCase ? 'gu' : 'giu');
}

function textRuns(content, holder) {
    const runs = new Map();
    const walker = content.ownerDocument.createTreeWalker(content, NodeFilter.SHOW_TEXT | NodeFilter.SHOW_ELEMENT);
    for (let node = walker.nextNode(); node; node = walker.nextNode()) {
        if (node === holder || (!isText(node) && node.localName !== 'br')) {
            continue;
        }
        const block = blockOf(node, content) ?? content;
        const run = runs.get(block) ?? { block, text: '', nodes: [] };
        runs.set(block, run);
        if (isText(node)) {
            run.nodes.push({ node, start: run.text.length });
            run.text += node.data;
        } else {
            run.text += '\n';
        }
    }
    return [...runs.values()];
}

function pointIn(run, index, isEnd) {
    const entry = run.nodes.findLast(({ node, start }) => (isEnd
        ? start < index && index <= start + node.length
        : start <= index && index < start + node.length));
    return [entry.node, index - entry.start];
}

function findMatches(content, holder, text, matchCase) {
    const expression = pattern(text, matchCase);
    return textRuns(content, holder).flatMap((run) => [...run.text.matchAll(expression)]
        .filter((match) => match[0].length > 0)
        .map((match) => ({
            block: run.block,
            start: pointIn(run, match.index, false),
            end: pointIn(run, match.index + match[0].length, true)
        })));
}

function toRange(content, match) {
    const range = createRange(content);
    range.setStart(...match.start);
    range.setEnd(...match.end);
    return range;
}

function sameRange(content, match, range) {
    return Boolean(range) && toRange(content, match).compareBoundaryPoints(Range.START_TO_START, range) === 0
        && toRange(content, match).compareBoundaryPoints(Range.END_TO_END, range) === 0;
}

function textsBetween(match) {
    const [first] = match.start;
    const [last] = match.end;
    const texts = [];
    const walker = first.ownerDocument.createTreeWalker(match.block, NodeFilter.SHOW_TEXT);
    walker.currentNode = first;
    for (let node = walker.nextNode(); node && node !== last; node = walker.nextNode()) {
        texts.push(node);
    }
    return texts;
}

function replaceMatch(match, replacement) {
    const [first, startOffset] = match.start;
    const [last, endOffset] = match.end;
    if (first === last) {
        first.replaceData(startOffset, endOffset - startOffset, replacement);
    } else {
        textsBetween(match).forEach((text) => text.remove());
        last.deleteData(0, endOffset);
        first.replaceData(startOffset, first.length - startOffset, replacement);
        if (last.length === 0) {
            last.remove();
        }
    }
    return unitsBefore(match.block, first, startOffset + replacement.length);
}

function tidy(content, blocks, block, units) {
    blocks.forEach((tidied) => (tidied === content ? removeEmptyInlines(tidied) : ensurePlaceholder(tidied)));
    const caret = createRange(content);
    caret.setStart(...pointAt(block, units));
    caret.collapse(true);
    return caret;
}

export class Finder {
    #ctx;
    #change;
    #range = null;

    constructor(ctx, change) {
        this.#ctx = ctx;
        this.#change = change;
    }

    find(text, matchCase) {
        this.clear();
        const { content, ranges, caretMarks } = this.#ctx;
        const matches = this.#searchable(text) ? findMatches(content, caretMarks.holder, text, matchCase) : [];
        if (matches.length === 0) {
            return NONE;
        }
        const from = ranges.latest();
        const after = from ? matches.findIndex((match) => from.comparePoint(...match.start) === 1
            || (from.collapsed && from.comparePoint(...match.start) === 0)) : 0;
        const index = Math.max(after, 0);
        this.#select(toRange(content, matches[index]));
        return { count: matches.length, current: index + 1 };
    }

    replace(text, replacement, matchCase) {
        const { content, ranges, caretMarks } = this.#ctx;
        const matches = this.#searchable(text) ? findMatches(content, caretMarks.holder, text, matchCase) : [];
        const match = matches.find((candidate) => sameRange(content, candidate, ranges.latest()));
        if (match) {
            this.clear();
            this.#change(() => {
                ranges.remember(tidy(content, [match.block], match.block, replaceMatch(match, replacement)));
                return true;
            });
        }
        return this.find(text, matchCase);
    }

    replaceAll(text, replacement, matchCase) {
        const { content, ranges, caretMarks } = this.#ctx;
        const matches = this.#searchable(text) ? findMatches(content, caretMarks.holder, text, matchCase) : [];
        if (matches.length === 0) {
            return 0;
        }
        this.clear();
        this.#change(() => {
            const carets = matches.toReversed().map((match) => ({ block: match.block, units: replaceMatch(match, replacement) }));
            const first = carets.at(-1);
            ranges.remember(tidy(content, unique(carets.map((caret) => caret.block)), first.block, first.units));
            return true;
        });
        return matches.length;
    }

    clear() {
        if (this.#range) {
            this.#highlight()?.delete(this.#range);
            this.#range = null;
        }
    }

    #searchable(text) {
        return typeof text === 'string' && text.length > 0 && !this.#ctx.sourceView.active;
    }

    #select(range) {
        this.#ctx.ranges.remember(range);
        this.#ctx.panel.follow(range);
        this.#range = range;
        this.#highlight(true)?.add(range);
        range.startContainer.parentElement?.scrollIntoView({ block: 'nearest', inline: 'nearest' });
    }

    #highlight(create = false) {
        const registry = this.#ctx.content.ownerDocument.defaultView?.CSS?.highlights;
        const Highlight = this.#ctx.content.ownerDocument.defaultView?.Highlight;
        if (!registry || !Highlight) {
            return null;
        }
        if (!registry.has(HIGHLIGHT) && create) {
            registry.set(HIGHLIGHT, new Highlight());
        }
        return registry.get(HIGHLIGHT) ?? null;
    }
}
