import { tidyBreaksAtCaret } from './breaks.js';
import { normalizeRoot, refreshEmptyState } from './content.js';
import { insertFragment } from './insert.js';
import { applyRange, preserveSelection, resolveBookmark } from './selection.js';

export class Transactions {
    #ctx;
    #disposed;

    constructor(ctx, disposed) {
        this.#ctx = ctx;
        this.#disposed = disposed;
    }

    get canEdit() {
        return !this.#disposed() && !this.#ctx.config.readOnly && !this.#ctx.sourceView.active;
    }

    afterChange() {
        refreshEmptyState(this.#ctx.content);
        this.#ctx.state.schedule();
    }

    edit(action, source = 'command') {
        if (!this.canEdit) {
            return false;
        }
        const ctx = this.#ctx;
        ctx.changes.flush();
        preserveSelection(ctx.content, () => normalizeRoot(ctx.content));
        ctx.changes.begin();
        const result = action(ctx.ranges.ensure());
        if (result) {
            tidyBreaksAtCaret(ctx.content);
            ctx.changes.commit(source);
        }
        this.afterChange();
        return Boolean(result);
    }

    change(action) {
        if (!this.canEdit) {
            return false;
        }
        const ctx = this.#ctx;
        ctx.changes.flush();
        ctx.changes.begin();
        const result = action();
        if (result) {
            ctx.changes.commit('command');
        }
        this.afterChange();
        return Boolean(result);
    }

    type(action) {
        const range = this.canEdit ? this.#ctx.ranges.current() : null;
        if (!range) {
            return false;
        }
        const result = action(range);
        if (result) {
            tidyBreaksAtCaret(this.#ctx.content);
            this.#ctx.changes.input();
            this.afterChange();
        }
        return Boolean(result);
    }

    insert(fragment, source, range) {
        return this.edit((current) => {
            const { content } = this.#ctx;
            const target = range && content.contains(range.startContainer) ? applyRange(content, range) : current;
            return insertFragment(content, target, fragment);
        }, source);
    }

    travel(direction) {
        if (!this.canEdit) {
            return false;
        }
        const ctx = this.#ctx;
        ctx.changes.flush();
        const entry = direction < 0 ? ctx.history.undo() : ctx.history.redo();
        if (!entry) {
            return false;
        }
        ctx.images.deselect();
        ctx.content.replaceChildren(ctx.sanitizer.restore(entry.html));
        ctx.ranges.forget();
        const range = resolveBookmark(ctx.content, entry.bookmark);
        if (range) {
            ctx.ranges.select(range);
        }
        ctx.changes.commit('history', false);
        this.afterChange();
        return true;
    }
}
