const MAX_CHARACTERS = 16_000_000;

export class History {
    #entries = [];
    #index = -1;
    #characters = 0;
    limit;

    constructor(limit) {
        this.limit = limit;
    }

    get canUndo() {
        return this.#index > 0;
    }

    get canRedo() {
        return this.#index < this.#entries.length - 1;
    }

    get current() {
        return this.#entries[this.#index] ?? null;
    }

    reset(entry) {
        this.#entries = [entry];
        this.#index = 0;
        this.#characters = entry.html.length;
    }

    record(entry) {
        const current = this.current;
        if (current?.html === entry.html) {
            current.bookmark = entry.bookmark;
            return false;
        }
        for (const dropped of this.#entries.splice(this.#index + 1)) {
            this.#characters -= dropped.html.length;
        }
        this.#entries.push(entry);
        this.#characters += entry.html.length;
        this.#index = this.#entries.length - 1;
        this.#trim();
        return true;
    }

    undo() {
        const from = this.current;
        const target = this.#go(-1);
        return target && { html: target.html, bookmark: from.before ?? target.bookmark };
    }

    redo() {
        return this.#go(1);
    }

    #go(delta) {
        const target = this.#index + delta;
        if (target < 0 || target >= this.#entries.length) {
            return null;
        }
        this.#index = target;
        return this.#entries[target];
    }

    #trim() {
        while (this.#entries.length > 1 && (this.#entries.length > this.limit + 1 || this.#characters > MAX_CHARACTERS)) {
            this.#characters -= this.#entries.shift().html.length;
            this.#index--;
        }
    }
}
