import { tidyBreaksAtCaret } from './breaks.js';
import { isContentEmpty, normalizeAtCaret, refreshEmptyState, resetEmptyContent } from './content.js';

export function bindContentEvents(ctx) {
    const { content, channel, signal } = ctx;
    const options = { signal };
    let composing = false;
    content.addEventListener('compositionstart', () => {
        composing = true;
    }, options);
    content.addEventListener('compositionend', channel.guard(() => {
        composing = false;
        ctx.caretMarks.settle();
    }), options);
    content.addEventListener('input', channel.guard((event) => {
        if (!event.isComposing) {
            if (event.inputType?.startsWith('delete') && !ctx.caretMarks.holder && isContentEmpty(content)) {
                resetEmptyContent(content);
            }
            normalizeAtCaret(content);
            tidyBreaksAtCaret(content);
            ctx.caretMarks.settle();
        }
        ctx.changes.input();
        refreshEmptyState(content);
        ctx.state.schedule();
    }), options);
    content.addEventListener('focus', channel.guard(() => {
        channel.focusChanged(true);
        ctx.state.schedule();
    }), options);
    content.addEventListener('blur', channel.guard(() => {
        ctx.caretMarks.settle(true);
        ctx.changes.flush();
        channel.focusChanged(false);
        ctx.state.schedule();
    }), options);
    ctx.darkScheme?.addEventListener('change', () => ctx.state.schedule(), options);
    content.ownerDocument.addEventListener('selectionchange', channel.guard(() => {
        const range = ctx.ranges.track();
        if (range) {
            ctx.images.sync(range);
            if (!composing) {
                ctx.caretMarks.settle();
            }
            ctx.state.schedule();
        } else if (ctx.state.engaged) {
            ctx.state.schedule();
        }
    }), options);
}
