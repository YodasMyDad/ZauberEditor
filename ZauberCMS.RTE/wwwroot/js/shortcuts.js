const signature = (mod, shift, alt, key) => `${mod ? 'm' : ''}${shift ? 's' : ''}${alt ? 'a' : ''}:${key}`;

export function compileShortcuts(shortcuts) {
    const map = new Map();
    for (const shortcut of shortcuts ?? []) {
        if (shortcut?.itemId && shortcut?.key) {
            const key = String(shortcut.key).toLowerCase();
            map.set(signature(Boolean(shortcut.mod), Boolean(shortcut.shift), Boolean(shortcut.alt), key),
                { id: shortcut.itemId, custom: shortcut.custom === true });
        }
    }
    return map;
}

function candidateKeys(event) {
    const keys = [];
    const key = event.key?.toLowerCase();
    const code = event.code ?? '';
    const digit = /^Digit(\d)$/.exec(code);
    if (digit) {
        keys.push(digit[1]);
    }
    if (key) {
        keys.push(key);
    }
    const letter = /^Key([A-Z])$/.exec(code);
    if (letter) {
        keys.push(letter[1].toLowerCase());
    }
    return keys;
}

export function matchShortcut(map, event) {
    if (map.size === 0) {
        return null;
    }
    const mod = event.ctrlKey || event.metaKey;
    for (const key of candidateKeys(event)) {
        const binding = map.get(signature(mod, event.shiftKey, event.altKey, key));
        if (binding) {
            return binding;
        }
    }
    return null;
}

export function historyShortcut(event) {
    if (!(event.ctrlKey || event.metaKey) || event.altKey) {
        return 0;
    }
    const key = event.key?.toLowerCase();
    if (key === 'z') {
        return event.shiftKey ? 1 : -1;
    }
    return key === 'y' && !event.shiftKey ? 1 : 0;
}
