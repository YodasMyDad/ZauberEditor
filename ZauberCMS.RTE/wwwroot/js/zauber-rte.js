import { Editor } from './editor.js';

export async function createEditor(root, content, source, toolbar, dotNetRef, options) {
    const editor = new Editor(root, content, source, toolbar, dotNetRef, options ?? {});
    await editor.loadStyles();
    return editor;
}
