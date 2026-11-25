import { Editor } from './editor.js';

export function createEditor(root, content, source, toolbar, dotNetRef, options) {
    return new Editor(root, content, source, toolbar, dotNetRef, options ?? {});
}
