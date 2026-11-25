const MARKER = 'data-rte-fullscreen';
const LOCK_CLASS = 'rte-scroll-lock';

function syncScrollLock(doc) {
    doc.documentElement.classList.toggle(LOCK_CLASS, doc.querySelector(`[${MARKER}]`) !== null);
}

const ownsEscape = (target) => Boolean(target?.closest?.('[role="menu"], [aria-expanded="true"]'));

export class Fullscreen {
    #ctx;
    #active = false;
    #scroll = null;

    constructor(ctx) {
        this.#ctx = ctx;
        ctx.root.addEventListener('keydown', ctx.channel.guard((event) => {
            if (event.key === 'Escape' && this.#active && !event.defaultPrevented && !ownsEscape(event.target)) {
                event.preventDefault();
                this.set(false);
            }
        }), { signal: ctx.signal });
    }

    get active() {
        return this.#active;
    }

    set(active) {
        this.#apply(active === true);
        this.#ctx.state.schedule();
    }

    dispose() {
        this.#apply(false);
    }

    #apply(active) {
        if (active === this.#active) {
            return;
        }
        this.#active = active;
        const doc = this.#ctx.root.ownerDocument;
        const view = doc.defaultView;
        if (active) {
            this.#scroll = view && { left: view.scrollX, top: view.scrollY, behavior: 'instant' };
        }
        this.#ctx.root.toggleAttribute(MARKER, active);
        syncScrollLock(doc);
        if (!active && this.#scroll) {
            view.scrollTo(this.#scroll);
            this.#scroll = null;
        }
    }
}
