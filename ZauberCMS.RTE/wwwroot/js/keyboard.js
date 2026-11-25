import { coversAllContent, isContentEmpty, resetEmptyContent } from './content.js';
import { blockOf, placeholderOf } from './dom.js';
import { insertBreak, replaceSelection } from './insert.js';
import { handleBackspace, handleDelete, handleEnter, handleTab } from './keys.js';
import { caretAt, startNode } from './selection.js';
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

function typeIntoEmpty(content, range, text) {
    if (!range.collapsed || !text) {
        return false;
    }
    const node = content.ownerDocument.createTextNode(text);
    const placeholder = placeholderOf(blockOf(startNode(range), content));
    if (placeholder) {
        placeholder.replaceWith(node);
    } else {
        range.insertNode(node);
    }
    caretAt(content, node, node.length);
    return true;
}

function clearAll(content, range) {
    if (!coversAllContent(content, range)) {
        return false;
    }
    resetEmptyContent(content);
    return true;
}

const startsFresh = (ctx, event) => event.inputType === 'insertText' && !ctx.caretMarks.holder && isContentEmpty(ctx.content);

const isPlainPaste = (event) => (event.ctrlKey || event.metaKey) && event.shiftKey && !event.altKey
    && (event.code === 'KeyV' || event.key?.toLowerCase() === 'v');

function onKeyDown(ctx, actions, event) {
    ctx.pasteTextOnce = isPlainPaste(event);
    if (event.isComposing) {
        return;
    }
    const direction = historyShortcut(event);
    if (direction !== 0) {
        event.preventDefault();
        actions.travel(direction);
        return;
    }
    const binding = matchShortcut(ctx.config.shortcuts, event);
    if (binding) {
        event.preventDefault();
        if (!ctx.config.readOnly) {
            ctx.changes.flush();
            ctx.channel.shortcut(binding.id, binding.custom);
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
        event.preventDefault();
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
    if (type === 'insertParagraph' || type === 'insertLineBreak') {
        ctx.changes.flush();
    }
    ctx.changes.begin();
    if (event.isComposing) {
        return;
    }
    if (actions.type((range) => ctx.embeds.input(range, type, event.data))) {
        event.preventDefault();
        return;
    }
    if (startsFresh(ctx, event) && actions.type((range) => typeIntoEmpty(ctx.content, range, event.data))) {
        event.preventDefault();
        return;
    }
    const text = replacementText(event);
    if (text === '' && actions.type((range) => clearAll(ctx.content, range))) {
        event.preventDefault();
        return;
    }
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
