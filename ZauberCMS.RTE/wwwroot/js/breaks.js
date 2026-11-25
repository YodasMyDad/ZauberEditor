import {
    blockOf, createBreak, hasBlockChild, hasOwnContent, hasVisibleText, isAtomic, isBlock, isContainer, isText,
    removeWithEmptyInlines
} from './dom.js';
import { preserveSelection, rangeIn, startNode } from './selection.js';

const isBreak = (node) => node?.localName === 'br';

function enclosingBlock(node, root) {
    let element = node.parentNode;
    while (element !== root && !isBlock(element)) {
        element = element.parentNode;
    }
    return element;
}

function flowNeighbour(br, root, forwards) {
    const block = enclosingBlock(br, root);
    const walker = br.ownerDocument.createTreeWalker(block, NodeFilter.SHOW_ELEMENT | NodeFilter.SHOW_TEXT, {
        acceptNode: (node) => ((isText(node) ? hasVisibleText(node.data) : isAtomic(node) || isBlock(node))
            ? NodeFilter.FILTER_ACCEPT
            : NodeFilter.FILTER_SKIP)
    });
    walker.currentNode = br;
    const found = forwards ? walker.nextNode() : walker.previousNode();
    return found && !isBlock(found) && enclosingBlock(found, root) === block ? found : null;
}

function strayBreaks(root, keepPlaceholders) {
    return [...root.querySelectorAll('br')].filter((br) => {
        if (flowNeighbour(br, root, true)) {
            return false;
        }
        const previous = flowNeighbour(br, root, false);
        return previous ? !isBreak(previous) : !keepPlaceholders;
    });
}

const removeAll = (breaks, root) => breaks.forEach((br) => removeWithEmptyInlines(br, root));

export function hasStrayBreaks(root) {
    return strayBreaks(root, false).length > 0;
}

export function removeStrayBreaks(root, keepPlaceholders = false) {
    removeAll(strayBreaks(root, keepPlaceholders), root);
}

const needsPlaceholder = (element) => isBlock(element) && !isContainer(element) && !hasBlockChild(element)
    && !hasOwnContent(element) && !Array.prototype.some.call(element.childNodes, isBreak);

export function tidyBreaks(root) {
    removeStrayBreaks(root, true);
    for (const element of [...root.querySelectorAll('*')].filter(needsPlaceholder)) {
        element.append(createBreak(element.ownerDocument));
    }
}

export function tidyBreaksAtCaret(content) {
    const range = rangeIn(content);
    const block = range && blockOf(startNode(range), content);
    if (!block?.querySelector('br')) {
        return;
    }
    const stray = strayBreaks(block, true);
    if (stray.length > 0) {
        preserveSelection(content, () => removeAll(stray, block));
    }
}
