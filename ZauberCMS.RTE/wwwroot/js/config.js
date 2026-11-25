import { createProviders } from './embeds.js';
import { compileShortcuts } from './shortcuts.js';

const DEFAULT_IMAGE_TYPES = ['.jpg', '.jpeg', '.png', '.gif', '.webp'];
const DEFAULT_MAX_FILE_SIZE = 5 * 1024 * 1024;

const lower = (values) => (values ?? []).filter((value) => typeof value === 'string').map((value) => value.toLowerCase());
const atLeast = (value, minimum, fallback) => (Number.isFinite(value) && value >= minimum ? value : fallback);
const optionalSize = (value) => (Number.isFinite(value) && value > 0 ? value : null);

function createImageRules(image) {
    return {
        maintainAspectRatio: image?.maintainAspectRatio !== false,
        maxWidth: optionalSize(image?.maxWidth),
        maxHeight: optionalSize(image?.maxHeight),
        allowBase64Upload: image?.allowBase64Upload === true,
        maxFileSizeBytes: atLeast(image?.maxFileSizeBytes, 0, DEFAULT_MAX_FILE_SIZE),
        allowedTypes: new Set(image?.allowedTypes ? lower(image.allowedTypes) : DEFAULT_IMAGE_TYPES)
    };
}

function createTagAttributes(tagAttributes) {
    return new Map(Object.entries(tagAttributes ?? {})
        .filter(([, names]) => Array.isArray(names))
        .map(([tag, names]) => [tag.toLowerCase(), new Set(lower(names))]));
}

function createPolicy(policy) {
    return {
        allowedTags: new Set(lower(policy?.allowedTags)),
        allowedAttributes: new Set(lower(policy?.allowedAttributes)),
        tagAttributes: createTagAttributes(policy?.tagAttributes),
        allowDataUrls: policy?.allowDataUrls !== false,
        allowExternalImages: policy?.allowExternalImages !== false
    };
}

export function createConfig(options) {
    const aliases = new Map(Object.entries(options.tagAliases ?? {})
        .filter(([, primary]) => typeof primary === 'string')
        .map(([alias, primary]) => [alias.toLowerCase(), primary.toLowerCase()]));
    return {
        readOnly: options.readOnly === true,
        placeholder: typeof options.placeholder === 'string' && options.placeholder !== '' ? options.placeholder : null,
        tracked: new Set(lower(options.trackedTags)),
        aliases,
        shortcuts: compileShortcuts(options.shortcuts),
        policy: createPolicy(options.policy),
        historyLimit: atLeast(options.historyLimit, 1, 100),
        changeDelay: atLeast(options.changeDelay, 0, 300),
        inlineLimit: atLeast(options.inlineLimit, 4, 12000),
        maxContentBytes: atLeast(options.maxContentBytes, 0, 10 * 1024 * 1024),
        image: createImageRules(options.image),
        embeds: createProviders(options.embeds)
    };
}

export function primaryMark(config, tag) {
    const primary = config.aliases.get(tag) ?? tag;
    return config.tracked.has(primary) || config.tracked.has(tag) ? primary : null;
}

export function markFamily(config, tag) {
    const primary = config.aliases.get(tag) ?? tag;
    const tags = new Set([primary, tag]);
    for (const [alias, target] of config.aliases) {
        if (target === primary) {
            tags.add(alias);
        }
    }
    return { primary, tags };
}
