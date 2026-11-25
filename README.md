# ZauberCMS.RTE: rich-text editor for Blazor

[![NuGet](https://img.shields.io/nuget/v/ZauberCMS.RTE.svg)](https://www.nuget.org/packages/ZauberCMS.RTE/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/YodasMyDad/ZauberEditor/blob/master/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0%20%7C%2011.0-blue.svg)](https://dotnet.microsoft.com/)

An extensible WYSIWYG HTML editor component for Blazor. It is built for
[ZauberCMS](https://github.com/YodasMyDad/ZauberCMS) and released as a standalone package.

Upgrading from 3.x? Read [Migrating to 4.0](#migrating-to-40). Upgrading from 2.x? Read [Migrating to 3.0](#migrating-to-30)
first. 3.0 is a major release with breaking changes.

## Contents

- [Features](#features)
- [Requirements](#requirements)
- [Installation and setup](#installation-and-setup)
- [Basic usage](#basic-usage)
- [Parameters and events](#parameters-and-events)
- [Programmatic control with `IEditorApi`](#programmatic-control-with-ieditorapi)
- [Settings](#settings)
- [Toolbar layouts](#toolbar-layouts)
- [Built-in toolbar items](#built-in-toolbar-items)
- [Keyboard](#keyboard)
- [Theming and styling](#theming-and-styling)
- [Accessibility](#accessibility)
- [Security](#security)
- [Large documents](#large-documents)
- [Extending the editor](#extending-the-editor)
- [Migrating to 4.0](#migrating-to-40)
- [Migrating to 3.0](#migrating-to-30)
- [License](#license)

## Features

- **Blazor-native component.** Use `@bind-Value` and `EditForm` validation. It works in Interactive Server,
  WebAssembly and Auto render modes.
- **Zero setup in the page.** No `<script>` or `<link>` tags: the stylesheet and the ES modules load themselves, and
  every editor on a page gets its own isolated instance.
- **Isolated styles.** The editor's CSS matches nothing outside the editor, and it keeps its look under host resets
  such as Bootstrap Reboot and Tailwind Preflight.
- **Sanitised by default.** Every way HTML gets into the editor is cleaned with DOMPurify according to your
  `HtmlPolicy`, and URL schemes are allow-listed.
- **Extensible toolbar.** Register your own buttons, toggles, dropdowns and slide-out panels, override or remove
  built-in items by id, and arrange the toolbar in code or JSON. `IToolbarItemCatalog` lists the registered items for
  a "pick your toolbar" screen, and `Shortcuts` adds key combinations such as Ctrl+S without a button.
- **Formatting.** Bold, italic, underline, strikethrough, sub/superscript, inline code and code blocks, text and
  highlight colours, headings, quotes, lists, indentation, alignment, links, images (URL or upload, with resize
  handles), tables, horizontal lines, special characters, clear formatting, undo/redo, find and replace, plain-text
  paste, full screen, an HTML source view, and live word and character counts.
- **Office-friendly paste.** Word lists come through as real lists, empty table cells are kept, and pasted image files
  are embedded when the image settings and policy allow it.
- **Themes.** Light, dark and auto (follows the OS), driven by CSS custom properties that you can override without
  `!important`.
- **Accessible.** The toolbar uses roving tabindex, toggles expose `aria-pressed`, menus and panels use ARIA dialog and
  menu patterns, and focus is never trapped.
- **Large documents.** Content bigger than a SignalR message is sent in pieces, so you don't need to raise any limits.
- **Trimming and AOT.** The library is marked `IsTrimmable` and `IsAotCompatible`, with no reflection on the default
  path.

## Requirements

- **.NET 10** (`net10.0`, LTS) or **.NET 11** (`net11.0`). The package ships builds for both.
- An **interactive render mode** for the component: Interactive Server, Interactive WebAssembly or Interactive Auto.
  Prerendering is supported: the frame renders on the server and editing starts on the first interactive render.
- A **browser that supports CSS `light-dark()`**: Chrome/Edge 123+, Firefox 120+ or Safari 17.5+.

## Installation and setup

```bash
dotnet add package ZauberCMS.RTE
```

### 1. Register the services

In `Program.cs`:

```csharp
using ZauberCMS.RTE.Services;

builder.Services.AddZauberRte();
```

`AddZauberRte` is safe to call more than once, and each call's configuration callback is applied in order. Custom
toolbar items are registered in that callback (see [Extending the editor](#extending-the-editor)).

For **WebAssembly or Auto** render modes, also call `AddZauberRte(...)` in the client project's `Program.cs`, with
the same item registrations. Keep the server registration as well, because prerendering runs on the server.

### 2. Assets load themselves

There's nothing to add to `App.razor`. The first editor on a page adds the stylesheet
(`_content/ZauberCMS.RTE/css/zauber-editor.css`) to `<head>` once, ahead of your own stylesheets so your rules win
ties, and the component imports its JavaScript modules (`zauber-rte.js` and its dependencies, including DOMPurify).
With `MapStaticAssets` the fingerprinted URL is used, and a path base works. All assets ship inside the package;
nothing loads from a CDN.

The editor stays hidden (`visibility: hidden`, so it keeps its space) until the stylesheet applies, so it never shows
unstyled, prerendered HTML included. A prerendered editor links the stylesheet itself, so it is styled even if
interactivity never starts. If the stylesheet fails to load, the editor is shown after at most 3 seconds and the
failure is logged.

A `<link href="_content/ZauberCMS.RTE/css/zauber-editor.css" rel="stylesheet" />` kept from 3.x is harmless: the
editor finds it and doesn't add a second. To bundle or link the CSS yourself, turn auto-loading off:

```csharp
builder.Services.AddZauberRte(options => options.AutoLoadStyles = false);
```

With a Content-Security-Policy, `style-src` must allow the site's own origin (`'self'`). The hide-until-styled rule
is an inline `style` attribute; a policy that blocks inline styles just skips it.

### 3. Import the namespaces

In `_Imports.razor`:

```razor
@using ZauberCMS.RTE.Components
@using ZauberCMS.RTE.Models
```

## Basic usage

```razor
@page "/editor"
@rendermode InteractiveServer

<ZauberRichTextEditor @bind-Value="_html" Placeholder="Start writing…" />

@code {
    private string? _html = "<p>Hello <strong>world</strong></p>";
}
```

The bound value is the editor's HTML. Typing updates it once the user pauses (after `EditorSettings.ChangeDelay`,
250 ms by default). It also updates immediately when the editor loses focus, before any toolbar command runs, and
after paste, undo/redo or source-view edits. Setting the value from your code replaces the content and resets the
undo history, and it doesn't raise `ValueChanged`.

### Saving

Clicking a Save button moves focus out of the editor, and the latest content reaches the bound value before the
click handler runs, at any document size. When a save can start while the editor still has focus (a keyboard
shortcut, a timer, a `ValueChanged` handler that awaits), or your `ValueChanged` handler is asynchronous, flush first:

```razor
<ZauberRichTextEditor @ref="_editor" @bind-Value="_html" />

@code {
    private ZauberRichTextEditor _editor = default!;
    private string? _html;

    private async Task SaveAsync()
    {
        await _editor.Api.FlushAsync();
        await Articles.SaveAsync(_html);
    }
}
```

`FlushAsync` delivers any pending edits through `ValueChanged`, waits until its handler has finished, and does nothing
when nothing is pending. Disposing the editor also delivers pending edits first, while the connection is alive.

### Output HTML

The bound value is clean, semantic HTML:

- **An empty editor is `""`.** When the user clears everything, `ValueChanged` receives an empty string (never
  `<p></p>` or `<p><br></p>`), so `[Required]` validation works. Otherwise every block is kept as the user made it,
  including empty paragraphs before or after the text (`<p></p><p>2</p>`); only a document made entirely of empty
  blocks is `""`. A block holding only spaces or `&nbsp;` (as Word pastes it) is written as `<p></p>`.
- **Normal spaces.** Browsers insert `&nbsp;` for spaces typed at the edge of a text node. In the output, a single
  `&nbsp;` at the start or end of a text node with non-space text on both sides (across an inline tag such as
  `<strong>`) becomes a normal space, because it renders the same. A no-break space in the middle of a text node
  (`a&nbsp;b`, as the special characters panel inserts it) is kept, and so are leading and trailing spaces in a block
  and runs of spaces.
- **Canonical tags.** Tags the browser creates itself, such as `<b>` or `<i>`, are written as the toolbar item's tag
  (`<strong>`, `<em>`), and a code block has no trailing newline that doesn't show.
- **No placeholder `<br>`s.** The editor keeps a `<br>` inside each empty block so the caret can be placed there, but
  it never reaches the output: an empty paragraph between content is `<p></p>`, and a `<br>` that ends a block or a
  line before a nested block (`<p>Text<br></p>`) is dropped, because browsers don't render it. HTML from 2.x with such
  `<br>`s is cleaned on the next change, with no visual difference.
- **Line breaks are kept.** Shift+Enter in the middle of text gives `<p>a<br>b</p>`. A break at the end of a block
  needs two `<br>`s to show the new empty line, so `<p>a<br><br></p>` is kept exactly; only a single trailing `<br>`
  (which shows nothing) is dropped. Setting the output back as `Value` renders identically.

The same rules apply to `GetHtmlAsync`, `OnChange` and the HTML source view.

### In an `EditForm`

`@bind-Value` also passes `ValueExpression`, so the editor notifies the cascading `EditContext` and validation
messages work:

```razor
@using System.ComponentModel.DataAnnotations

<EditForm Model="_article" OnValidSubmit="Save">
    <DataAnnotationsValidator />

    <ZauberRichTextEditor @bind-Value="_article.Body" AriaLabel="Article body" />
    <ValidationMessage For="() => _article.Body" />

    <button type="submit">Save</button>
</EditForm>

@code {
    private readonly Article _article = new();

    private void Save()
    {
    }

    public sealed class Article
    {
        [Required]
        public string? Body { get; set; }
    }
}
```

`[Required]` accepts markup with no text, such as `<p></p>`. To require visible text, write a validation attribute
that strips the tags first. The demo app's `ZauberRTE.Web/Models/RequiredContentAttribute.cs` is an example.

## Parameters and events

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Value` | `string?` | `null` | The HTML content. Use with `@bind-Value`. |
| `ValueChanged` | `EventCallback<string?>` | | Raised when the content changes. |
| `ValueExpression` | `Expression<Func<string?>>?` | | Set by `@bind-Value`; identifies the field for `EditContext` validation. |
| `Settings` | `EditorSettings` | `new()` | Editor configuration. See [Settings](#settings). |
| `Toolbar` | `ToolbarLayout?` | `null` | The toolbar layout. Overrides `Settings.ToolbarLayout` when set. |
| `Theme` | `Theme` | `Theme.Auto` | `Light`, `Dark` or `Auto`. Use `@bind-Theme` to follow the built-in theme toggle. |
| `ThemeChanged` | `EventCallback<Theme>` | | Raised when the theme changes from inside the editor (theme toggle or `IEditorApi.SetThemeAsync`). |
| `ReadOnly` | `bool` | `false` | Blocks editing, paste and drop, and disables the toolbar (except the theme toggle and full screen). Content changes and shortcuts reported by the browser are ignored while it is set. Can change at runtime. |
| `Shortcuts` | `IReadOnlyList<EditorShortcut>?` | `null` | Keyboard shortcuts handled without a toolbar button. See [Keyboard shortcuts](#keyboard-shortcuts). |
| `Placeholder` | `string?` | `null` | Text shown while the editor is empty. |
| `AriaLabel` | `string` | `"Rich text editor"` | Accessible name of the editing area. The toolbar is labelled "{AriaLabel} toolbar". |
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>?` | | Any other attributes (such as `class`, `style`, `data-*`) are applied to the root element. `class` and `style` are merged with the editor's own. |

| Event | Argument | Raised when |
| --- | --- | --- |
| `OnChange` | `EditorChangeArgs(string Html, ChangeSource Source)` | The content changed. `Source` is `Input`, `Paste`, `Command`, `History`, `Api`, `SourceView` or `ImageResize`. |
| `OnStateChanged` | `EditorState` | The selection, active formatting, focus, undo/redo availability or mode changed. |
| `OnCommandExecuted` | `CommandExecutedArgs(string ItemId)` | A toolbar item or keyboard shortcut ran successfully. For an `EditorShortcut`, `ItemId` is its `Keys`. |
| `OnCommandFailed` | `CommandFailedArgs(string ItemId, Exception Exception)` | A toolbar item or keyboard shortcut threw. The failure is also logged and shown as an error toast, and the editor keeps working. For an `EditorShortcut`, `ItemId` is its `Keys`. |
| `OnImageResized` | `ImageResizedArgs(int Width, int Height)` | The user finished resizing an image. |
| `OnFocus` | | The editing area gained focus. |
| `OnBlur` | | The editing area lost focus (after any pending change has been delivered). |

```razor
<ZauberRichTextEditor @bind-Value="_html" OnChange="LogChange" OnStateChanged="state => _state = state" />

@code {
    private string? _html;
    private EditorState _state = EditorState.Empty;
    private int _pastes;

    private void LogChange(EditorChangeArgs change)
    {
        if (change.Source is ChangeSource.Paste)
        {
            _pastes++;
        }
    }
}
```

`EditorState` is an immutable snapshot with these members: `ActiveMarks` (primary tags such as `strong` and `em`,
case-insensitive), `BlockType` (lower-case tag: `p`, `h1`–`h6`, `blockquote`, `pre`, `ul`, `ol`), `HeadingLevel`
(0 when the block isn't a heading), `Alignment`, `HasSelection`, `HasFocus`, `CanUndo`, `CanRedo`, `InLink`,
`InImage`, `InTable`, `CanOutdent` (the block is indented or in a list), `IsSourceView`, `IsFullscreen`,
`IsPlainTextPaste`, `IsReadOnly`, `WordCount`, `CharacterCount`, `Theme`, `PrefersDarkScheme` and `IsDark` (resolves
`Theme.Auto`).

`WordCount` and `CharacterCount` are computed in the browser from the content's text whenever a content change is
delivered (after the `ChangeDelay` pause, not on every keystroke), and also when `Value` is set. A word is a run of
non-space characters containing a letter or digit; characters include spaces but not line breaks.

```razor
<ZauberRichTextEditor @bind-Value="_html" OnStateChanged="state => _state = state" />
<p>@_state.WordCount words · @_state.CharacterCount characters</p>
```

## Programmatic control with `IEditorApi`

Each editor exposes an `IEditorApi` through its `Api` property. Toolbar items receive the same API in `ExecuteAsync`.
Calls made before the editor has finished loading wait until it's ready, and calls made after it has been disposed
throw `ObjectDisposedException`. Every async member accepts an optional `CancellationToken`, and the members can be
called from any thread.

```razor
<ZauberRichTextEditor @ref="_editor" @bind-Value="_html" />
<button type="button" @onclick="AddSignatureAsync">Add signature</button>

@code {
    private ZauberRichTextEditor? _editor;
    private string? _html;

    private async Task AddSignatureAsync()
    {
        if (_editor is null)
        {
            return;
        }

        var signature = HtmlBuilder.Element("p").Child(HtmlBuilder.Element("em").Text("Kind regards"));
        await _editor.Api.InsertHtmlAsync(signature.Build());
    }
}
```

When focus has moved to a button outside the editor, commands apply at the last caret position inside the editor.

| Member | Description |
| --- | --- |
| `State` | The latest `EditorState`. |
| `Settings` | The editor's `EditorSettings`. |
| `ToggleMarkAsync(tag)` | Toggles an inline mark such as `strong`, `em` or `mark` on the selection. |
| `WrapSelectionAsync(tag, attributes?)` | Wraps the selection in an element, for example `span` with a `class`. |
| `UnwrapSelectionAsync(tag)` | Removes that element from around the selection. |
| `SetBlockTypeAsync(blockType, attributes?)` | Changes the current block(s) to `p`, `h1`–`h6`, `pre`, `blockquote`, `div`, `ul` or `ol`. |
| `SetBlockStyleAsync(styles)` | Sets inline styles on the current block(s), such as `text-align`. |
| `IndentAsync()` / `OutdentAsync()` | Indents or outdents the current block(s) with `margin-inline-start` in 2em steps, or nests list items under the item before them and lifts them back out. |
| `SetTextStyleAsync(styleName, value)` | Sets an inline style such as `color` or `background-color` on the selected text as a `<span style>`; `null` removes it. With a caret, it applies to the word around it or to what you type next. |
| `ClearFormattingAsync()` | Removes inline formatting from the selection. |
| `InsertHtmlAsync(html)` | Inserts HTML at the selection after sanitising it with the editor's policy. |
| `InsertTextAsync(text)` | Inserts plain text at the selection. |
| `FlushAsync()` | Delivers pending edits through `ValueChanged` and waits for its handler to finish; a no-op when nothing is pending. Await it before saving the bound value (see [Saving](#saving)). |
| `GetHtmlAsync()` / `GetTextAsync()` | Reads the current HTML or plain text (blocks separated by new lines). Throws `EditorContentTooLargeException` when it is larger than `EditorSettings.MaxContentBytes`, so a save never receives an empty string by mistake. |
| `SetHtmlAsync(html)` | Replaces the content with sanitised HTML as an undoable change. Raises `ValueChanged` and `OnChange` with `ChangeSource.Api`. Throws `InvalidOperationException` while the editor is read-only; set `Value` instead. |
| `GetSelectionAsync()` | Returns `SelectionInfo(string Text, bool IsCollapsed)`, or `null` when the selection isn't in the editor. |
| `SaveSelectionAsync()` / `RestoreSelectionAsync()` / `ClearSavedSelectionAsync()` | Remembers and restores the selection, typically around opening a panel. |
| `GetLinkAtCursorAsync()` / `SelectLinkAtCursorAsync()` | Returns `LinkInfo(Href, Text, Title, Target)`, with `Rel`, for the link at the cursor, or selects the whole link. |
| `GetImageAtCursorAsync()` / `SelectImageAtCursorAsync()` | Returns `ImageInfo(Src, Alt, Title, Width, Height)` for the selected image, or selects it. |
| `UndoAsync()` / `RedoAsync()` | Undo and redo. |
| `FocusAsync()` | Moves focus into the editor. |
| `ToggleSourceViewAsync()` | Switches between the rich view and the HTML source view. |
| `SetThemeAsync(theme)` | Changes the theme and raises `ThemeChanged`. |
| `SetFullscreenAsync(fullscreen)` | Makes the editor fill the browser window (Escape returns it), locking the page scroll. |
| `SetPlainTextPasteAsync(plainText)` | While on, paste inserts text only, with line breaks as paragraphs and `<br>`, like `InsertTextAsync`. |
| `FindAsync(text, matchCase?)` | Selects and highlights the next match after the selection, across inline formatting, wrapping to the start. Returns `FindResult(Count, Current)`. |
| `ReplaceAsync(text, replacement, matchCase?)` | Replaces the selected match as one undoable step, then finds the next. |
| `ReplaceAllAsync(text, replacement, matchCase?)` | Replaces every match as a single undoable step and returns how many were replaced. |
| `OpenPanelAsync<TPanel>(parameters?)` | Opens a `PanelBase` component in the slide-out panel. |
| `OpenPanelAsync(panelType, parameters?)` | Opens a panel whose type is chosen at runtime. Throws `ArgumentException` unless the type is a concrete `PanelBase`. |
| `ClosePanelAsync()` | Closes the open panel and returns focus to the editor. |
| `ShowToastAsync(message, type)` | Shows a short message inside the editor (`Info`, `Success`, `Warning` or `Error`). |

Tags and attributes you pass to these methods must be allowed by the editor's `HtmlPolicy`. Disallowed tags throw a
`JSException`. Disallowed attributes are dropped, and the editor logs a warning.

## Settings

`EditorSettings` and the types it contains are immutable records. Create them with object initialisers, and use
`with` to derive variations. Pass a new instance to change the settings of a live editor. Keep settings in a field
(ideally `static readonly`) rather than building them inline on every render.

```csharp
using System.Collections.Frozen;
using ZauberCMS.RTE.Models;

public static class EditorPresets
{
    public static EditorSettings Article { get; } = new()
    {
        ToolbarLayout = ToolbarLayout.Full,
        Dimensions = new() { Height = "420px", MaxHeight = "80vh" },
        ImageConstraints = new() { AllowBase64ImageUpload = true, MaxWidth = 1200 },
        HtmlPolicy = new()
        {
            AllowedTags = FrozenSet.Create(StringComparer.OrdinalIgnoreCase, [.. new HtmlPolicy().AllowedTags, "mark"]),
            AllowExternalImages = false
        },
        HistoryLimit = 200,
        ChangeDelay = TimeSpan.FromMilliseconds(500)
    };

    public static EditorSettings Comment { get; } = Article with
    {
        ToolbarLayout = ToolbarLayout.Simple,
        Dimensions = new() { Height = "160px" }
    };
}
```

```razor
<ZauberRichTextEditor @bind-Value="_html" Settings="EditorPresets.Article" />
```

### `EditorSettings`

| Property | Default | Description |
| --- | --- | --- |
| `ToolbarLayout` | `ToolbarLayout.Full` | Layout used when the component's `Toolbar` parameter isn't set. |
| `Dimensions` | `new()` | Editor size; see below. |
| `ImageConstraints` | `new()` | Image upload and resize rules; see below. |
| `HtmlPolicy` | `new()` | The HTML kept in the content; see below. |
| `HistoryLimit` | `100` | Maximum number of undo steps. Typing is grouped into one step per pause. |
| `ChangeDelay` | `250 ms` | How long typing must pause before the bound value updates. |
| `MaxContentBytes` | `10 MB` | Largest content, in UTF-8 bytes, that the editor transfers to .NET. Changes above it aren't delivered, and a warning is logged. |

### `EditorDimensions`

CSS lengths such as `"300px"`, `"20rem"` or `"50vh"`. A `null` value isn't emitted, so the stylesheet's defaults or
your own CSS apply. The editing area can also be resized by dragging the handle in the bottom corner.

| Property | Default | Description |
| --- | --- | --- |
| `Width` | `null` | Editor width. `null` fills the container. |
| `Height` | `"300px"` | Height of the editing area. `null` grows with the content. |
| `MinHeight` | `"100px"` | Minimum height of the editing area. |
| `MaxHeight` | `null` | Maximum height of the editing area. `null` is unlimited. |

### `ImageConstraints`

| Property | Default | Description |
| --- | --- | --- |
| `MaxWidth` / `MaxHeight` | `null` | Largest size, in pixels, an image can be resized to. `null` is unlimited. |
| `MaintainAspectRatio` | `true` | Whether resizing keeps the image's aspect ratio. |
| `AllowBase64ImageUpload` | `false` | Shows an upload option in the image panel and lets pasted or dropped image files be embedded; files become `data:` URLs. Also requires `HtmlPolicy.AllowDataUrls`. |
| `AllowedImageTypes` | `.jpg .jpeg .png .gif .webp` | File extensions accepted for uploaded, pasted or dropped image files (case-insensitive). |
| `MaxFileSizeBytes` | `5 MB` | Largest image file accepted by upload, paste or drop. |

A pasted or dropped image file that these rules reject isn't inserted; the editor shows a warning toast and logs it.

### `HtmlPolicy`

| Property | Default | Description |
| --- | --- | --- |
| `AllowedTags` | `p br div span strong em b i u s strike sub sup code pre h1 h2 h3 h4 h5 h6 blockquote ul ol li table caption colgroup col thead tbody tfoot tr th td a img figure figcaption hr` | Allowed element names (case-insensitive). |
| `AllowedAttributes` | `class id title lang dir role style data-* aria-* href target rel src alt width height colspan rowspan scope` | Allowed attribute names on any allowed element. `data-*` and `aria-*` allow those families. |
| `TagAttributes` | empty | Attributes allowed only on one tag, in addition to `AllowedAttributes`. Keys and names are case-insensitive. |
| `AllowDataUrls` | `true` | Whether images may use `data:image/...` URLs. Uploaded, pasted or dropped image files also need `ImageConstraints.AllowBase64ImageUpload`. |
| `AllowExternalImages` | `true` | Whether images may load from other origins. When `false`, only same-origin and relative image URLs are kept. |

For example, to keep `target` on links but strip it from every other element:

```csharp
using System.Collections.Frozen;

var policy = new HtmlPolicy
{
    AllowedAttributes = FrozenSet.Create(
        StringComparer.OrdinalIgnoreCase,
        [.. new HtmlPolicy().AllowedAttributes.Where(name => name is not "target")]),
    TagAttributes = new Dictionary<string, IReadOnlySet<string>>
    {
        ["a"] = FrozenSet.Create(StringComparer.OrdinalIgnoreCase, "target")
    }
};
```

To allow extra tags, extend the defaults as shown in the settings example above. Alignment is stored as an inline
`text-align` style, indentation as `margin-inline-start` and colours as `color` / `background-color` on a `span`,
so keep `style` and `span` allowed if you use those items. Pasted HTML keeps only these styles. See
[Security](#security) for the rules that apply whatever the policy says.

## Toolbar layouts

A `ToolbarLayout` is an ordered list of entries:

- `ToolbarGroup(params string[] itemIds)`: items rendered together.
- `ToolbarItemRef(string itemId)`: a single item outside a group.
- `ToolbarSeparator()`: a vertical divider.

Every entry has an optional `CssClass` property, which adds a class to the entry's element.

Two layouts are built in:

| Layout | Items |
| --- | --- |
| `ToolbarLayout.Simple` | `bold italic underline` \| `clean-html` \| `alignLeft alignCenter alignRight` |
| `ToolbarLayout.Full` (default) | `bold italic underline strike code subscript superscript` \| `textColor backgroundColor` \| `headings blockquote` \| `ul ol outdent indent` \| `link unlink image table horizontalRule specialCharacters` \| `alignLeft alignCenter alignRight justified` \| `clean-html undo redo findReplace pastePlainText` \| `viewSource fullscreen themeToggle` |

Build your own from `ToolbarItemIds` constants and your own items' ids:

```csharp
using ZauberCMS.RTE.Models;

public static class Toolbars
{
    public static ToolbarLayout Compact { get; } = new(
        new ToolbarGroup(ToolbarItemIds.Bold, ToolbarItemIds.Italic, ToolbarItemIds.Link) { CssClass = "formatting" },
        new ToolbarSeparator(),
        new ToolbarGroup(ToolbarItemIds.Headings, ToolbarItemIds.UnorderedList, ToolbarItemIds.OrderedList),
        new ToolbarSeparator(),
        new ToolbarItemRef(ToolbarItemIds.Undo),
        new ToolbarItemRef(ToolbarItemIds.Redo));
}
```

```razor
<ZauberRichTextEditor @bind-Value="_html" Toolbar="Toolbars.Compact" />
```

Ids that aren't in the registry are skipped. Ids that were never registered are logged at `Warning` level, and ids
removed with `RemoveToolbarItem` at `Debug` level, once each time the editor builds its toolbar. Keyboard shortcuts are
active only for items in the editor's layout, including dropdown children.

### Layouts as JSON

Layouts serialise to and from JSON, for example to store them in configuration:

```csharp
var json = Toolbars.Compact.ToJson();
var layout = ToolbarLayout.FromJson(json);
```

```json
{
  "entries": [
    { "$type": "group", "itemIds": ["bold", "italic", "link"], "cssClass": "formatting" },
    { "$type": "separator" },
    { "$type": "item", "itemId": "undo" }
  ]
}
```

`FromJson` throws a `JsonException` for JSON that isn't a layout, including entries or item ids that are missing or
null, so a bad stored layout fails where you read it rather than inside the editor.

## Built-in toolbar items

Every built-in item is always registered. The layout decides which ones are shown. Use the `ToolbarItemIds`
constants rather than literal strings. Ids are case-insensitive.

| Id | `ToolbarItemIds` | Action | Shortcut |
| --- | --- | --- | --- |
| `bold` | `Bold` | Toggles `<strong>` (also recognises `<b>`) | Ctrl+B |
| `italic` | `Italic` | Toggles `<em>` (also recognises `<i>`) | Ctrl+I |
| `underline` | `Underline` | Toggles `<u>` | Ctrl+U |
| `strike` | `Strike` | Toggles `<s>` (also recognises `<strike>`) | |
| `subscript` | `Subscript` | Toggles `<sub>` | |
| `superscript` | `Superscript` | Toggles `<sup>` | |
| `code` | `Code` | Inline `<code>` on a selection; a `<pre>` code block when nothing is selected | |
| `paragraph` | `Paragraph` | Makes the block a paragraph | |
| `h1` to `h6` | `Heading1` to `Heading6` | Toggles the block between that heading and a paragraph | |
| `codeBlock` | `CodeBlock` | Toggles the block between a `<pre>` code block and a paragraph | |
| `headings` | `Headings` | Dropdown listing `paragraph`, `h1` to `h6` and `codeBlock`; the trigger shows the current one | |
| `textColor` | `TextColor` | Dropdown of nine text colours (`textColor-red`, `-orange`, `-amber`, `-green`, `-teal`, `-blue`, `-purple`, `-pink`, `-grey`) and `textColor-default`, applied as `<span style="color: ...">` | |
| `backgroundColor` | `BackgroundColor` | Dropdown of nine translucent highlights (`backgroundColor-yellow`, `-orange`, `-red`, `-pink`, `-purple`, `-blue`, `-teal`, `-green`, `-grey`) and `backgroundColor-none`, applied as `background-color` | |
| `blockquote` | `Blockquote` | Toggles a block quote | |
| `ul` | `UnorderedList` | Toggles a bulleted list | Ctrl+Shift+8 |
| `ol` | `OrderedList` | Toggles a numbered list | Ctrl+Shift+7 |
| `indent` | `Indent` | Nests list items under the item before them; indents other blocks by 2em | Tab (in a list) |
| `outdent` | `Outdent` | Lifts list items out a level; reduces block indentation. Disabled when there is nothing to outdent | Shift+Tab (in a list) |
| `alignLeft` | `AlignLeft` | Left-aligns the block | |
| `alignCenter` | `AlignCenter` | Centres the block | |
| `alignRight` | `AlignRight` | Right-aligns the block | |
| `justified` | `Justify` | Justifies the block | |
| `link` | `Link` | Opens the link panel (URL, text, title, new window) | Ctrl+K |
| `unlink` | `Unlink` | Removes the link at the cursor | |
| `image` | `Image` | Opens the image panel (URL or upload, alt text); edits the selected image | |
| `table` | `Table` | Opens the table panel (rows, columns, header row; up to 20 x 20) | |
| `horizontalRule` | `HorizontalRule` | Inserts an `<hr>` with the caret in a paragraph after it. Inside a list, the line goes after the list (the list is split at the caret), never into a list item | |
| `specialCharacters` | `SpecialCharacters` | Opens a panel of common characters (© ® ™ € £ ¥ ° ± × ÷ – — … « » ‘ ’ “ ” § ¶ • † ½ ¼ ¾ → ← and a non-breaking space) | |
| `clean-html` | `ClearFormatting` | Removes inline formatting from the selection | |
| `undo` | `Undo` | Undo | Ctrl+Z |
| `redo` | `Redo` | Redo | Ctrl+Y or Ctrl+Shift+Z |
| `findReplace` | `FindReplace` | Opens the find and replace panel (match case, Find next, Replace, Replace all, match count) | Ctrl+H |
| `pastePlainText` | `PastePlainText` | Toggles pasting as plain text | |
| `viewSource` | `ViewSource` | Toggles the HTML source view | |
| `fullscreen` | `Fullscreen` | Toggles full screen; Escape exits. Sits at `--rte-z-fullscreen` (5000), above most host modals; see [Design tokens](#design-tokens) | |
| `themeToggle` | `ThemeToggle` | Switches between light and dark, and raises `ThemeChanged` | |

On macOS, Cmd works wherever Ctrl is listed. To replace or remove a built-in item, see
[Extending the editor](#extending-the-editor).

## Keyboard

- **Shortcuts:** see the table above. A button's tooltip shows its shortcut. Add your own with the `Shortcuts`
  parameter (below).
- **Tab / Shift+Tab** indents and outdents list items, and moves between table cells. Everywhere else, Tab moves focus
  out of the editor as usual.
- **Enter** splits blocks and list items. On an empty list item, Enter outdents it, which ends the list at the top
  level. In a code block, Enter inserts a new line, and Enter on an empty last line leaves the code block for a new
  paragraph. In a table cell Enter inserts a line break. **Shift+Enter** inserts a line break.
- **Ctrl+H** opens find and replace. **Ctrl+Shift+V** always pastes as plain text. **Escape** leaves full screen.
- **Ctrl/Cmd+click** on a link opens it in a new tab. A plain click places the caret, so links stay editable. In
  read-only mode, links open normally.
- In the toolbar, **Left/Right arrows** move between buttons. In a dropdown, **Up/Down/Home/End** move, **Enter** or
  **Space** chooses, and **Escape** closes. In a panel, **Escape** closes and **Enter** submits the built-in panels.

### Keyboard shortcuts

The `Shortcuts` parameter adds key combinations that run your code without a toolbar button, such as Ctrl+S to save.
`Keys` uses the same syntax as an item's `Shortcut` (`Control` matches Ctrl or Cmd), and an invalid combination throws
`ArgumentException` when the `EditorShortcut` is created.

```razor
<ZauberRichTextEditor @bind-Value="_html" Shortcuts="_shortcuts" OnCommandFailed="ShowError" />

@code {
    private string? _html;
    private IReadOnlyList<EditorShortcut> _shortcuts = [];

    protected override void OnInitialized() => _shortcuts = [new("Control+s", SaveAsync)];

    private async Task SaveAsync(IEditorApi api, CancellationToken cancellationToken)
    {
        await Articles.SaveAsync(await api.GetHtmlAsync(cancellationToken), cancellationToken);
        await api.ShowToastAsync("Saved.", ToastType.Success, cancellationToken);
    }
}
```

- Only the registered combinations are reported to .NET, and the browser's own action (such as the Save dialog) is
  prevented. Any pending typing is delivered to `ValueChanged` before the handler runs.
- They work while the editing area has focus, not in the HTML source view. Ctrl+Z, Ctrl+Y and Ctrl+Shift+Z always
  undo and redo.
- An editor shortcut wins over a toolbar item's shortcut with the same keys; if two editor shortcuts share keys, the
  last one wins.
- They are ignored while the editor is `ReadOnly`, like item shortcuts (the browser action is still prevented).
- A handler that throws is handled like a failing item: it's logged, an error toast is shown and `OnCommandFailed` is
  raised. A successful handler raises `OnCommandExecuted`.
- A new list with equal entries (same keys and handler) doesn't reconfigure the editor, so creating the list in markup
  is fine. Changing only a handler takes effect without reconfiguring the script.

## Theming and styling

### Themes

```razor
<ZauberRichTextEditor @bind-Value="_html" Theme="Theme.Dark" />

<ZauberRichTextEditor @bind-Value="_html" @bind-Theme="_theme" />

@code {
    private string? _html;
    private Theme _theme = Theme.Light;
}
```

- `Theme.Auto` (the default) follows the operating system's colour scheme and updates live when it changes.
- `Theme` is reactive, so changing the parameter re-themes the editor immediately.
- With `@bind-Theme`, the built-in `themeToggle` item and `IEditorApi.SetThemeAsync` update your field.

Themes work through CSS `color-scheme`. The root gets `rte-theme-light`, `rte-theme-dark` or `rte-theme-auto`, which
set `color-scheme` to `light`, `dark` or `light dark`. Every colour token is defined with `light-dark()`, so one set of
rules serves both themes. The editor sets its own `color-scheme`, so the host page's scheme doesn't leak in.

### Design tokens

Everything visual is a CSS custom property declared on `.zauber-rte`. Slide-out panels are moved to `<body>`, so
they carry the same tokens on `.zauber-rte-portal`. Override tokens in your own CSS, and include both selectors so the
panels match:

```css
.zauber-rte,
.zauber-rte-portal {
    --rte-color-accent: #7c3aed;
    --rte-color-accent-hover: #6d28d9;
    --rte-color-bg: light-dark(#fffdf8, #18181b);
    --rte-radius-lg: 0.25rem;
    --rte-font-family: "Inter", system-ui, sans-serif;
}
```

To support both themes, use `light-dark(light, dark)` for colours. To style one editor only, give it a class
(`<ZauberRichTextEditor class="brand-editor" ... />`) and scope the override to `.zauber-rte.brand-editor`. That
override doesn't reach its slide-out panel, which uses the page-wide tokens.

| Group | Tokens |
| --- | --- |
| Colour | `--rte-color-bg`, `--rte-color-bg-subtle`, `--rte-color-bg-hover`, `--rte-color-input-bg`, `--rte-color-text`, `--rte-color-text-muted`, `--rte-color-text-subtle`, `--rte-color-border`, `--rte-color-border-strong`, `--rte-color-border-hover`, `--rte-color-accent`, `--rte-color-accent-hover`, `--rte-color-on-accent`, `--rte-color-accent-text`, `--rte-color-selected-bg`, `--rte-color-focus`, `--rte-color-success`, `--rte-color-warning`, `--rte-color-danger`, `--rte-color-backdrop`, `--rte-color-shadow`, `--rte-color-find` |
| Shadow | `--rte-shadow-sm`, `--rte-shadow-md`, `--rte-shadow-lg`, `--rte-shadow-panel` |
| Shape | `--rte-radius-sm`, `--rte-radius-md`, `--rte-radius-lg`, `--rte-radius-round`, `--rte-border-width`, `--rte-border-width-thick`, `--rte-border-width-accent`, `--rte-focus-offset` |
| Spacing | `--rte-space-3xs`, `--rte-space-2xs`, `--rte-space-xs`, `--rte-space-sm`, `--rte-space-md`, `--rte-space-lg`, `--rte-space-xl`, `--rte-block-spacing`, `--rte-list-indent` |
| Sizes | `--rte-toolbar-button-size`, `--rte-icon-size`, `--rte-icon-size-sm`, `--rte-icon-size-lg`, `--rte-separator-size`, `--rte-content-min-height`, `--rte-panel-width`, `--rte-dropdown-min-width`, `--rte-dropdown-max-height`, `--rte-image-handle-size`, `--rte-image-preview-max-height`, `--rte-grip-width`, `--rte-grip-height`, `--rte-toast-width`, `--rte-char-cell-size` |
| Typography | `--rte-font-family`, `--rte-font-mono`, `--rte-font-size`, `--rte-font-size-xs`, `--rte-font-size-sm`, `--rte-font-size-lg`, `--rte-font-size-code`, `--rte-font-size-input`, `--rte-font-weight-medium`, `--rte-font-weight-bold`, `--rte-line-height`, `--rte-line-height-tight`, `--rte-line-height-code`, `--rte-letter-spacing-caps`, `--rte-heading-1` to `--rte-heading-6` |
| Motion and layering | `--rte-duration-fast`, `--rte-duration-base`, `--rte-ease`, `--rte-transition-colors`, `--rte-z-handles`, `--rte-z-toast`, `--rte-z-dropdown`, `--rte-z-fullscreen`, `--rte-z-overlay`, `--rte-opacity-disabled`, `--rte-opacity-muted` |

The layering tokens default to `--rte-z-dropdown: 1000`, `--rte-z-fullscreen: 5000` and `--rte-z-overlay: 10000`
(slide-out panels). A full-screen editor sits above most modal libraries (Blazored Modal uses 100, Bootstrap 1055),
so a custom item that opens your own modal while the editor is full screen shows it underneath. Either lower the
token below your modal (`.zauber-rte { --rte-z-fullscreen: 50; }`), raise your modal above 5000, open the UI as an
editor panel (`OpenPanelAsync`), or leave full screen first with `await api.SetFullscreenAsync(false)`.

`--rte-width`, `--rte-height`, `--rte-min-height` and `--rte-max-height` are written to the root's inline style from
`EditorSettings.Dimensions`. To size the editor from CSS instead, set those dimensions to `null`. On touch devices
(`pointer: coarse`), buttons and handles grow automatically. With `prefers-reduced-motion`, animations are turned off.

### Content font

By default the content inherits the host page's font family, so it looks like the published site. Its text colour
comes from the theme (`--rte-color-text`), because the editor paints its own light or dark background. The host's
rules for individual elements (`p`, `a`, `ul li::before`, `table td` and so on) don't reach the content.

- `--rte-content-font` sets the content's font family only: `.zauber-rte { --rte-content-font: Georgia, serif; }`.
- The `rte-neutral` class (`<ZauberRichTextEditor class="rte-neutral" ... />`) gives the editor a system font instead
  of the page's, for fully neutral styling.

### Cascade layer and host CSS frameworks

All of the editor's rules live in the cascade layer `@layer zauber-rte`, and its selectors are wrapped in `:where()`
(zero specificity). Any rule you write outside a layer wins, without `!important`:

```css
.zauber-rte .rte-content blockquote {
    border-inline-start-color: var(--rte-color-accent);
    font-style: italic;
}
```

The flip side is that a CSS reset that isn't in a layer would also beat the editor's layered rules. Examples are
Bootstrap's Reboot, Tailwind's Preflight and theme rules such as `* { margin: 0 }`, `button { ... }` or
`ul li::before { content: ... }`. So the stylesheet ends with one small unlayered block, scoped to `.zauber-rte`,
`.zauber-rte-portal` and `.rte-slideout-overlay`. On the editor's own elements (the root and every element with an
`rte-` class) and on everything in the content, it sets `all: revert-layer` at single-class specificity: host rules
with only element and universal selectors are discarded there, and the editor's layered rules (or the browser
defaults) apply instead. A few properties that an attribute selector such as `[role=dialog]` could tie are also set
at two-class specificity. So Bootstrap and Tailwind v3 need nothing extra, and the editor looks the same with or
without them (the demo's `/host-css` page shows it).

Your stylesheets come after the editor's, so a class selector of your own wins over the guard, as in the example
above; for the few two-class properties, match that (for example `.zauber-rte .rte-btn`). Classes you put on content
(`.callout`, `.ink-red`) and markup in your own panels without `rte-` classes are left alone, and rules that reach
the content through a class on a parent (`.prose p`) still apply.

Tailwind v4 already uses cascade layers. Declare the layer order once, before any stylesheet loads, so Preflight
(`base`) sits below the editor and your utilities sit above it:

```css
@layer theme, base, zauber-rte, components, utilities;
```

### CSS classes

The editor's markup uses stable `rte-*` class names that you can target. These are the main ones:

- `.zauber-rte`: the root, plus `.rte-readonly`, `.rte-source-mode` and `.rte-fullscreen`. While an editor is full
  screen, `<html>` has `.rte-scroll-lock`, which hides the page's scrollbar; it's the only style the editor applies
  outside its own elements.
- `.rte-toolbar`, `.rte-toolbar-block`, `.rte-toolbar-single-item` and `.rte-toolbar-separator`: the toolbar and its
  entries.
- `.rte-toolbar-btn` (plus `.rte-btn-active`): a toolbar button. `.rte-dropdown-{id}` and `.rte-dropdown-menu`: a
  dropdown and its menu.
- `.rte-content`: the editable area. `.rte-source-view`: the source textarea.
- `.rte-slideout-panel`: a panel. `.rte-toast`: a toast.

Icons render inside `<span class="rte-icon">`.

## Accessibility

- **Toolbar:** `role="toolbar"` labelled "{AriaLabel} toolbar", with groups (`role="group"`) and separators
  (`role="separator"`). Roving tabindex makes the toolbar a single Tab stop, and the arrow keys move between enabled
  buttons.
- **Buttons:** every button has an `aria-label` (the item's `Label`) and a tooltip that includes its shortcut. Toggle
  items expose `aria-pressed`. Disabled items use the native `disabled` attribute.
- **Dropdowns:** the trigger has `aria-haspopup="menu"` and `aria-expanded`. The menu is `role="menu"`, toggle children
  are `menuitemradio` with `aria-checked`, and focus returns to the trigger on close.
- **Editing area:** `role="textbox"`, `aria-multiline="true"`, `aria-label` and `aria-readonly`. Tab isn't trapped
  outside lists and tables.
- **Panels:** `role="dialog"`, `aria-modal="true"` and labelled by the panel's `Title`. Focus moves into the panel and
  is trapped while it's open. Escape closes it, and focus returns to the editor.
- **Toasts:** announced through a `role="status"` live region.
- **Visuals:** `:focus-visible` rings, larger targets on touch screens, and no motion with `prefers-reduced-motion`.

## Security

- **Every way into the editor is sanitised** with DOMPurify in the browser, using the editor's `HtmlPolicy`: the
  initial `Value` and later parameter changes, `SetHtmlAsync`, `InsertHtmlAsync` (and so every panel), paste, drop,
  and edits in the HTML source view.
- **Always removed, whatever the policy says:** `script`, `style`, `iframe`, `frame`, `frameset`, `object`, `embed`,
  `form`, `input`, `button`, `select`, `textarea`, `svg`, `math`, `template`, `link`, `meta`, `base`, `noscript`, and
  all `on*` event-handler attributes.
- **URL schemes are allow-listed.** `href` and `src` keep only `http:`, `https:`, `mailto:`, `tel:` and relative URLs;
  `javascript:` and similar are stripped. Images also accept `data:image/...` when `AllowDataUrls` is on, and only
  same-origin sources when `AllowExternalImages` is off. Links with `target="_blank"` get
  `rel="noopener noreferrer"`. Ctrl/Cmd+click opens a link only if its scheme is allowed, and always with `noopener`.
- **`HtmlBuilder` encodes** text and attribute values and rejects invalid tag and attribute names, so values typed
  into panels can't break out of an attribute.
- **Sanitise on the server too.** The editor's sanitising runs in the user's browser, and a modified client can post
  any HTML to your app. Treat the bound value as untrusted input: sanitise it on the server before you store it or
  render it as `MarkupString`, or only render it where untrusted HTML is safe (for example a sandboxed `iframe`, as the
  demo app does). A `Value` you set is sanitised before display, but the bound field only holds the sanitised HTML
  after the next edit.
- `style` is allowed by default because alignment uses it. If untrusted users author content and you don't need
  alignment, remove `style` from `AllowedAttributes`.

## Large documents

Content changes are sent to .NET in pieces of at most 12 KB (UTF-8), and the bound value is set as soon as the last
piece arrives, so a Save click right after an edit sees it. Everything the API reads back from the browser
(`GetHtmlAsync`, `GetTextAsync`, `GetSelectionAsync`, `GetLinkAtCursorAsync` and `GetImageAtCursorAsync`, whose `Src`
can be a large `data:` URL) is read through a JavaScript stream reference (`IJSStreamReference`). Every other message
from the browser is small. As a result, Blazor Server's default 32 KB `MaximumReceiveMessageSize` is enough for any
document and for base64 images, and you don't need `AddSignalR(o => o.MaximumReceiveMessageSize = ...)` for the
editor. Content above `EditorSettings.MaxContentBytes` (10 MB by default) isn't transferred: the bound value keeps its
last value, the user sees a warning, the warning is logged, and the read methods throw
`EditorContentTooLargeException`. Typing is debounced (`ChangeDelay`), so a large document isn't re-sent on every
keystroke, and a toolbar item runs only after the latest content has reached the bound value.

## Extending the editor

Register custom toolbar items, dropdowns and panels in `AddZauberRte`:

```csharp
builder.Services.AddZauberRte(options =>
{
    options.AddToolbarItem<HighlightItem>();
    options.AddToolbarItem<InsertTimeItem>();
    options.RemoveToolbarItem(ToolbarItemIds.Image);
});
```

Then add their ids to a [layout](#toolbar-layouts). To let users choose their own buttons, inject
`IToolbarItemCatalog` (scoped) and call `GetItems()`: it returns a `ToolbarItemDescriptor` (`Id`, `Label`, `Tooltip`,
`IconSvg`, `IconCss`, `Shortcut`, `Kind`, `ChildItemIds`, `IsBuiltIn`) for every registered item, built-ins first.
Removed items aren't listed, and an override is listed with `IsBuiltIn = false`. The
[extension guide](https://github.com/YodasMyDad/ZauberEditor/blob/master/QUICK_START_EXTENDING.md) covers buttons,
toggles, dropdowns, panels, dependency injection, overriding built-ins and icons, with working code from the demo app.

---

## Migrating to 4.0

4.0 adds .NET 11 support. The package now targets both `net10.0` and `net11.0`, so an app on .NET 10 (LTS) or
.NET 11 can use it, and NuGet picks the matching build. Apps on .NET 9 or earlier stay on 3.x.

The editor now loads its own stylesheet:

- Remove `<link href="_content/ZauberCMS.RTE/css/zauber-editor.css" rel="stylesheet" />` from `App.razor`, or keep
  it: the editor finds an existing link and doesn't add a second.
- If you bundle the CSS yourself, set `options.AutoLoadStyles = false` in `AddZauberRte`.
- With a Content-Security-Policy, `style-src` must allow `'self'`.
- The unlayered guard now covers every property of the editor's elements and the content. A host rule that styled
  the content through element selectors alone (`p { color: ... }`) no longer reaches it; use a class selector
  (`.zauber-rte .rte-content p`) or the `--rte-*` tokens.

There are no other API changes from 3.1: update the package reference and rebuild.

## Migrating to 3.0

3.0 rebuilds the editor's internals and trims the public API. The consumer namespaces stay the same
(`ZauberCMS.RTE.Components`, `ZauberCMS.RTE.Models`, `ZauberCMS.RTE.Services`). Everything else listed below changed.
Work through the sections in order, then build: most breaks are compile errors that point at the line to change. The
full list of fixes is in the [changelog](https://github.com/YodasMyDad/ZauberEditor/blob/master/CHANGELOG.md).

### 1. Host page and services

| 2.x | 3.0 |
| --- | --- |
| `<script src="_content/ZauberCMS.RTE/js/lib/purify.min.js">` | **Remove.** DOMPurify is now imported as an ES module. |
| `<script src="_content/ZauberCMS.RTE/js/zauber-rte.js">` | **Remove.** The file is now an ES module that the component imports. Loading it as a classic script throws a syntax error. |
| `<link href="_content/ZauberCMS.RTE/css/fontawesome.min.css">` | **Remove.** Font Awesome is no longer shipped (see [Icons](#7-icons-css-and-themes)). |
| `<link href="_content/ZauberCMS.RTE/css/zauber-editor.css">` | Keep (optional from 4.0, which loads it itself). |
| `AddSignalR(o => o.MaximumReceiveMessageSize = ...)` for large content or base64 images | Not needed for the editor; content changes arrive in pieces under 12 KB, and every value the API reads back is streamed. |
| WebAssembly not really supported (the JS was a page-wide global) | Interactive Server, WebAssembly and Auto supported. For WebAssembly or Auto, call `AddZauberRte` in the client project too. |
| Browsers: "Chrome 80+" etc. | Chrome/Edge 123+, Firefox 120+, Safari 17.5+ (CSS `light-dark()`, cascade layers, nesting). |

### 2. Registration (`AddZauberRte`)

| 2.x | 3.0 |
| --- | --- |
| `AddZauberRte()` scanned the entry assembly for `IToolbarItem` types | `AddZauberRte()` registers only the built-in items. **Custom items no longer appear unless you register them.** |
| `AddZauberRte(params Assembly[] assemblies)` | `AddZauberRte(o => o.AddToolbarItem<MyItem>())` (preferred, trim-safe), or `AddZauberRte(o => o.AddToolbarItemsFromAssembly(assembly))` (reflection; marked `[RequiresUnreferencedCode]`). Scanning registers every concrete `IToolbarItem` in the assembly, including items meant to be registered as instances (for example dropdown children that take constructor arguments). Types DI can't construct are skipped and logged as errors. Prefer explicit `AddToolbarItem<T>()`. |
| `AddZauberRte(Action<ZauberRteOptions>, params Assembly[])` | `AddZauberRte(Action<ZauberRteOptions>? configure = null)` |
| `ZauberRteOptions.Assemblies` | `options.AddToolbarItemsFromAssembly(assembly)` |
| `ZauberRteOptions.AllowOverrides` | Removed. Registrations apply in order, and a later item with the same id always replaces the earlier one (including built-ins). |
| No way to remove a built-in item | `options.RemoveToolbarItem(id)` |
| | New: `options.AddToolbarItem(IToolbarItem instance)` registers one stateless instance shared by every editor. |
| Items were created per DI scope and cached | Items registered by type are created **per editor instance** through `ActivatorUtilities`, so they can inject scoped services and are disposed with the editor if they implement `IDisposable`/`IAsyncDisposable`. |
| `ToolbarDiscoveryService`, `ToolbarItemMetadata`, `ToolbarItemFactory` (public) | Internal; not part of the API. |
| `IZauberJsRuntime`, `ZauberJsRuntime`, `SelectionInterop`, `ClipboardInterop`, `ImageInterop`, `MutationInterop`, `PanelInterop`, `HistoryInterop` (public) | Removed. Use `IEditorApi`. |

### 3. Toolbar items (`IToolbarItem`, `ToolbarItemBase`)

| 2.x | 3.0 |
| --- | --- |
| `ToolbarPlacement Placement` (abstract) and the `ToolbarPlacement` enum | Removed. Delete your `Placement` overrides. |
| `string IconCss` (abstract); rendered as `<i class="fas {IconCss}">` | `string? IconCss` (virtual, optional). Rendered **as given**, with no `fas` prefix: `"fa-bold"` becomes `"fas fa-bold"` (or `"fa-solid fa-bold"`), and you load your own icon font. |
| | New: `string? IconSvg`, trusted inline SVG markup (preferred). With neither icon, the button shows `Label` as text. |
| `ToolbarItemType ItemType` (`Button`, `Dropdown`, `Separator`) | `ToolbarItemKind Kind` (`Button`, `Dropdown`). Separators are layout entries (`ToolbarSeparator`). |
| `List<IToolbarItem> ChildItems` | `IReadOnlyList<string> ChildItemIds`. Children are looked up by id, so they must be registered themselves, and overrides apply to them. |
| `bool IsDropdownLabel` | Removed. A dropdown's trigger shows the active child's label and icon automatically. |
| `Type? PanelComponent` | Removed (it was never read). Open panels from `ExecuteAsync` with `await api.OpenPanelAsync<MyPanel>()`. |
| `Task ExecuteAsync(IEditorApi api)` | `Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken)`. The token is cancelled when the editor is disposed; pass it to the API and to your own async work. |
| `string[] TrackedTags` | `IReadOnlyList<string> TrackedTags` |
| `ToolbarItemBase.IsEnabled` returned `true` | Returns `false` while the editor is read-only or showing HTML source. Override it to change that. |
| `ToolbarItemBase.IsActive` returned `false` | Returns `true` when `PrimaryTag` is in `State.ActiveMarks`, so toggles only need `TrackedTags` and `PrimaryTag`. |
| `Shortcut`: `Control` matched only Ctrl; `Ctrl+Shift+7`/`8` didn't fire on US/UK layouts; every registered item's shortcut was active | `Control` means Ctrl on Windows/Linux and Cmd on macOS; digits match the physical key; only items in the editor's layout respond. |
| Built-in item classes in `ZauberCMS.RTE.Models.ToolbarItems` (`BoldItem`, `Heading1Item`, `TableItem`, ...) and `DefaultToolbarItems.GetAllDefaultItems()` | Internal. Refer to built-ins by id through `ToolbarItemIds` (ids are unchanged). Remove `@using ZauberCMS.RTE.Models.ToolbarItems`. |
| `ZauberCMS.RTE.Examples` (`CustomDropdownExamples`, `CustomLayoutExample`, `CustomToolbarItemTemplate`, `CustomPanelTemplate`, `CustomDropdownUsage`, `UsageExample`) compiled into the package | Removed from the package. Equivalent working examples live in the demo app (`ZauberRTE.Web/Toolbar/`). |
| `table` item existed but wasn't registered or in any layout | Registered by default and included in `ToolbarLayout.Full`. |

### 4. `IEditorApi`

Every async member gained an optional `CancellationToken cancellationToken = default` as its last parameter. Callers
aren't affected, but your own implementations of `IEditorApi` (for example test fakes) must update.

| 2.x | 3.0 |
| --- | --- |
| No public way to get the API outside a toolbar item (`editor.EditorApi` in the old docs didn't exist) | `ZauberRichTextEditor.Api` (use `@ref`) |
| `EditorState GetState()` | `EditorState State { get; }` |
| `EditorSettings GetSettings()` | `EditorSettings Settings { get; }` |
| `Task<string> GetCurrentBlockTypeAsync()` | `State.BlockType` |
| `Task<int> GetCurrentHeadingLevelAsync()` | `State.HeadingLevel` |
| `SelectionInfo? GetSelection()` (synchronous) | `GetSelectionAsync()` |
| `string? GetHtml()` (synchronous, could be stale) | `Task<string> GetHtmlAsync()` |
| `string? GetText()` (always returned `null`) | `Task<string> GetTextAsync()` |
| `SetTextAsync(string text)` | Removed. Use `SetHtmlAsync(HtmlBuilder.Element("p").Text(text).Build())`. |
| `ReplaceSelectionAsync(string html)` | `InsertHtmlAsync(html)` (it was an alias). |
| `CleanHtmlAsync()` | `ClearFormattingAsync()` |
| `BlurAsync()` (did nothing) | Removed. |
| `SaveSelectionRangeAsync()` | `SaveSelectionAsync()` |
| `Task<bool> RestoreSelectionRangeAsync()` | `Task RestoreSelectionAsync()` |
| `ClearSavedSelectionRangeAsync()` | `ClearSavedSelectionAsync()` |
| `Task<bool> SelectLinkAtCursorAsync()` / `Task<bool> SelectImageAtCursorAsync()` | `Task SelectLinkAtCursorAsync()` / `Task SelectImageAtCursorAsync()` |
| `OpenPanelAsync(Type componentType, object? parameters)` (non-dictionary parameters were silently dropped) | `OpenPanelAsync<TPanel>(IReadOnlyDictionary<string, object?>? parameters = null) where TPanel : PanelBase` |
| `WrapSelectionAsync(string tagName, Dictionary<string, string>?)`, `SetBlockTypeAsync(..., Dictionary<string, string>?)`, `SetBlockStyleAsync(Dictionary<string, string>)` | Take `IReadOnlyDictionary<string, string>` (a `Dictionary` still works). Parameter names are now `tag` and `blockType`, which matters only for named arguments. |
| `ToggleMarkAsync(string markName)` | `ToggleMarkAsync(string tag)` |
| `ShowToastAsync` did nothing | Shows a toast. |
| `SetHtmlAsync` didn't update the bound value or the history | Undoable, and raises `ValueChanged` and `OnChange` (`ChangeSource.Api`). |
| | New: `SetThemeAsync(Theme)`. |
| `PanelParameters` class | Removed (unused). |

### 5. State, info and event types

All of these are now immutable `sealed record`s. Use `with` to copy.

| 2.x | 3.0 |
| --- | --- |
| `EditorState` (mutable class) `CurrentBlockType` (`string?`) | `BlockType` (`string`, lower-case, `"p"` by default) |
| `CurrentHeadingLevel` (`int?`) | `HeadingLevel` (`int`, 0 when not a heading) |
| `CurrentAlignment` | `Alignment` (now actually reported) |
| `ActiveMarks` (`HashSet<string>`) | `IReadOnlySet<string>` of primary tags (case-insensitive) |
| `CanFormat`, `IsDirty`, `WordCount`, `CharacterCount`, `Clone()` | Removed (never populated). |
| | New: `InLink`, `InImage`, `InTable`, `IsReadOnly`, `Theme`, `PrefersDarkScheme`, `IsDark`, `EditorState.Empty`. |
| `TextAlignment.Justified` | `TextAlignment.Justify` |
| `SelectionInfo { StartOffset, EndOffset, IsCollapsed, SelectedText, SelectedHtml }` | `SelectionInfo(string Text, bool IsCollapsed)` |
| `LinkInfo { Href, Target, Rel, Title, Text }` | `LinkInfo(string Href, string Text, string? Title, string? Target)` with an init-only `Rel` property (removed in 3.0.0, restored in 3.1.0). |
| `ImageInfo { Src, Alt, Width (string?), Height (string?), IsDataUrl }` | `ImageInfo(string Src, string? Alt, string? Title, int? Width, int? Height)`; use `Src.StartsWith("data:")` instead of `IsDataUrl`. |
| `EditorEventArgs` base class (with `Timestamp`) | Removed; event args no longer derive from `System.EventArgs`. |
| `EditorChangeArgs { Html, Text, IsUserInput, Source }` | `EditorChangeArgs(string Html, ChangeSource Source)`. `Text` was HTML anyway; call `Api.GetTextAsync()` if you need text. Replace `IsUserInput` with a check on `Source`. |
| `ChangeSource.User`, `Paste`, `Command`, `Programmatic`, `UndoRedo` | `Input`, `Paste`, `Command`, `Api`, `History`, plus new `SourceView` and `ImageResize`. |
| `CommandExecutedArgs { Command, Parameters, Success, ErrorMessage }` | `CommandExecutedArgs(string ItemId)`, raised only on success. Failures are logged and shown as an error toast. |
| `ImageResizedArgs { ImageSrc, NewWidth, NewHeight, OriginalWidth, OriginalHeight, MaintainedAspectRatio }` (never raised) | `ImageResizedArgs(int Width, int Height)`, raised after each resize. |
| `PasteArgs`, `SelectionChangedArgs`, `ZauberKeyboardEventArgs` (with `MatchesShortcut`) | Removed with their events (see below). |

### 6. The `ZauberRichTextEditor` component

| 2.x | 3.0 |
| --- | --- |
| `OnKeyDown` (`ZauberKeyboardEventArgs`; `Cancel()` had no effect) | Removed. Use a toolbar item with a `Shortcut`, and put that item in the editor's layout (it shows as a button): shortcuts are active only for items in the layout. |
| `OnPaste` (`PasteArgs`; raised after the insert) | Removed. Use `OnChange` and check `Source == ChangeSource.Paste`. |
| `OnSelectionChanged` (`SelectionChangedArgs`; never fired) | Removed. Use `OnStateChanged` (`EditorState`). |
| `SetContentAsync(string?)` (public) | Removed. Set `Value`, or call `Api.SetHtmlAsync`. |
| `[JSInvokable]` `OnPasteCallbackAsync`, `OnImageClickCallbackAsync`, `OnContentModifiedAsync` (public) | Removed from the component (now internal callbacks). |
| `IDisposable.Dispose()` | `IAsyncDisposable.DisposeAsync()` |
| Toolbar layout only through `Settings.ToolbarLayout` | New `Toolbar` parameter, which overrides it. |
| `Theme` read once at start; `Auto` rendered light | Reactive. `Auto` follows the OS (so users with a dark OS now see a dark editor; set `Theme="Theme.Light"` to keep 2.x's look). New `ThemeChanged` for `@bind-Theme`. |
| `ReadOnly` ignored | Implemented. |
| `ValueChanged` fired on every keystroke and on every toolbar click, and with `null` when an interop call failed | Debounced by `Settings.ChangeDelay`, flushed on blur and before commands, raised only when the HTML actually changed, and never with a value from a failed call. |
| No `EditForm` support | `ValueExpression` (passed by `@bind-Value`) notifies the `EditContext`. |
| | New: `Placeholder`, `AriaLabel`, `OnStateChanged`, `AdditionalAttributes`. |
| Root element had an `id` | No `id`. Pass your own through `AdditionalAttributes` if you need one. |
| `ToolbarHost`, `ToolbarButton`, `ToolbarDropdown`, `SlideOutPanel`, `Panels.LinkPanel`/`ImagePanel`/`TablePanel` (public, in `ZauberCMS.RTE.Components` and `ZauberCMS.RTE.Components.Panels`) | Moved to `ZauberCMS.RTE.Components.Internal` and hidden from IntelliSense; `ToolbarHost` is now `EditorToolbar`. They're implementation details, so don't use them directly. Remove `@using ZauberCMS.RTE.Components.Panels`. |

### 7. Icons, CSS and themes

| 2.x | 3.0 |
| --- | --- |
| Font Awesome CSS bundled; fonts loaded from cdnjs | Removed. Built-in icons are inline SVG. For custom items, use `IconSvg`, or load your own icon font and use `IconCss`. |
| `<i class="fas fa-...">` in toolbar markup | `<span class="rte-icon" aria-hidden="true">` containing the SVG (or `<i class="{IconCss}">`). |
| Documented variables `--rte-primary-color`, `--rte-border-color`, `--rte-background-color`, `--rte-text-color`, `--rte-toolbar-height`, `--rte-font-family` (none existed) | Real tokens: `--rte-color-accent`, `--rte-color-border`, `--rte-color-bg`, `--rte-color-text`, `--rte-toolbar-button-size`, `--rte-font-family`, and the [full list](#design-tokens). |
| 265 `!important` declarations | None. Rules are in `@layer zauber-rte` with zero-specificity selectors, so your CSS wins without `!important`. Remove `!important` from your overrides; some host styles you relied on being overridden may now apply. See [Cascade layer and host CSS frameworks](#cascade-layer-and-host-css-frameworks). |
| Dark theme by duplicated `.zauber-rte.rte-theme-dark ...` selectors | `color-scheme` plus `light-dark()` tokens. Your `.rte-theme-dark` selectors still match, but prefer overriding tokens with `light-dark()`. `rte-theme-auto` now works. |
| Image resize handles `.rte-resize-handle` and `.rte-resize-handle-{n,ne,e,se,s,sw,w,nw}`, positioned `fixed` | `.rte-image-resize-handles` > `.rte-image-resize-handle[data-handle="nw\|ne\|sw\|se"]`, positioned inside the editor (corners only). |
| `.rte-toolbar-icon`, `.rte-toolbar-row`, `.rte-drop-zone`, `.rte-heading-label`, `.rte-dropdown-label-item`, `.resizing` | Removed. |
| | New classes: `rte-icon`, `rte-readonly`, `rte-source-mode`, `rte-empty`, `rte-toast`, `rte-toast-{success\|warning\|error}`, `rte-toast-region`, `rte-dropdown-backdrop`, `rte-dropdown-{id}`, `rte-form-error`, `rte-image-preview`, `rte-button-row`, `zauber-rte-portal` (portalled panels). |

### 8. Settings

The settings types changed from mutable classes (`set`) to immutable `sealed record`s (`init`). Object initialisers
still compile. Code that changes a property after creation doesn't compile: build a new instance, or use `with`.

| 2.x | 3.0 |
| --- | --- |
| `EditorSettings.DefaultTheme` | Removed. Use the component's `Theme` parameter. |
| `EditorSettings.CmsDefault()` | `new EditorSettings { ImageConstraints = new() { MaxWidth = 800, MaxHeight = 600 } }` |
| `EditorSettings.Minimal()` | `new EditorSettings { ToolbarLayout = ToolbarLayout.Simple }` |
| `EditorSettings.BlockTags` | Removed; block detection is built in. |
| `EditorSettings.ToolbarLayout` defaulted to `ToolbarLayout.Default` | Defaults to `ToolbarLayout.Full`. |
| | New: `HistoryLimit` (100), `ChangeDelay` (250 ms), `MaxContentBytes` (10 MB). |
| `EditorDimensions.MinHeight` / `MaxHeight` (`int?`, ignored) | `string?` CSS lengths (`"100px"` / `null`), now applied. `Height` is now the real height (it used to be applied as a minimum height). |
| `ImageConstraints.MaxWidth` / `MaxHeight` (`int`, 800/600, ignored) | `int?`, `null` (unlimited) by default, enforced while resizing. |
| `ImageConstraints.MaintainAspectRatio` (ignored) | Honoured. |
| `ImageConstraints.AllowedImageTypes` (`HashSet<string>`) | `IReadOnlySet<string>`; `.webp` added to the defaults. |
| | New: `ImageConstraints.MaxFileSizeBytes` (5 MB). |
| `HtmlPolicy.AllowedTags` (`HashSet<string>`) | `IReadOnlySet<string>`. Defaults add `h4`–`h6`, `strike`, `caption`, `colgroup`, `col` and `tfoot`. |
| `HtmlPolicy.AllowedAttributes` (`Dictionary<string, HashSet<string>>` per tag, with `"*"` for all) | `IReadOnlySet<string>`, one flat set that applies to every allowed tag. Defaults: `class id title lang dir role style data-* aria-* href target rel src alt width height colspan rowspan scope`. Removed from the defaults: `tabindex`, `max-width`, `border`, `cellpadding`, `cellspacing`, `align`, `valign`. |
| `HtmlPolicy` applied only to paste; `AllowDataUrls` and `AllowExternalImages` ignored | Applied to every entry point, including the initial `Value`, and both flags are enforced. **Stored content with tags or attributes outside the policy is now stripped when it loads.** Extend the policy before upgrading if your content uses other markup. |

### 9. Toolbar layouts

| 2.x | 3.0 |
| --- | --- |
| `class ToolbarLayout { List<ToolbarLayoutItem> LayoutItems }` | `sealed record ToolbarLayout(params IReadOnlyList<ToolbarLayoutEntry> Entries)` |
| `ToolbarLayout.FromItems(a, b, c)` | `new ToolbarLayout(a, b, c)` |
| `layout.LayoutItems`, `layout.GetEffectiveLayout()` | `layout.Entries` |
| `ToolbarLayout.Default`, `ToolbarLayout.Cms` | `ToolbarLayout.Full` (both were aliases of it). `Full` now also contains `table`. |
| `ToolbarLayoutItem` (abstract class with `Type` and the `LayoutItemType` enum) | `ToolbarLayoutEntry` (abstract record with `CssClass`); use pattern matching instead of `Type`. |
| `new ToolbarBlock("bold", "italic")` / `ToolbarBlock.Items` | `new ToolbarGroup("bold", "italic")` / `ToolbarGroup.ItemIds` |
| `new ToolbarBlock("css-class", items)` | `new ToolbarGroup([.. items]) { CssClass = "css-class" }` |
| `new ToolbarItemReference(id, "css-class")` | `new ToolbarItemRef(id) { CssClass = "css-class" }` |
| `new ToolbarSeparator("css-class")` | `new ToolbarSeparator { CssClass = "css-class" }` |
| `ToolbarLayoutExtensions.Block(...)`, `.Item(...)`, `.Separator(...)` | Removed; use the constructors above. |
| JSON: `{"LayoutItems":[{"$type":"block","Type":0,"Items":[...],"CssClass":null}]}` | JSON: `{"entries":[{"$type":"group","itemIds":[...]}]}` (camelCase; discriminators `group`, `item`, `separator`). Convert stored 2.x JSON. |

### 10. `HtmlBuilder`

| 2.x | 3.0 |
| --- | --- |
| Attribute values written unencoded | Encoded. If you pre-encoded values, stop, or they'll be double-encoded. Invalid tag or attribute names throw `ArgumentException`. |
| `Html(string html)` (raw HTML) | Removed. Compose with `Child(...)`, or insert raw HTML separately with `InsertHtmlAsync` (which sanitises it). |
| `Id(string id)` / `Style(string style)` | `Attr("id", id)` / `Attr("style", style)` |
| `Attr(string name, string value)` | `Attr(string name, string? value)`: `null` leaves the attribute out, and setting the same name again replaces it. |
| `Attrs(Dictionary<string, string>)` | `Attrs(IReadOnlyDictionary<string, string>)` |
| Implicit conversion to `string` | Removed. Call `Build()`. |

### 11. Panels (`PanelBase`)

| 2.x | 3.0 |
| --- | --- |
| Title derived from the component's type name | `public abstract string Title { get; }`: **you must override it**. It's the panel heading and the dialog's accessible name. |
| `IEditorApi? Api` | `IEditorApi Api` (never null inside a panel). |
| `CloseAsync()` also cleared the saved selection | `CloseAsync()` only closes. Call `Api.ClearSavedSelectionAsync()` yourself when you cancel after saving a selection. |
| `PanelBase : IDisposable` with `OnDispose()` | No longer disposable; implement `IDisposable` on your panel if it needs cleanup. |
| | New: `ElementId(name)` returns an element id unique to the panel instance (for `<label for>`). |
| Opened with `OpenPanelAsync(typeof(MyPanel), new Dictionary<string, object> { ... })` | `OpenPanelAsync<MyPanel>(new Dictionary<string, object?> { [nameof(MyPanel.Value)] = value })` |
| Exceptions in a panel were swallowed | Panels render inside an error boundary that shows a message. |

### 12. Behaviour changes to check

- **Sanitisation everywhere.** The `HtmlPolicy` now applies to the initial `Value`, so check that your stored content
  fits it.
- **Debounced value.** `ValueChanged` arrives after a pause in typing, not per keystroke. It's always flushed on blur,
  so form submit buttons see the final value.
- **Tab** leaves the editor except inside lists and tables (2.x trapped focus).
- **Pasted or dropped image files** are embedded as `data:` URLs when `HtmlPolicy.AllowDataUrls` is on (the default).
  Turn it off to block them.
- **Shortcuts** respond only for items in the layout, and Cmd works on macOS.
- **Multiple editors** on one page are fully independent (2.x sent every paste to the last editor initialised).

## License

MIT. See [LICENSE](https://github.com/YodasMyDad/ZauberEditor/blob/master/LICENSE).
