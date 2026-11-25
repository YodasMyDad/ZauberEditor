import { refreshEmptyState, replaceContent } from './content.js';
import { Debouncer } from './scheduler.js';
import { formatHtml } from './serialize.js';

export class SourceView {
    #ctx;
    #debounce;
    #active = false;
    #pristine = true;
    #applied = null;

    constructor(ctx) {
        this.#ctx = ctx;
        this.#debounce = new Debouncer(ctx.channel.guard(() => this.#sync()), ctx.config.changeDelay);
        ctx.source.addEventListener('input', () => {
            if (this.#active) {
                this.#pristine = false;
                this.#debounce.delay = ctx.config.changeDelay;
                this.#debounce.schedule();
            }
        }, { signal: ctx.signal });
        ctx.source.addEventListener('blur', () => this.flush(), { signal: ctx.signal });
    }

    get active() {
        return this.#active;
    }

    enter() {
        if (this.#active) {
            return;
        }
        this.#ctx.changes.flush();
        this.#ctx.images.deselect();
        this.refresh();
        this.#active = true;
    }

    refresh() {
        this.#ctx.source.value = formatHtml(this.#ctx.content, this.#ctx.caretMarks.holder, this.#ctx.config.aliases);
        this.#pristine = true;
        this.#applied = null;
    }

    flush() {
        this.#debounce.flush();
    }

    exit() {
        if (!this.#active) {
            return;
        }
        this.#debounce.cancel();
        this.#active = false;
        if (this.#pristine) {
            return;
        }
        this.#ctx.changes.begin();
        if (this.#ctx.source.value !== this.#applied) {
            this.#apply();
        }
        this.#ctx.changes.commit('sourceView');
        this.#ctx.state.schedule();
    }

    dispose() {
        this.#debounce.cancel();
    }

    #apply() {
        const { content, source, sanitizer, ranges } = this.#ctx;
        this.#applied = source.value;
        replaceContent(content, sanitizer.clean(source.value));
        ranges.forget();
        refreshEmptyState(content);
    }

    #sync() {
        this.#apply();
        this.#ctx.changes.notify('sourceView');
    }
}
