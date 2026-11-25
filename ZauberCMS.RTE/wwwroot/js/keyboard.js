import { handleBackspace, handleDelete, handleEnter, handleTab } from './keys.js';
import { insertBreak, replaceSelection } from './insert.js';
import { historyShortcut, matchShortcut } from './shortcuts.js';

const TYPED_EDITS = new Map([
    ['insertParagraph', handleEnter],
    ['insertLineBreak', insertBreak],
    ['deleteContentBackward', handleBackspace],
    ['deleteContentForward', handleDelete]
]);
const DELETING = /^delete(?!ByDrag)/;

function replacementText(event) {
    if (event.inputType === 'insertText') {
        return event.data ?? null;
    }
    return DELETING.test(event.inputType) ? '' : null;
}

function onKeyDown(ctx, actions, event) {
    if (event.isComposing) {
        return;
    }
    const direction = historyShortcut(event);
    if (direction !== 0) {
        event.preventDefault();
        actions.travel(direction);
        return;
    }
    const itemId = matchShortcut(ctx.config.shortcuts, event);
    if (itemId) {
        event.preventDefault();
        if (!ctx.config.readOnly) {
            ctx.changes.flush();
            ctx.channel.shortcut(itemId);
        }
        return;
    }
    if (ctx.config.readOnly) {
        return;
    }
    if (event.key === 'Tab' && !event.ctrlKey && !event.metaKey && !event.altKey) {
        if (actions.edit((range) => handleTab(ctx.content, range, event.shiftKey), 'input')) {
            event.preventDefault();
        }
        return;
    }
    if (event.key === 'Escape' && ctx.images.selected) {
        ctx.images.deselect();
    }
}

function onBeforeInput(ctx, actions, event) {
    if (ctx.config.readOnly) {
        event.preventDefault();
        return;
    }
    const type = event.inputType;
    if (type === 'historyUndo' || type === 'historyRedo') {
        event.preventDefault();
        actions.travel(type === 'historyUndo' ? -1 : 1);
        return;
    }
    if (type.startsWith('format')) {
        event.preventDefault();
        return;
    }
    ctx.changes.begin();
    if (event.isComposing) {
        return;
    }
    const text = replacementText(event);
    if (text !== null && actions.type((range) => replaceSelection(ctx.content, range, text))) {
        event.preventDefault();
        return;
    }
    const edit = TYPED_EDITS.get(type);
    if (edit && actions.type((range) => edit(ctx.content, range))) {
        event.preventDefault();
    }
}

export function bindKeyboard(ctx, actions) {
    const options = { signal: ctx.signal };
    ctx.content.addEventListener('keydown', ctx.channel.guard((event) => onKeyDown(ctx, actions, event)), options);
    ctx.content.addEventListener('beforeinput', ctx.channel.guard((event) => onBeforeInput(ctx, actions, event)), options);
}
