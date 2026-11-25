import { CELL_TAGS, closestWithin } from './dom.js';
import { indentItems, outdentItems } from './lists.js';
import { textBlocksInRange } from './text-blocks.js';

const STEP_EM = 2;
const MAX_LEVEL = 10;
const PROPERTY = 'margin-inline-start';

export function indentLevel(element) {
    const value = element.style?.getPropertyValue(PROPERTY) ?? '';
    return value.endsWith('em') ? Math.max(0, Math.round(Number.parseFloat(value) / STEP_EM)) : 0;
}

function shiftBlock(block, delta) {
    const current = indentLevel(block);
    const level = Math.min(MAX_LEVEL, Math.max(0, current + delta));
    if (level === current) {
        return false;
    }
    if (level === 0) {
        block.style.removeProperty(PROPERTY);
    } else {
        block.style.setProperty(PROPERTY, `${level * STEP_EM}em`);
    }
    if (block.getAttribute('style') === '') {
        block.removeAttribute('style');
    }
    return true;
}

export function changeIndent(content, range, delta) {
    const blocks = textBlocksInRange(content, range);
    const inList = blocks.some((block) => closestWithin(block, 'li', content));
    const others = blocks.filter((block) => !closestWithin(block, 'li', content) && !CELL_TAGS.has(block.localName));
    let changed = inList && (delta > 0 ? indentItems(content, range) : outdentItems(content, range));
    for (const block of others) {
        changed = shiftBlock(block, delta) || changed;
    }
    return changed;
}
