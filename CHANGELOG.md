# Changelog

All notable changes to ZauberCMS.RTE are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [3.0.0] - 2026-11-25

A major release that rebuilds the editor's internals against the project standards. It contains breaking changes:
see [Migrating to 3.0](README.md#migrating-to-30) for every change, with before and after.

### Added

- Interactive WebAssembly and Auto render modes are supported alongside Interactive Server. The demo app has pages
  for each mode.
- `ZauberRichTextEditor.Api` exposes `IEditorApi` for programmatic control. Calls made before the editor is ready wait
  for it.
- New component parameters: `Toolbar`, `ThemeChanged` (for `@bind-Theme`), `Placeholder`, `AriaLabel`,
  `OnStateChanged` and `AdditionalAttributes`. `ValueExpression` makes the editor work with `EditForm` validation.
- Registration through options: `AddToolbarItem<T>()`, `AddToolbarItem(instance)`, `RemoveToolbarItem(id)`, and the
  opt-in `AddToolbarItemsFromAssembly(assembly)`.
- `ToolbarItemIds` constants for every built-in item.
- `IToolbarItem.IconSvg` for inline SVG icons. All built-in icons are now inline SVG.
- `IEditorApi.SetThemeAsync`, `GetHtmlAsync` and `GetTextAsync`, and `CancellationToken` parameters on every async
  member.
- `EditorState` gains `InLink`, `InImage`, `InTable`, `IsReadOnly`, `Theme`, `PrefersDarkScheme` and `IsDark`.
- `ChangeSource.SourceView` and `ChangeSource.ImageResize`.
- `EditorSettings.HistoryLimit`, `ChangeDelay` and `MaxContentBytes`, and `ImageConstraints.MaxFileSizeBytes`.
- `PanelBase.Title`, used as the panel heading and the dialog's accessible name, and the `PanelBase.ElementId` helper.
- Toast notifications (`ShowToastAsync`), announced through a live region.
- A design-token system of CSS custom properties, covering colour, shadow, shape, spacing, sizes, typography and
  motion.
- Pasted or dropped image files are embedded when `ImageConstraints.AllowBase64ImageUpload` and
  `HtmlPolicy.AllowDataUrls` allow it, within `AllowedImageTypes` and `MaxFileSizeBytes` (rejected files raise a warning
  toast), Word lists paste as real lists, and Ctrl/Cmd+click opens links.
- Accessibility: a roving-tabindex toolbar with arrow keys, `aria-pressed` toggles, ARIA menus for dropdowns, modal
  dialogs with a focus trap and Escape for panels, `aria-readonly`, `:focus-visible` rings, larger touch targets, and
  `prefers-reduced-motion`.
- `ZauberCMS.RTE.Tests` (xUnit v3, bUnit, Shouldly, NSubstitute), a `CHANGELOG.md` and a `LICENSE` file.
- `EditorContentTooLargeException`, thrown by `GetHtmlAsync` and the other reads when content is larger than
  `MaxContentBytes`.
- Symbols package (`.snupkg`), SourceLink, and the library is marked trimmable and AOT-compatible.

### Changed

- The JavaScript is now ES modules, imported by the component. Each editor gets its own instance through
  `createEditor`, with no `window` globals. Consumers add no `<script>` tags.
- Custom toolbar items must be registered explicitly. Assembly scanning is opt-in. Items registered by type are
  created per editor instance through DI.
- `IToolbarItem`: `ItemType` is now `Kind`, `ChildItems` is now `ChildItemIds` (resolved through the registry, so
  overrides apply to dropdown children), `TrackedTags` is now `IReadOnlyList<string>`, and `IconCss` is optional and
  rendered without an implicit `fas` prefix. `ToolbarItemBase` defaults now disable items in read-only and source
  view, and derive `IsActive` from `PrimaryTag`.
- `IEditorApi` was reshaped: properties for `State` and `Settings`, async-only reads, the selection methods renamed,
  and a generic `OpenPanelAsync<TPanel>`. Calls after the editor is disposed throw `ObjectDisposedException`, and
  `SetHtmlAsync` throws while the editor is read-only.
- `IToolbarItem.ExecuteAsync` takes a `CancellationToken`, cancelled when the editor is disposed.
- `ToolbarLayout`, `ToolbarGroup` and `ToolbarItemRef` reject null entries and ids, and `ToolbarLayout.FromJson`
  reports them as a `JsonException`.
- State, info, event-argument and settings types are now immutable `sealed record`s. `HtmlPolicy.AllowedAttributes` is
  a flat set, and `EditorDimensions` uses CSS lengths throughout.
- Toolbar layouts are records: `ToolbarGroup` (was `ToolbarBlock`), `ToolbarItemRef` (was `ToolbarItemReference`)
  and `ToolbarLayoutEntry` (was `ToolbarLayoutItem`). Layout JSON is camelCase, written with a source-generated,
  trim-safe serializer.
- `HtmlBuilder` encodes attribute values and validates names.
- `Theme` is reactive, and `Theme.Auto` follows the operating system.
- `ValueChanged` is debounced (`ChangeDelay`) and flushed on blur and before commands. Toolbar items run only after
  the latest content has reached the bound value.
- While `ReadOnly` is set, content changes and shortcuts reported by the browser are ignored, and shortcuts only run
  items that are in the editor's layout.
- The stylesheet was rewritten: everything is in `@layer zauber-rte` with `:where()` selectors, native nesting and
  logical properties, with no `!important`. Themes use `color-scheme` and `light-dark()` tokens instead of duplicated
  dark selectors.
- Browser baseline: Chrome/Edge 123+, Firefox 120+, Safari 17.5+.
- DOMPurify updated to 3.5.0 and vendored as an ES module.
- Built-in components that are implementation details moved to `ZauberCMS.RTE.Components.Internal`.
- The demo app was rebuilt with pages for the full toolbar, custom items and a panel, multiple editors, read-only
  mode, themes, large content, and the WebAssembly and Auto render modes.

### Removed

- The `OnKeyDown`, `OnPaste` and `OnSelectionChanged` events (use `OnChange` with `ChangeSource` and
  `OnStateChanged`), and the public `SetContentAsync` and `[JSInvokable]` methods on the component.
- `IToolbarItem.Placement`, `ToolbarPlacement`, `IsDropdownLabel`, `PanelComponent` and
  `ToolbarItemType.Separator`.
- The synchronous `IEditorApi` members (`GetSelection`, `GetHtml`, `GetText`), plus `ReplaceSelectionAsync`,
  `BlurAsync`, `CleanHtmlAsync`, `SetTextAsync`, `GetCurrentBlockTypeAsync`, `GetCurrentHeadingLevelAsync` and
  `PanelParameters`.
- `EditorSettings.DefaultTheme`, `CmsDefault()`, `Minimal()` and `BlockTags`. `ToolbarLayout.Default`, `Cms`,
  `FromItems` and `GetEffectiveLayout`. `ToolbarLayoutExtensions`. `HtmlBuilder.Html`, `Id`, `Style` and its implicit
  string conversion.
- Never-populated state and argument members, such as `EditorState.CanFormat`, `IsDirty`, `WordCount` and
  `CharacterCount`.
- The public built-in item classes, `DefaultToolbarItems`, discovery and factory services, and JS runtime wrappers.
- The `Examples` namespace from the package (examples now live in the demo app).
- The bundled Font Awesome stylesheet and its CDN font dependency.
- `AddZauberRte(params Assembly[])` and `ZauberRteOptions.AllowOverrides`/`Assemblies`.

### Fixed

- **Multiple editors on one page:** paste went to the last editor initialised, because the paste callback was a
  page-wide global. Each editor now handles its own paste, selection, history and state.
- **Value race and data loss:** the remaining race after #1, where a stale value echoed back from the parent could
  overwrite newer typing, is gone. The editor recognises the echo of a value it is still reporting, and content
  notifications carry a revision, so a notification or streamed read that started before the parent set a new `Value`
  can't overwrite it. A one-way `Value` that returns to an earlier value is applied. Interop failures no longer publish
  `null`. Opening the source view no longer empties the value, source edits update the value live, and leaving the
  source view keeps editing working.
- **Keyboard:** Ctrl+Z/Ctrl+Y no longer run twice, Ctrl+Shift+Z redoes, Cmd shortcuts work on macOS, Ctrl+Shift+7/8
  work on US and UK layouts, and only items in the layout respond. Toolbar buttons work from the keyboard, and Tab no
  longer traps focus.
- **Editing:** Backspace no longer deletes image-only blocks elsewhere in the document. Enter with a selection deletes
  it first. Enter works in code blocks, table cells and after images. List conversions no longer lose text or create
  invalid markup. Headings no longer get a stray `level` attribute. Bold on part of a bold run, alias tags (`<b>`, `<i>`)
  and clear formatting behave correctly. Inserting a block no longer deletes the text after the caret.
- **Undo:** the caret is restored after undo and redo, typing is grouped into sensible steps, and the history limit is
  configurable.
- **Paste:** ordinary prose is no longer turned into lists, Word list markers are removed, empty table cells survive,
  plain text is pasted as text, and paste into the source view works.
- **Links and images:** editing a link no longer nests anchors. Image editing works. Resize handles no longer leak
  listeners or drift on scroll. `OnImageResized` is raised, and clicking an image no longer writes an `id` into the
  content.
- **Settings that did nothing now work:** `ReadOnly`, `Theme.Auto`, `EditorDimensions.MinHeight`/`MaxHeight`,
  `ImageConstraints.MaxWidth`/`MaxHeight`/`MaintainAspectRatio`, `HtmlPolicy.AllowDataUrls`/`AllowExternalImages`,
  and `ShowToastAsync`. The editor reports text alignment.
- **Toolbar UI:** dropdowns close on any outside click and no longer clear their own state when opened. `aria-expanded`
  renders correctly. Items are disabled in source view. The `table` item is registered and appears in
  `ToolbarLayout.Full`.
- **Lifetimes:** every `DotNetObjectReference`, JS object reference, event listener and observer is released on
  dispose. There's no `Task.Delay` polling, and initialisation failures are logged and shown.
- `AddZauberRte` is idempotent.

### Security

- **Stored XSS:** the initial `Value`, `SetHtmlAsync`, `InsertHtmlAsync` and HTML source edits were inserted
  unsanitised. Every entry point is now sanitised with DOMPurify under the editor's `HtmlPolicy`.
- **Attribute injection:** `HtmlBuilder` didn't encode attribute values, so input such as `x" onerror="...` in the
  link or image panels became live markup. Values are now encoded.
- **Mutation XSS in paste cleaning:** regexes ran over already-sanitised HTML and could re-open attributes. Cleaning
  now works on a DOM, and sanitising runs last.
- **Script execution through inserts:** `createContextualFragment` and live-document parsing could execute script.
  `innerHTML` is now only assigned DOMPurify output.
- **URL schemes:** `href`/`src` are restricted to `http`, `https`, `mailto`, `tel` and relative URLs. Ctrl/Cmd+click
  checks the scheme and opens with `noopener`, and `target="_blank"` links get `rel="noopener noreferrer"`.
- **No `eval`:** removed, along with the CDN dependency, so every asset ships inside the package.

### Performance

- About 10 SignalR round trips and 4 toolbar renders per keystroke are replaced by one debounced content
  notification and one state notification per animation frame, sent only when the state actually changed. Keyboard
  shortcuts are matched in JavaScript.
- Content larger than 12 KB, and every value the API reads back (including image `data:` sources, selected text and
  link text), is streamed through `IJSStreamReference`, so large documents and base64 images work within Blazor
  Server's default 32 KB SignalR message limit.
- Toolbar buttons re-render only when their own state changes, and the item registry is frozen once at startup.
- Undo history stores coalesced snapshots with selection bookmarks instead of one full snapshot per keystroke.

## [2.1.0] and earlier

Changes before 3.0.0 aren't itemised here. See the
[commit history](https://github.com/YodasMyDad/ZauberEditor/commits/master).

[3.0.0]: https://github.com/YodasMyDad/ZauberEditor/releases/tag/v3.0.0
[2.1.0]: https://github.com/YodasMyDad/ZauberEditor/commits/master
