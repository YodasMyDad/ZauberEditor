export class Debouncer {
    #callback;
    #timer = 0;
    delay;

    constructor(callback, delay) {
        this.#callback = callback;
        this.delay = delay;
    }

    get pending() {
        return this.#timer !== 0;
    }

    schedule() {
        clearTimeout(this.#timer);
        this.#timer = setTimeout(() => {
            this.#timer = 0;
            this.#callback();
        }, this.delay);
    }

    flush() {
        if (this.pending) {
            this.cancel();
            this.#callback();
        }
    }

    cancel() {
        clearTimeout(this.#timer);
        this.#timer = 0;
    }
}

export class FrameScheduler {
    #callback;
    #handle = 0;

    constructor(callback) {
        this.#callback = callback;
    }

    schedule() {
        if (this.#handle === 0) {
            this.#handle = requestAnimationFrame(() => {
                this.#handle = 0;
                this.#callback();
            });
        }
    }

    cancel() {
        cancelAnimationFrame(this.#handle);
        this.#handle = 0;
    }
}
