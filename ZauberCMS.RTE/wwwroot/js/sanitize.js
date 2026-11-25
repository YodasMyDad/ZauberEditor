import DOMPurify from './lib/purify.es.mjs';

const SAFE_URI = /^(?:(?:https?|mailto|tel):|[^a-z]|[a-z+.-]+(?:[^a-z+.\-:]|$))/i;
const CONTROL_CHARACTERS = /[\u0000- \u007F-\u009F]/g;
const UNSAFE_STYLE = /[\\@]|\/\*|(?:url|src|image|image-set|element|expression)\s*\(|(?:behavior|binding)\s*:|position\s*:\s*(?:fixed|sticky)/i;
const IMAGE_DATA_URL = /^data:image\/(?:png|jpe?g|gif|webp|avif|bmp|svg\+xml)[;,]/i;
const DENIED_TAGS = new Set([
    'base', 'button', 'embed', 'form', 'frame', 'frameset', 'iframe', 'input', 'link', 'math', 'meta', 'noscript',
    'object', 'script', 'select', 'style', 'svg', 'template', 'textarea'
]);

const safeDeclarations = (style) => style.split(';').filter((declaration) => !UNSAFE_STYLE.test(declaration)).join(';').trim();

export function isSafeUrl(url) {
    return typeof url === 'string' && SAFE_URI.test(url.replace(CONTROL_CHARACTERS, ''));
}

function isAllowedImageSource(value, policy, doc) {
    const source = value.replace(CONTROL_CHARACTERS, '');
    if (/^data:/i.test(source)) {
        return policy.allowDataUrls && IMAGE_DATA_URL.test(source);
    }
    if (!SAFE_URI.test(source) || /^(?:mailto|tel):/i.test(source)) {
        return false;
    }
    if (policy.allowExternalImages) {
        return true;
    }
    try {
        return new URL(source, doc.baseURI).origin === new URL(doc.baseURI).origin;
    } catch {
        return false;
    }
}

function allowedEverywhere(policy, name) {
    return policy.allowedAttributes.has(name)
        || (name.startsWith('data-') && policy.allowedAttributes.has('data-*'))
        || (name.startsWith('aria-') && policy.allowedAttributes.has('aria-*'));
}

function allowedOn(policy, tag, name) {
    return allowedEverywhere(policy, name) || policy.tagAttributes.get(tag)?.has(name) === true;
}

function policyConfig(policy) {
    const attributes = [...policy.allowedAttributes];
    const tagOnly = [...policy.tagAttributes.values()].flatMap((names) => [...names]);
    return {
        ALLOWED_TAGS: [...policy.allowedTags].filter((tag) => !DENIED_TAGS.has(tag)),
        ALLOWED_ATTR: [...new Set([...attributes, ...tagOnly])].filter((name) => !name.includes('*') && !/^on/i.test(name)),
        ALLOW_DATA_ATTR: attributes.includes('data-*'),
        ALLOW_ARIA_ATTR: attributes.includes('aria-*'),
        KEEP_CONTENT: true
    };
}

function createPurifier(config, policy, doc, scoped) {
    const purifier = DOMPurify();
    purifier.setConfig({ ...config, ALLOWED_URI_REGEXP: SAFE_URI, RETURN_DOM_FRAGMENT: true });
    purifier.addHook('uponSanitizeAttribute', (node, data) => {
        if (scoped && !allowedOn(policy, node.localName, data.attrName)) {
            data.keepAttr = false;
        } else if (data.attrName === 'src' && node.localName === 'img' && !isAllowedImageSource(data.attrValue, policy, doc)) {
            data.keepAttr = false;
        } else if (data.attrName === 'style') {
            data.attrValue = safeDeclarations(data.attrValue);
            data.keepAttr = data.attrValue !== '';
        }
    });
    purifier.addHook('afterSanitizeAttributes', (node) => {
        if (node.localName === 'a' && node.getAttribute('target') === '_blank' && node.hasAttribute('rel')) {
            node.setAttribute('rel', 'noopener noreferrer');
        }
    });
    return purifier;
}

export class Sanitizer {
    #policy;
    #doc;
    #strict;
    #loose;

    constructor(policy, doc) {
        this.#policy = policy;
        this.#doc = doc;
        this.#strict = createPurifier(policyConfig(policy), policy, doc, true);
        this.#loose = createPurifier({ USE_PROFILES: { html: true } }, policy, doc, false);
    }

    clean(html) {
        return this.#finish(this.#strict.sanitize(String(html ?? '')));
    }

    restore(html) {
        return this.#finish(this.#loose.sanitize(String(html ?? '')));
    }

    allowsTag(tag) {
        return !DENIED_TAGS.has(tag) && this.#policy.allowedTags.has(tag);
    }

    isAllowedAttribute(tag, name, value) {
        if (typeof value !== 'string' || !/^[a-z_:][-a-z0-9_:.]*$/i.test(name) || /^on/i.test(name)
            || !allowedOn(this.#policy, tag.toLowerCase(), name.toLowerCase())) {
            return false;
        }
        if (name.toLowerCase() === 'href') {
            return isSafeUrl(value) && this.#strict.isValidAttribute(tag, name, value);
        }
        if (name.toLowerCase() === 'style' && safeDeclarations(value) !== value.trim()) {
            return false;
        }
        if (name.toLowerCase() === 'src') {
            return isAllowedImageSource(value, this.#policy, this.#doc) && this.#strict.isValidAttribute(tag, name, value);
        }
        return this.#strict.isValidAttribute(tag, name, value);
    }

    #finish(fragment) {
        for (const image of fragment.querySelectorAll('img:not([src])')) {
            image.remove();
        }
        return fragment;
    }
}
