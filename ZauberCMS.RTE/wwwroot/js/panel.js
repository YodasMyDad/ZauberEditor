const FOCUSABLE = [
    'a[href]', 'button:not([disabled])', 'input:not([disabled]):not([type="hidden"])', 'select:not([disabled])',
    'textarea:not([disabled])', '[tabindex]:not([tabindex="-1"])'
].join(', ');

const dialogOf = (panel) => panel.querySelector('[role="dialog"]') ?? panel;

const focusablesIn = (element) => [...element.querySelectorAll(FOCUSABLE)].filter((candidate) => candidate.getClientRects().length > 0);

function trapTab(event, panel) {
    const items = focusablesIn(panel);
    if (items.length === 0) {
        event.preventDefault();
        return;
    }
    const active = panel.ownerDocument.activeElement;
    const first = items[0];
    const last = items.at(-1);
    if (!panel.contains(active)) {
        event.preventDefault();
        first.focus();
    } else if (event.shiftKey && active === first) {
        event.preventDefault();
        last.focus();
    } else if (!event.shiftKey && active === last) {
        event.preventDefault();
        first.focus();
    }
}

export class PanelHost {
    #ctx;
    #panel = null;
    #placeholder = null;
    #session = null;
    #range = null;

    constructor(ctx) {
        this.#ctx = ctx;
    }

    open(panel) {
        if (this.#panel !== panel) {
            this.#release();
            this.#attach(panel);
        }
        this.#focusFirst();
    }

    close() {
        if (!this.#panel) {
            return;
        }
        const range = this.#range;
        this.#release();
        const { ranges } = this.#ctx;
        if (ranges.current()) {
            ranges.ensure();
        } else if (range && this.#ctx.content.contains(range.startContainer)) {
            ranges.select(range);
        }
    }

    dispose() {
        this.#release();
    }

    #attach(panel) {
        const doc = panel.ownerDocument;
        this.#range = this.#ctx.ranges.latest()?.cloneRange() ?? null;
        this.#placeholder = doc.createComment('zauber-rte-panel');
        panel.before(this.#placeholder);
        doc.body.append(panel);
        this.#panel = panel;
        this.#session = new AbortController();
        const signal = AbortSignal.any([this.#ctx.signal, this.#session.signal]);
        panel.addEventListener('keydown', this.#ctx.channel.guard((event) => {
            if (event.key === 'Escape') {
                event.preventDefault();
                event.stopPropagation();
                this.#ctx.channel.panelEscape();
            } else if (event.key === 'Tab') {
                trapTab(event, dialogOf(panel));
            }
        }), { signal });
    }

    #release() {
        if (!this.#panel) {
            return;
        }
        this.#session.abort();
        if (this.#placeholder.isConnected) {
            this.#placeholder.replaceWith(this.#panel);
        } else {
            this.#panel.remove();
        }
        this.#panel = null;
        this.#placeholder = null;
        this.#session = null;
        this.#range = null;
    }

    #focusFirst() {
        const dialog = dialogOf(this.#panel);
        const content = dialog.querySelector('.rte-slideout-content');
        const target = (content && focusablesIn(content)[0]) ?? focusablesIn(dialog)[0];
        target?.focus({ preventScroll: true });
    }
}
