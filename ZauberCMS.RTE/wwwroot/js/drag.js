export function startDrag(event, handle, signal, { move, end }) {
    event.preventDefault();
    const controller = new AbortController();
    const options = { signal: AbortSignal.any([signal, controller.signal]) };
    const origin = { x: event.clientX, y: event.clientY };
    let latest = null;
    let frame = 0;
    let done = false;
    const apply = () => {
        frame = 0;
        if (latest) {
            move({ deltaX: latest.clientX - origin.x, deltaY: latest.clientY - origin.y, event: latest });
        }
    };
    const finish = (finalEvent) => {
        if (done) {
            return;
        }
        done = true;
        cancelAnimationFrame(frame);
        apply();
        controller.abort();
        end?.(finalEvent);
    };
    handle.setPointerCapture(event.pointerId);
    handle.addEventListener('pointermove', (moveEvent) => {
        latest = moveEvent;
        frame ||= requestAnimationFrame(apply);
    }, options);
    handle.addEventListener('pointerup', finish, options);
    handle.addEventListener('pointercancel', finish, options);
    handle.addEventListener('lostpointercapture', finish, options);
    options.signal.addEventListener('abort', () => cancelAnimationFrame(frame), { once: true });
}
