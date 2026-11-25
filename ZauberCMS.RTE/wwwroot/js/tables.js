import { createBreak } from './dom.js';
import { caretInside } from './selection.js';

function cellsOf(table) {
    return [...table.querySelectorAll('td, th')].filter((cell) => cell.closest('table') === table);
}

function appendRow(row) {
    const copy = row.cloneNode(false);
    copy.removeAttribute('id');
    for (const cell of row.cells) {
        const empty = row.ownerDocument.createElement(cell.localName === 'th' ? 'td' : cell.localName);
        if (cell.colSpan > 1) {
            empty.colSpan = cell.colSpan;
        }
        empty.append(createBreak(row.ownerDocument));
        copy.append(empty);
    }
    row.after(copy);
    return copy.cells[0] ?? null;
}

export function moveToCell(content, cell, backwards) {
    const table = cell.closest('table');
    const cells = cellsOf(table);
    const index = cells.indexOf(cell);
    let target = cells[index + (backwards ? -1 : 1)] ?? null;
    if (!target && !backwards) {
        target = appendRow(cell.closest('tr'));
    }
    if (!target) {
        return false;
    }
    caretInside(content, target, false);
    return true;
}
