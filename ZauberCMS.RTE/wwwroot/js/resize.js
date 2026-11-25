import { startDrag } from './drag.js';

const HANDLE_SELECTOR = '.rte-editor-resize-handle';
const MIN_HEIGHT = 80;

export function bindEditorResize(ctx) {
    const { root, content, source, signal } = ctx;
    root.addEventListener('pointerdown', ctx.channel.guard((event) => {
        const handle = event.target.closest?.(HANDLE_SELECTOR);
        if (!handle || event.button !== 0 || handle.closest('.zauber-rte') !== root) {
            return;
        }
        const visible = content.hidden ? source : content;
        const startHeight = visible.offsetHeight;
        startDrag(event, handle, signal, {
            move: ({ deltaY }) => {
                const height = `${Math.max(MIN_HEIGHT, Math.round(startHeight + deltaY))}px`;
                content.style.height = height;
                source.style.height = height;
            }
        });
    }), { signal });
}
