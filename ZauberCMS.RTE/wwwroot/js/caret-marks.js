import { ZWSP, elementOf, hasAnyContent, removeWithEmptyInlines, splitAt } from './dom.js';
import { caretAt, rangeIn } from './selection.js';

export class CaretMarks {
    #content;
    #holder = null;

    constructor(content) {
        this.#content = content;
    }

    enter(range, tag) {
        const doc = this.#content.ownerDocument;
        const element = doc.createElement(tag);
        const text = doc.createTextNode(ZWSP);
        element.append(text);
        range.insertNode(element);
        this.#hold(text);
        return false;
    }

    exit(range, mark) {
        const doc = this.#content.ownerDocument;
        const text = doc.createTextNode(ZWSP);
        let outer = text;
        for (let element = elementOf(range.startContainer); element && element !== mark; element = element.parentElement) {
            const shell = element.cloneNode(false);
            shell.removeAttribute('id');
            shell.append(outer);
            outer = shell;
        }
        const tail = splitAt(mark, range.startContainer, range.startOffset);
        mark.after(outer);
        for (const piece of [mark, tail]) {
            if (!hasAnyContent(piece)) {
                piece.remove();
            }
        }
        this.#hold(text);
        return false;
    }

    get holder() {
        return this.#holder?.isConnected && this.#holder.data === ZWSP ? this.#holder : null;
    }

    settle(force = false) {
        const text = this.#holder;
        if (!text) {
            return;
        }
        if (!text.isConnected) {
            this.#holder = null;
            return;
        }
        if (text.data !== ZWSP) {
            const index = text.data.indexOf(ZWSP);
            if (index >= 0) {
                text.deleteData(index, 1);
            }
            this.#holder = null;
            return;
        }
        if (!force && rangeIn(this.#content)?.startContainer === text) {
            return;
        }
        this.#release(text);
    }

    #hold(text) {
        this.settle(true);
        this.#holder = text;
        caretAt(this.#content, text, 1);
    }

    #release(text) {
        this.#holder = null;
        removeWithEmptyInlines(text, this.#content);
    }
}
