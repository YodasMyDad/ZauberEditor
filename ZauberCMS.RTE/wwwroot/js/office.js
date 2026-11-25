const MSO_LIST = /mso-list:\s*(l\d+)\s+level(\d+)/i;
const ORDERED_MARKER = /^\(?[0-9a-z]{1,5}[.)]/i;
const STYLE_MARKS = [
    ['strong', (style) => style.fontWeight === 'bold' || Number.parseInt(style.fontWeight, 10) >= 600],
    ['em', (style) => style.fontStyle === 'italic'],
    ['u', (style) => style.textDecorationLine?.includes('underline') || style.textDecoration?.includes('underline')],
    ['s', (style) => style.textDecorationLine?.includes('line-through') || style.textDecoration?.includes('line-through')]
];

function removeComments(root) {
    const walker = root.ownerDocument.createTreeWalker(root, NodeFilter.SHOW_COMMENT);
    const comments = [];
    for (let node = walker.nextNode(); node; node = walker.nextNode()) {
        comments.push(node);
    }
    comments.forEach((comment) => comment.remove());
}

function wordListInfo(paragraph) {
    const match = MSO_LIST.exec(paragraph.getAttribute('style') ?? '');
    if (!match) {
        return null;
    }
    const marker = paragraph.querySelector('[style*="mso-list:ignore" i], [style*="mso-list: ignore" i]');
    const text = marker?.textContent.replace(/\s+/g, '') ?? '';
    marker?.remove();
    return { paragraph, level: Number(match[2]), ordered: ORDERED_MARKER.test(text) };
}

function runsOf(items) {
    const runs = [];
    for (const item of items) {
        const run = runs.at(-1);
        if (run && run.at(-1).paragraph.nextElementSibling === item.paragraph) {
            run.push(item);
        } else {
            runs.push([item]);
        }
    }
    return runs;
}

function buildList(doc, run) {
    const createList = (item) => doc.createElement(item.ordered ? 'ol' : 'ul');
    const top = createList(run[0]);
    run[0].paragraph.before(top);
    const stack = [{ level: run[0].level, list: top }];
    for (const item of run) {
        while (stack.length > 1 && item.level < stack.at(-1).level) {
            stack.pop();
        }
        if (item.level > stack.at(-1).level) {
            const nested = createList(item);
            const parent = stack.at(-1).list.lastElementChild ?? stack.at(-1).list.appendChild(doc.createElement('li'));
            parent.append(nested);
            stack.push({ level: item.level, list: nested });
        }
        const entry = doc.createElement('li');
        entry.append(...item.paragraph.childNodes);
        stack.at(-1).list.append(entry);
        item.paragraph.remove();
    }
}

function convertWordLists(root) {
    const items = [...root.querySelectorAll('p')].map(wordListInfo).filter(Boolean);
    runsOf(items).forEach((run) => buildList(root.ownerDocument, run));
}

function unwrapDocsWrappers(root) {
    for (const wrapper of root.querySelectorAll('b[id^="docs-internal-guid"], b[style*="font-weight:normal" i], b[style*="font-weight: normal" i]')) {
        wrapper.replaceWith(...wrapper.childNodes);
    }
}

function semanticSpans(root) {
    for (const span of root.querySelectorAll('span[style]')) {
        for (const [tag, test] of STYLE_MARKS) {
            if (test(span.style)) {
                const mark = span.ownerDocument.createElement(tag);
                mark.append(...span.childNodes);
                span.append(mark);
            }
        }
    }
}

function removeOfficeClasses(root) {
    for (const element of root.querySelectorAll('[class*="Mso" i]')) {
        const kept = [...element.classList].filter((name) => !/^mso/i.test(name));
        if (kept.length > 0) {
            element.className = kept.join(' ');
        } else {
            element.removeAttribute('class');
        }
    }
}

export function prepareOfficeHtml(body) {
    removeComments(body);
    removeOfficeClasses(body);
    unwrapDocsWrappers(body);
    convertWordLists(body);
    semanticSpans(body);
}
