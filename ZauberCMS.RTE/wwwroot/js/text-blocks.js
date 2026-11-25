import { MEDIA_SELECTOR, blockOf, elementOf, isTextBlock, wrapInlineRuns } from './dom.js';
import { isRangeEmpty, startNode } from './selection.js';

export const RETAGGABLE_TAGS = new Set(['p', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'pre', 'div', 'address']);

function endsAtStartOf(range, block) {
    const probe = block.ownerDocument.createRange();
    probe.setStart(block, 0);
    probe.setEnd(range.endContainer, range.endOffset);
    return isRangeEmpty(probe) && !probe.cloneContents().querySelector(MEDIA_SELECTOR);
}

export function textBlocksInRange(content, range) {
    if (range.collapsed) {
        const block = blockOf(startNode(range), content);
        return block ? [block] : [];
    }
    const scope = elementOf(range.commonAncestorContainer);
    const ancestors = [];
    for (let element = scope; element && element !== content; element = element.parentElement) {
        if (isTextBlock(element)) {
            ancestors.unshift(element);
        }
    }
    const descendants = [];
    const walker = content.ownerDocument.createTreeWalker(scope, NodeFilter.SHOW_ELEMENT, {
        acceptNode: (node) => {
            if (!range.intersectsNode(node)) {
                return NodeFilter.FILTER_REJECT;
            }
            return isTextBlock(node) ? NodeFilter.FILTER_ACCEPT : NodeFilter.FILTER_SKIP;
        }
    });
    for (let node = walker.nextNode(); node; node = walker.nextNode()) {
        descendants.push(node);
    }
    const blocks = [...ancestors, ...descendants];
    if (blocks.length > 1 && endsAtStartOf(range, blocks.at(-1))) {
        blocks.pop();
    }
    return blocks;
}

export function asTextBlock(block) {
    if (RETAGGABLE_TAGS.has(block.localName) || block.localName === 'li') {
        return block;
    }
    wrapInlineRuns(block);
    return block.firstElementChild && isTextBlock(block.firstElementChild) ? block.firstElementChild : block;
}
