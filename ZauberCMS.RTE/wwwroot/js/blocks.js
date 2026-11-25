import {
    CELL_TAGS, HEADING_TAGS, closestWithin, convertNewlines, retag, setAttributes, unique, unwrapElement, wrapInlineRuns
} from './dom.js';
import { liftToBlock } from './lists.js';
import { preserveSelection } from './selection.js';
import { RETAGGABLE_TAGS, asTextBlock, textBlocksInRange } from './text-blocks.js';

const FLOW_PARENTS = new Set(['td', 'th', 'li']);

function convertBlock(block, type, attributes) {
    if (block.localName === 'li') {
        return liftToBlock(block, type, attributes);
    }
    const target = asTextBlock(block);
    if (target === block && !RETAGGABLE_TAGS.has(block.localName)) {
        return block;
    }
    if (target.localName === 'pre' && type !== 'pre') {
        convertNewlines(target, true);
    } else if (type === 'pre' && target.localName !== 'pre') {
        convertNewlines(target, false);
    }
    const converted = target.localName === type ? target : retag(target, type);
    return setAttributes(converted, attributes);
}

export function setBlockType(content, range, type, attributes) {
    const blocks = textBlocksInRange(content, range);
    if (blocks.length === 0) {
        return false;
    }
    const toggleOff = type !== 'p' && blocks.every((block) => block.localName === type);
    const target = toggleOff ? 'p' : type;
    preserveSelection(content, () => {
        if (target === 'p') {
            unwrapQuotes(content, blocks);
        }
        blocks.filter((block) => block.isConnected).forEach((block) => convertBlock(block, target, attributes));
    });
    return true;
}

function unitOf(block, content) {
    let unit = block;
    while (unit.parentElement && unit.parentElement !== content && !FLOW_PARENTS.has(unit.parentElement.localName)) {
        unit = unit.parentElement;
    }
    return unit;
}

function groupSiblings(units) {
    const groups = [];
    for (const unit of units) {
        const group = groups.at(-1);
        if (group && group.at(-1).nextElementSibling === unit) {
            group.push(unit);
        } else {
            groups.push([unit]);
        }
    }
    return groups;
}

export function wrapBlocks(content, range, tag, attributes) {
    const blocks = textBlocksInRange(content, range).filter((block) => !closestWithin(block, tag, content));
    if (blocks.length === 0) {
        return false;
    }
    preserveSelection(content, () => {
        const units = unique(blocks.map((block) => unitOf(CELL_TAGS.has(block.localName) ? asTextBlock(block) : block, content)));
        for (const group of groupSiblings(units)) {
            const wrapper = setAttributes(content.ownerDocument.createElement(tag), attributes);
            group[0].before(wrapper);
            wrapper.append(...group);
        }
    });
    return true;
}

function unwrapBlock(element) {
    wrapInlineRuns(element);
    unwrapElement(element);
}

export function unwrapBlocks(content, range, tags) {
    const selector = [...tags].join(', ');
    const wrappers = unique(textBlocksInRange(content, range).map((block) => closestWithin(block, selector, content)).filter(Boolean));
    if (wrappers.length === 0) {
        return false;
    }
    preserveSelection(content, () => wrappers.forEach(unwrapBlock));
    return true;
}

function unwrapQuotes(content, blocks) {
    const quotes = blocks.map((block) => (block.localName === 'blockquote' ? block : closestWithin(block, 'blockquote', content)));
    unique(quotes.filter(Boolean)).forEach(unwrapBlock);
}

export function toggleWrapper(content, range, tag, attributes) {
    const blocks = textBlocksInRange(content, range);
    if (blocks.length > 0 && blocks.every((block) => closestWithin(block, tag, content))) {
        return unwrapBlocks(content, range, new Set([tag]));
    }
    return wrapBlocks(content, range, tag, attributes);
}

export function setBlockStyle(content, range, styles) {
    const blocks = textBlocksInRange(content, range);
    for (const block of blocks) {
        for (const [property, value] of styles) {
            if (value === null || value === '') {
                block.style.removeProperty(property);
            } else {
                block.style.setProperty(property, value);
            }
        }
        if (block.getAttribute('style') === '') {
            block.removeAttribute('style');
        }
    }
    return blocks.length > 0;
}

export function resetBlocks(content, range) {
    const blocks = textBlocksInRange(content, range);
    preserveSelection(content, () => {
        unwrapQuotes(content, blocks);
        for (const block of blocks.filter((candidate) => candidate.isConnected)) {
            const plain = block.localName === 'li' || HEADING_TAGS.has(block.localName) || block.localName === 'pre'
                ? convertBlock(block, 'p', [])
                : block;
            plain.removeAttribute('style');
            plain.removeAttribute('class');
        }
    });
    return blocks.length > 0;
}
