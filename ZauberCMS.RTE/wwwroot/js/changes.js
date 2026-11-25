import { countContent } from './counts.js';
import { Debouncer } from './scheduler.js';
import { bookmark } from './selection.js';
import { serializeContent, snapshotHtml, utf8Chunks } from './serialize.js';

export class ChangeTracker {
    #ctx;
    #debounce;
    #dirty = false;
    #lastSent = null;
    #before = null;
    #counts = { words: 0, characters: 0 };

    constructor(ctx) {
        this.#ctx = ctx;
        this.#debounce = new Debouncer(ctx.channel.guard(() => this.flush()), ctx.config.changeDelay);
    }

    get dirty() {
        return this.#dirty;
    }

    get counts() {
        return this.#counts;
    }

    begin() {
        if (!this.#dirty) {
            this.#before = bookmark(this.#ctx.content, this.#ctx.ranges.latest());
        }
    }

    input() {
        if (!this.#dirty && this.#before === null) {
            this.begin();
        }
        this.#dirty = true;
        this.#debounce.delay = this.#ctx.config.changeDelay;
        this.#debounce.schedule();
    }

    flush() {
        this.#debounce.cancel();
        if (this.#dirty) {
            this.commit('input');
        }
    }

    commit(source, record = true) {
        this.#debounce.cancel();
        this.#dirty = false;
        if (record) {
            this.#ctx.history.record(this.#entry());
        }
        this.#before = null;
        this.notify(source);
        this.#ctx.state.schedule();
    }

    notify(source) {
        const html = serializeContent(this.#ctx.content, this.#ctx.caretMarks.holder, this.#ctx.config.aliases);
        if (html === this.#lastSent) {
            return;
        }
        this.#lastSent = html;
        this.#count();
        const { config, channel, revision } = this.#ctx;
        const { chunks, bytes } = utf8Chunks(html, config.inlineLimit);
        channel.contentChanged(bytes > config.maxContentBytes ? null : chunks, source, revision);
    }

    baseline() {
        this.#debounce.cancel();
        this.#dirty = false;
        this.#before = null;
        this.#lastSent = serializeContent(this.#ctx.content, this.#ctx.caretMarks.holder, this.#ctx.config.aliases);
        this.#count();
        this.#ctx.history.reset(this.#entry());
    }

    dispose() {
        this.#debounce.cancel();
    }

    #count() {
        this.#counts = countContent(this.#ctx.content, this.#ctx.caretMarks.holder);
        this.#ctx.state.schedule();
    }

    #entry() {
        const { content, ranges, caretMarks } = this.#ctx;
        const after = bookmark(content, ranges.latest());
        return { html: snapshotHtml(content, caretMarks.holder), bookmark: after, before: this.#before ?? after };
    }
}
