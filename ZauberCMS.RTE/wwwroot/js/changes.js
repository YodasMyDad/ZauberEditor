import { Debouncer } from './scheduler.js';
import { bookmark } from './selection.js';
import { fitsInline, serializeContent, snapshotHtml } from './serialize.js';

export class ChangeTracker {
    #ctx;
    #debounce;
    #dirty = false;
    #lastSent = null;
    #before = null;

    constructor(ctx) {
        this.#ctx = ctx;
        this.#debounce = new Debouncer(ctx.channel.guard(() => this.flush()), ctx.config.changeDelay);
    }

    get dirty() {
        return this.#dirty;
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
        const html = serializeContent(this.#ctx.content, this.#ctx.caretMarks.holder);
        if (html === this.#lastSent) {
            return;
        }
        this.#lastSent = html;
        this.#ctx.channel.contentChanged(fitsInline(html, this.#ctx.config.inlineLimit) ? html : null, source, this.#ctx.revision);
    }

    baseline() {
        this.#debounce.cancel();
        this.#dirty = false;
        this.#before = null;
        this.#lastSent = serializeContent(this.#ctx.content, this.#ctx.caretMarks.holder);
        this.#ctx.history.reset(this.#entry());
    }

    dispose() {
        this.#debounce.cancel();
    }

    #entry() {
        const { content, ranges, caretMarks } = this.#ctx;
        const after = bookmark(content, ranges.latest());
        return { html: snapshotHtml(content, caretMarks.holder), bookmark: after, before: this.#before ?? after };
    }
}
