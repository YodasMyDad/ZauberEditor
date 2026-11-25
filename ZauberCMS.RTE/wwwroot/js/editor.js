import { resetBlocks, setBlockStyle, setBlockType, toggleWrapper, unwrapBlocks, wrapBlocks } from './blocks.js';
import { CaretMarks } from './caret-marks.js';
import { ChangeTracker } from './changes.js';
import { Channel } from './channel.js';
import { bindClipboard, normalizeFragment } from './clipboard.js';
import { createConfig, markFamily } from './config.js';
import { normalizeRoot, refreshEmptyState, replaceContent } from './content.js';
import { BLOCK_TAGS, LIST_TAGS, blockOf } from './dom.js';
import { bindContentEvents } from './events.js';
import { History } from './history.js';
import { ImageTool, imageAt, imageInfo } from './images.js';
import { insertFragment, textFragment } from './insert.js';
import { bindKeyboard } from './keyboard.js';
import { bindLinks, linkAt, linkInfo } from './links.js';
import { clearInline, toggleMark, unwrapInline, wrapInline } from './marks.js';
import { toggleList } from './lists.js';
import { PanelHost } from './panel.js';
import { RangeKeeper } from './ranges.js';
import { bindEditorResize } from './resize.js';
import { Sanitizer } from './sanitize.js';
import { applyRange, nodeRange, preserveSelection, resolveBookmark, startNode } from './selection.js';
import { contentText, jsonBytes, serializeContent, toUtf8 } from './serialize.js';
import { SourceView } from './source-view.js';
import { StateTracker } from './state.js';
import { bindToolbar } from './toolbar.js';
import { blockType, inlineTag, validAttributes, validStyles, wrapperName, wrapperTag } from './validation.js';

export class Editor {
    #ctx;
    #controller = new AbortController();
    #disposed = false;

    constructor(root, content, source, toolbar, dotNetRef, options) {
        const channel = new Channel(dotNetRef);
        const config = createConfig(options);
        const ctx = {
            root, content, source, toolbar, channel, config,
            signal: this.#controller.signal,
            revision: 0,
            options: { ...options },
            sanitizer: new Sanitizer(config.policy, content.ownerDocument),
            history: new History(config.historyLimit),
            ranges: new RangeKeeper(content),
            caretMarks: new CaretMarks(content),
            darkScheme: content.ownerDocument.defaultView?.matchMedia?.('(prefers-color-scheme: dark)') ?? null
        };
        this.#ctx = ctx;
        ctx.changes = new ChangeTracker(ctx);
        ctx.state = new StateTracker(ctx);
        ctx.images = new ImageTool(ctx);
        ctx.sourceView = new SourceView(ctx);
        ctx.panel = new PanelHost(ctx);
        const actions = {
            edit: (action, changeSource) => this.#edit(action, changeSource),
            type: (action) => this.#type(action),
            travel: (direction) => this.#travel(direction),
            insert: (fragment, changeSource, range) => this.#insert(fragment, changeSource, range)
        };
        bindContentEvents(ctx);
        bindKeyboard(ctx, actions);
        bindClipboard(ctx, actions);
        bindLinks(ctx);
        bindToolbar(ctx);
        bindEditorResize(ctx);
        this.#applyConfig();
        this.setHtml(options.html ?? '', 'parameter');
    }

    setHtml(html, source = 'api', revision = 0) {
        const ctx = this.#ctx;
        if (source === 'parameter') {
            ctx.revision = Number.isInteger(revision) ? revision : 0;
        }
        if (this.#disposed) {
            return;
        }
        const fragment = ctx.sanitizer.clean(html);
        const replace = () => {
            ctx.images.deselect();
            replaceContent(ctx.content, fragment);
            ctx.ranges.forget();
        };
        if (source === 'parameter') {
            replace();
            ctx.changes.baseline();
            this.#afterChange();
        } else {
            ctx.changes.flush();
            ctx.changes.begin();
            replace();
            ctx.changes.commit('api');
            this.#afterChange();
        }
        if (ctx.sourceView.active) {
            ctx.sourceView.refresh();
        }
    }

    getHtmlStream() {
        this.#ctx.sourceView.flush();
        return toUtf8(serializeContent(this.#ctx.content, this.#ctx.caretMarks.holder));
    }

    getTextStream() {
        this.#ctx.sourceView.flush();
        return toUtf8(contentText(this.#ctx.content, this.#ctx.caretMarks.holder));
    }

    getState() {
        return this.#ctx.state.snapshot();
    }

    toggleMark(tag) {
        const name = inlineTag(this.#ctx, tag);
        if (!name) {
            return false;
        }
        return this.#edit((range) => toggleMark(this.#ctx.content, range, this.#ctx.config, name, this.#ctx.caretMarks));
    }

    wrap(tag, attributes) {
        const name = wrapperTag(this.#ctx, tag);
        const entries = name && validAttributes(this.#ctx, name, attributes);
        if (!entries) {
            return false;
        }
        if (BLOCK_TAGS.has(name)) {
            return this.#edit((range) => wrapBlocks(this.#ctx.content, range, name, entries));
        }
        return this.#edit((range) => wrapInline(this.#ctx.content, range, name, entries));
    }

    unwrap(tag) {
        const name = wrapperName(tag);
        const tags = markFamily(this.#ctx.config, name).tags;
        if (BLOCK_TAGS.has(name)) {
            return this.#edit((range) => unwrapBlocks(this.#ctx.content, range, tags));
        }
        return this.#edit((range) => unwrapInline(this.#ctx.content, range, tags));
    }

    setBlockType(type, attributes) {
        const name = blockType(this.#ctx, type);
        const entries = name && validAttributes(this.#ctx, name, attributes);
        if (!entries) {
            return false;
        }
        if (name === 'blockquote') {
            return this.#edit((range) => toggleWrapper(this.#ctx.content, range, name, entries));
        }
        if (LIST_TAGS.has(name)) {
            return this.#edit((range) => toggleList(this.#ctx.content, range, name));
        }
        return this.#edit((range) => setBlockType(this.#ctx.content, range, name, entries));
    }

    setBlockStyle(styles) {
        const entries = validStyles(this.#ctx, styles);
        if (!entries) {
            return false;
        }
        return this.#edit((range) => setBlockStyle(this.#ctx.content, range, entries));
    }

    clearFormatting() {
        return this.#edit((range) => {
            const { content } = this.#ctx;
            const block = range.collapsed ? blockOf(startNode(range), content) : null;
            if (block) {
                range.selectNodeContents(block);
            }
            clearInline(content, range);
            return resetBlocks(content, this.#ctx.ranges.current() ?? range);
        });
    }

    insertHtml(html) {
        const ctx = this.#ctx;
        const fragment = normalizeFragment(ctx.sanitizer.clean(html), ctx.config, ctx.sanitizer);
        return this.#insert(fragment, 'command', null);
    }

    insertText(text) {
        return this.#insert(textFragment(this.#ctx.content.ownerDocument, text), 'command', null);
    }

    undo() {
        return this.#travel(-1);
    }

    redo() {
        return this.#travel(1);
    }

    saveSelection() {
        return this.#ctx.ranges.save();
    }

    restoreSelection() {
        return this.#ctx.ranges.restore();
    }

    clearSavedSelection() {
        this.#ctx.ranges.clearSaved();
    }

    getSelectionStream() {
        const range = this.#ctx.ranges.latest();
        return jsonBytes(range ? { text: range.toString(), isCollapsed: range.collapsed } : null);
    }

    getLinkStream() {
        const range = this.#ctx.ranges.latest();
        const link = range && linkAt(this.#ctx.content, range);
        return jsonBytes(link ? linkInfo(link) : null);
    }

    selectLink() {
        const range = this.#ctx.ranges.latest();
        const link = range && linkAt(this.#ctx.content, range);
        if (!link) {
            return false;
        }
        this.#ctx.ranges.remember(nodeRange(this.#ctx.content, link));
        return true;
    }

    getImageStream() {
        const image = this.#currentImage();
        return jsonBytes(image ? imageInfo(image) : null);
    }

    selectImage() {
        const image = this.#currentImage();
        return image ? this.#ctx.images.select(image) : false;
    }

    focus() {
        if (!this.#disposed && !this.#ctx.sourceView.active) {
            this.#ctx.ranges.ensure();
        }
    }

    configure(partial) {
        if (this.#disposed) {
            return;
        }
        const ctx = this.#ctx;
        const policyChanged = partial?.policy !== undefined;
        Object.assign(ctx.options, partial ?? {});
        ctx.config = createConfig(ctx.options);
        if (policyChanged) {
            ctx.sanitizer = new Sanitizer(ctx.config.policy, ctx.content.ownerDocument);
        }
        ctx.history.limit = ctx.config.historyLimit;
        this.#applyConfig();
        ctx.state.schedule();
    }

    enterSourceView() {
        this.#ctx.sourceView.enter();
    }

    exitSourceView() {
        this.#ctx.sourceView.exit();
        refreshEmptyState(this.#ctx.content);
    }

    openPanel(panel) {
        if (!this.#disposed && panel) {
            this.#ctx.panel.open(panel);
        }
    }

    closePanel() {
        this.#ctx.panel.close();
    }

    dispose() {
        if (this.#disposed) {
            return;
        }
        this.#disposed = true;
        const ctx = this.#ctx;
        ctx.panel.dispose();
        this.#controller.abort();
        ctx.images.dispose();
        ctx.state.dispose();
        ctx.changes.dispose();
        ctx.sourceView.dispose();
        ctx.channel.dispose();
    }

    #applyConfig() {
        const { content, source, config, images } = this.#ctx;
        content.contentEditable = config.readOnly ? 'false' : 'true';
        content.setAttribute('aria-readonly', String(config.readOnly));
        source.readOnly = config.readOnly;
        if (config.placeholder) {
            content.dataset.placeholder = config.placeholder;
        } else {
            delete content.dataset.placeholder;
        }
        if (config.readOnly) {
            images.deselect();
        }
    }

    #canEdit() {
        return !this.#disposed && !this.#ctx.config.readOnly && !this.#ctx.sourceView.active;
    }

    #afterChange() {
        refreshEmptyState(this.#ctx.content);
        this.#ctx.state.schedule();
    }

    #edit(action, source = 'command') {
        if (!this.#canEdit()) {
            return false;
        }
        const ctx = this.#ctx;
        ctx.changes.flush();
        preserveSelection(ctx.content, () => normalizeRoot(ctx.content));
        ctx.changes.begin();
        const result = action(ctx.ranges.ensure());
        if (result) {
            ctx.changes.commit(source);
        }
        this.#afterChange();
        return Boolean(result);
    }

    #type(action) {
        const range = this.#canEdit() ? this.#ctx.ranges.current() : null;
        if (!range) {
            return false;
        }
        const result = action(range);
        if (result) {
            this.#ctx.changes.input();
            this.#afterChange();
        }
        return Boolean(result);
    }

    #insert(fragment, source, range) {
        return this.#edit((current) => {
            const target = range && this.#ctx.content.contains(range.startContainer) ? applyRange(this.#ctx.content, range) : current;
            return insertFragment(this.#ctx.content, target, fragment);
        }, source);
    }

    #travel(direction) {
        if (!this.#canEdit()) {
            return false;
        }
        const ctx = this.#ctx;
        ctx.changes.flush();
        const entry = direction < 0 ? ctx.history.undo() : ctx.history.redo();
        if (!entry) {
            return false;
        }
        ctx.images.deselect();
        ctx.content.replaceChildren(ctx.sanitizer.restore(entry.html));
        ctx.ranges.forget();
        const range = resolveBookmark(ctx.content, entry.bookmark);
        if (range) {
            ctx.ranges.select(range);
        }
        ctx.changes.commit('history', false);
        this.#afterChange();
        return true;
    }

    #currentImage() {
        const range = this.#ctx.ranges.latest();
        return this.#ctx.images.selected ?? (range ? imageAt(this.#ctx.content, range) : null);
    }
}
