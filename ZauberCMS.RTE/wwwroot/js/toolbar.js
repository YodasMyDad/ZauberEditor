const STEPS = new Map([['ArrowRight', 1], ['ArrowLeft', -1]]);

const inMenu = (element) => element.closest('[role="menu"]') !== null;

function controlsOf(toolbar) {
    return [...toolbar.querySelectorAll('button')].filter((button) => !button.disabled && !inMenu(button));
}

function targetOf(controls, index, key) {
    if (key === 'Home') {
        return controls[0];
    }
    if (key === 'End') {
        return controls.at(-1);
    }
    const step = STEPS.get(key);
    return step === undefined ? null : controls[(index + step + controls.length) % controls.length];
}

function moveFocus(toolbar, event) {
    if (inMenu(event.target) || event.altKey || event.ctrlKey || event.metaKey) {
        return;
    }
    const controls = controlsOf(toolbar);
    const index = controls.indexOf(event.target);
    const next = index < 0 ? null : targetOf(controls, index, event.key);
    if (next) {
        event.preventDefault();
        next.focus();
    }
}

function rove(toolbar, target) {
    if (target.localName !== 'button' || inMenu(target)) {
        return;
    }
    for (const button of controlsOf(toolbar)) {
        button.tabIndex = button === target ? 0 : -1;
    }
}

export function bindToolbar(ctx) {
    const { toolbar, signal, channel } = ctx;
    if (!toolbar) {
        return;
    }
    toolbar.addEventListener('mousedown', channel.guard((event) => {
        const button = event.target.closest('button');
        if (!button) {
            return;
        }
        if (!button.disabled) {
            ctx.changes.flush();
            ctx.sourceView.flush();
        }
        event.preventDefault();
    }), { signal });
    toolbar.addEventListener('keydown', (event) => moveFocus(toolbar, event), { signal });
    toolbar.addEventListener('focusin', (event) => rove(toolbar, event.target), { signal });
}
