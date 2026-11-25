import { EMBED_SELECTOR, closestWithin, emptyParagraph, isEmbed, isTextBlock } from './dom.js';
import { SELECTED_EMBED_CLASS } from './embeds.js';
import { caretAt, caretInside, nodeRange, selectedElement } from './selection.js';

const DELETING = /^delete(?!ByDrag)/;
const BREAKING = new Set(['insertParagraph', 'insertLineBreak']);

export function selectedEmbed(range) {
    const element = range ? selectedElement(range) : null;
    return isEmbed(element) ? element : null;
}

function removeEmbed(content, embed) {
    const next = embed.nextElementSibling;
    const previous = embed.previousElementSibling;
    if (isTextBlock(next)) {
        embed.remove();
        caretInside(content, next, false);
    } else if (isTextBlock(previous)) {
        embed.remove();
        caretInside(content, previous, true);
    } else {
        const paragraph = emptyParagraph(content.ownerDocument);
        embed.replaceWith(paragraph);
        caretInside(content, paragraph, false);
    }
}

function replaceWithText(content, embed, data) {
    const paragraph = content.ownerDocument.createElement('p');
    const text = content.ownerDocument.createTextNode(data);
    paragraph.append(text);
    embed.replaceWith(paragraph);
    caretAt(content, text, text.length);
}

function breakAfter(content, embed) {
    const paragraph = emptyParagraph(content.ownerDocument);
    embed.after(paragraph);
    caretInside(content, paragraph, false);
}

export class EmbedTool {
    #ctx;
    #marked = null;

    constructor(ctx) {
        this.#ctx = ctx;
        const options = { signal: ctx.signal };
        ctx.content.addEventListener('mouseup', (event) => {
            if (this.#target(event)) {
                event.preventDefault();
            }
        }, options);
        ctx.content.addEventListener('click', ctx.channel.guard((event) => {
            const embed = this.#target(event);
            if (embed) {
                ctx.ranges.select(nodeRange(ctx.content, embed));
            }
        }), options);
    }

    #target(event) {
        const { config, content } = this.#ctx;
        return config.readOnly ? null : closestWithin(event.target, EMBED_SELECTOR, content);
    }

    sync(range) {
        const embed = this.#ctx.config.readOnly ? null : selectedEmbed(range);
        if (embed === this.#marked) {
            return;
        }
        this.#marked?.classList.remove(SELECTED_EMBED_CLASS);
        this.#marked = embed;
        embed?.classList.add(SELECTED_EMBED_CLASS);
    }

    input(range, type, data) {
        const embed = selectedEmbed(range);
        const { content } = this.#ctx;
        if (!embed) {
            return false;
        }
        if (DELETING.test(type)) {
            removeEmbed(content, embed);
        } else if (BREAKING.has(type)) {
            breakAfter(content, embed);
        } else if (type === 'insertText' && data) {
            replaceWithText(content, embed, data);
        } else {
            return false;
        }
        return true;
    }
}
