import { applyRange, caretInside, containsRange, rangeIn } from './selection.js';

export class RangeKeeper {
    #content;
    #last = null;
    #saved = null;

    constructor(content) {
        this.#content = content;
    }

    get focused() {
        const active = this.#content.ownerDocument.activeElement;
        return Boolean(active) && this.#content.contains(active);
    }

    track() {
        const range = rangeIn(this.#content);
        if (range) {
            this.#last = range.cloneRange();
        }
        return range;
    }

    current() {
        return rangeIn(this.#content);
    }

    latest() {
        return this.current() ?? this.#valid(this.#last);
    }

    ensure() {
        const range = this.latest()?.cloneRange() ?? null;
        this.#focus();
        if (range) {
            return applyRange(this.#content, range);
        }
        return caretInside(this.#content, this.#content.lastElementChild ?? this.#content, true);
    }

    select(range) {
        this.#focus();
        this.#last = range.cloneRange();
        return applyRange(this.#content, range);
    }

    remember(range) {
        this.#last = range.cloneRange();
        if (this.focused) {
            applyRange(this.#content, range);
        }
    }

    save() {
        this.#saved = this.latest()?.cloneRange() ?? null;
        return this.#saved !== null;
    }

    restore() {
        const range = this.#valid(this.#saved);
        if (!range) {
            return false;
        }
        this.select(range.cloneRange());
        return true;
    }

    clearSaved() {
        this.#saved = null;
    }

    forget() {
        this.#last = null;
        this.#saved = null;
    }

    #focus() {
        if (!this.focused) {
            this.#content.focus({ preventScroll: true });
        }
    }

    #valid(range) {
        return containsRange(this.#content, range) ? range : null;
    }
}
