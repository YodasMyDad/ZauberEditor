export class Channel {
    #reference;
    #disposed = false;

    constructor(reference) {
        this.#reference = reference;
    }

    contentChanged(html, source, revision) {
        this.#send('OnContentChanged', html, source, revision);
    }

    stateChanged(snapshot) {
        this.#send('OnStateChanged', snapshot);
    }

    shortcut(itemId) {
        this.#send('OnShortcut', itemId);
    }

    focusChanged(focused) {
        this.#send('OnFocusChanged', focused);
    }

    imageResized(width, height) {
        this.#send('OnImageResized', width, height);
    }

    panelEscape() {
        this.#send('OnPanelEscape');
    }

    error(message) {
        this.#send('OnError', message);
    }

    rejected(message) {
        this.#send('OnInputRejected', message);
    }

    guard(action) {
        return (...args) => {
            try {
                return action(...args);
            } catch (failure) {
                this.error(failure?.message ?? String(failure));
                return undefined;
            }
        };
    }

    dispose() {
        this.#disposed = true;
    }

    #send(method, ...args) {
        if (this.#disposed) {
            return;
        }
        this.#reference.invokeMethodAsync(method, ...args).catch(() => undefined);
    }
}
