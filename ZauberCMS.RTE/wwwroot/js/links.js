import { ZWSP, closestWithin } from './dom.js';
import { isSafeUrl } from './sanitize.js';
import { selectedElement, startNode } from './selection.js';

export function linkAt(content, range) {
    const selected = selectedElement(range);
    if (selected?.localName === 'a') {
        return selected;
    }
    return closestWithin(startNode(range), 'a', content);
}

export function linkInfo(link) {
    return {
        href: link.getAttribute('href') ?? '',
        text: link.textContent.replaceAll(ZWSP, ''),
        title: link.getAttribute('title'),
        target: link.getAttribute('target')
    };
}

function openLink(link) {
    const href = link.getAttribute('href');
    if (href && isSafeUrl(href)) {
        link.ownerDocument.defaultView?.open(link.href, '_blank', 'noopener,noreferrer');
    }
}

export function bindLinks(ctx) {
    const onClick = ctx.channel.guard((event) => {
        const link = closestWithin(event.target, 'a', ctx.content);
        if (!link) {
            return;
        }
        if (ctx.config.readOnly && event.button === 0 && isSafeUrl(link.getAttribute('href') ?? '')) {
            return;
        }
        event.preventDefault();
        if (event.button === 0 && (event.ctrlKey || event.metaKey)) {
            openLink(link);
        }
    });
    ctx.content.addEventListener('click', onClick, { signal: ctx.signal });
    ctx.content.addEventListener('auxclick', onClick, { signal: ctx.signal });
}
