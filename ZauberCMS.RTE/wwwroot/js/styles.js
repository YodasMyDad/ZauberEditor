const LOAD_TIMEOUT = 3000;
const SHEET_PATH = /\/_content\/ZauberCMS\.RTE\/css\/zauber-editor(?:\.[\w-]+)?\.css$/i;
const loads = new WeakMap();

const isEditorSheet = (link) => SHEET_PATH.test(new URL(link.href).pathname);

function addLink(doc, href) {
    const link = doc.createElement('link');
    link.rel = 'stylesheet';
    link.href = new URL(href, doc.baseURI).href;
    link.setAttribute('data-permanent', '');
    doc.head.insertBefore(link, doc.head.querySelector('link[rel~="stylesheet"], style'));
    return link;
}

function whenLoaded(link) {
    if (link.sheet) {
        return Promise.resolve(true);
    }
    const view = link.ownerDocument.defaultView;
    return new Promise((resolve) => {
        const controller = new AbortController();
        const settle = (loaded) => {
            controller.abort();
            view.clearTimeout(timer);
            resolve(loaded);
        };
        const timer = view.setTimeout(() => settle(false), LOAD_TIMEOUT);
        link.addEventListener('load', () => settle(true), { signal: controller.signal });
        link.addEventListener('error', () => settle(false), { signal: controller.signal });
    });
}

export function loadStylesheet(doc, href) {
    const current = loads.get(doc);
    if (current?.link.isConnected) {
        return current.loaded;
    }
    const link = [...doc.head.querySelectorAll('link[rel~="stylesheet"][href]')].find(isEditorSheet) ?? addLink(doc, href);
    const loaded = whenLoaded(link);
    loads.set(doc, { link, loaded });
    return loaded;
}
