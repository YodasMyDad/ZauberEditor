import { ZWSP } from './dom.js';
import { contentText } from './serialize.js';

const WORD_CHARACTER = /[\p{L}\p{N}]/u;

export function countContent(content, holder) {
    const text = contentText(content, holder).replaceAll(ZWSP, '');
    let characters = 0;
    for (const character of text) {
        if (character !== '\n') {
            characters++;
        }
    }
    const words = text.split(/\s+/).filter((token) => WORD_CHARACTER.test(token)).length;
    return { words, characters };
}
