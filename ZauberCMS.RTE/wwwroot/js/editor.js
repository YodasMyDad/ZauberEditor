import { resetBlocks, setBlockStyle, setBlockType, toggleWrapper, unwrapBlocks, wrapBlocks } from './blocks.js';
import { CaretMarks } from './caret-marks.js';
import { ChangeTracker } from './changes.js';
import { Channel } from './channel.js';
import { bindClipboard, normalizeFragment } from './clipboard.js';
import { createConfig, markFamily } from './config.js';
import { refreshEmptyState, replaceContent } from './content.js';
import { BLOCK_TAGS, LIST_TAGS, blockOf } from './dom.js';
import { bindContentEvents } from './events.js';
import { Finder } from './find.js';
import { Fullscreen } from './fullscreen.js';
import { History } from './history.js';
import { ImageTool, imageAt, imageInfo } from './images.js';
import { changeIndent } from './indent.js';
import { textFragment } from './insert.js';
import { bindKeyboard } from './keyboard.js';
import { bindLinks, linkAt, linkInfo } from './links.js';
import { clearInline, setInlineStyle, toggleMark, unwrapInline, wrapInline } from './marks.js';
import { toggleList } from './lists.js';
import { PanelHost } from './panel.js';
import { RangeKeeper } from './ranges.js';
import { bindEditorResize } from './resize.js';
import { Sanitizer } from './sanitize.js';
import { nodeRange, startNode } from './selection.js';
import { contentText, jsonBytes, serializeContent, toUtf8 } from './serialize.js';
import { SourceView } from './source-view.js';
import { StateTracker } from './state.js';
import { bindToolbar } from './toolbar.js';
import { Transactions } from './transactions.js';
import { blockType, inlineTag, validAttributes, validStyles, wrapperName, wrapperTag } from './validation.js';

export class Editor {
    #ctx;
    #tx;
    #controller = new AbortController();
    #disposed = false;

    constructor(root, content, source, toolbar, dotNetRef, options) {
        const channel = new Channel(dotNetRef);
        const config = createConfig(options);
        const ctx = {
            root, content, source, toolbar, channel, config,
            signal: this.#controller.signal,
            revision: 0,
            plainPaste: false,
            pasteTextOnce: false,
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
        ctx.fullscreen = new Fullscreen(ctx);
        const tx = new Transactions(ctx, () => this.#disposed);
        this.#tx = tx;
        ctx.finder = new Finder(ctx, (action) => tx.change(action));
        const actions = {
            edit: (action, changeSource) => tx.edit(action, changeSource),
            type: (action) => tx.type(action),
            travel: (direction) => tx.travel(direction),
            insert: (fragment, changeSource, range) => tx.insert(fragment, changeSource, range)
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
            this.#tx.afterChange();
        } else {
            ctx.changes.flush();
            ctx.changes.begin();
            replace();
            ctx.changes.commit('api');
            this.#tx.afterChange();
        }
        if (ctx.sourceView.active) {
            ctx.sourceView.refresh();
        }
    }

    getHtmlStream() {
        this.#ctx.sourceView.flush();
        return toUtf8(serializeContent(this.#ctx.content, this.#ctx.caretMarks.holder, this.#ctx.config.aliases));
    }

    getTextStream() {
        this.#ctx.sourceView.flush();
        return toUtf8(contentText(this.#ctx.content, this.#ctx.caretMarks.holder));
    }

    flush() {
        if (!this.#disposed) {
            this.#ctx.changes.flush();
            this.#ctx.sourceView.flush();
        }
    }

    getState() {
        return this.#ctx.state.snapshot();
    }

    toggleMark(tag) {
        const name = inlineTag(this.#ctx, tag);
        if (!name) {
            return false;
        }
        return this.#tx.edit((range) => toggleMark(this.#ctx.content, range, this.#ctx.config, name, this.#ctx.caretMarks));
    }

    wrap(tag, attributes) {
        const name = wrapperTag(this.#ctx, tag);
        const entries = name && validAttributes(this.#ctx, name, attributes);
        if (!entries) {
            return false;
        }
        if (BLOCK_TAGS.has(name)) {
            return this.#tx.edit((range) => wrapBlocks(this.#ctx.content, range, name, entries));
        }
        return this.#tx.edit((range) => wrapInline(this.#ctx.content, range, name, entries));
    }

    unwrap(tag) {
        const name = wrapperName(tag);
        const tags = markFamily(this.#ctx.config, name).tags;
        if (BLOCK_TAGS.has(name)) {
            return this.#tx.edit((range) => unwrapBlocks(this.#ctx.content, range, tags));
        }
        return this.#tx.edit((range) => unwrapInline(this.#ctx.content, range, tags));
    }

    setBlockType(type, attributes) {
        const name = blockType(this.#ctx, type);
        const entries = name && validAttributes(this.#ctx, name, attributes);
        if (!entries) {
            return false;
        }
        if (name === 'blockquote') {
            return this.#tx.edit((range) => toggleWrapper(this.#ctx.content, range, name, entries));
        }
        if (LIST_TAGS.has(name)) {
            return this.#tx.edit((range) => toggleList(this.#ctx.content, range, name));
        }
        return this.#tx.edit((range) => setBlockType(this.#ctx.content, range, name, entries));
    }

    setBlockStyle(styles) {
        const entries = validStyles(this.#ctx, styles);
        if (!entries) {
            return false;
        }
        return this.#tx.edit((range) => setBlockStyle(this.#ctx.content, range, entries));
    }

    indent() {
        return this.#tx.edit((range) => changeIndent(this.#ctx.content, range, 1));
    }

    outdent() {
        return this.#tx.edit((range) => changeIndent(this.#ctx.content, range, -1));
    }

    setTextStyle(property, value) {
        const entries = inlineTag(this.#ctx, 'span') && validStyles(this.#ctx, { [property]: value ?? null });
        if (!entries || entries.length !== 1) {
            return false;
        }
        const [[name, style]] = entries;
        const { content, caretMarks } = this.#ctx;
        return this.#tx.edit((range) => setInlineStyle(content, range, name, style, caretMarks));
    }

    clearFormatting() {
        return this.#tx.edit((range) => {
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
        return this.#tx.insert(fragment, 'command', null);
    }

    insertText(text) {
        return this.#tx.insert(textFragment(this.#ctx.content.ownerDocument, text), 'command', null);
    }

    undo() {
        return this.#tx.travel(-1);
    }

    redo() {
        return this.#tx.travel(1);
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

    setFullscreen(active) {
        if (!this.#disposed) {
            this.#ctx.fullscreen.set(active);
        }
    }

    setPlainTextPaste(active) {
        if (!this.#disposed) {
            this.#ctx.plainPaste = active === true;
            this.#ctx.state.schedule();
        }
    }

    find(text, matchCase) {
        return this.#disposed ? { count: 0, current: 0 } : this.#ctx.finder.find(text, matchCase === true);
    }

    replace(text, replacement, matchCase) {
        return this.#tx.canEdit
            ? this.#ctx.finder.replace(text, String(replacement ?? ''), matchCase === true)
            : this.find(text, matchCase);
    }

    replaceAll(text, replacement, matchCase) {
        return this.#tx.canEdit ? this.#ctx.finder.replaceAll(text, String(replacement ?? ''), matchCase === true) : 0;
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
        this.#ctx.finder.clear();
        this.#ctx.panel.close();
    }

    dispose() {
        if (this.#disposed) {
            return;
        }
        this.#disposed = true;
        const ctx = this.#ctx;
        ctx.panel.dispose();
        ctx.finder.clear();
        ctx.fullscreen.dispose();
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

    #currentImage() {
        const range = this.#ctx.ranges.latest();
        return this.#ctx.images.selected ?? (range ? imageAt(this.#ctx.content, range) : null);
    }
}
