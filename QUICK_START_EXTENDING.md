# Extending ZauberCMS.RTE

This guide shows how to add your own toolbar buttons, toggles, dropdowns and slide-out panels, and how to replace or
remove the built-in ones. Every sample comes from the demo app (`ZauberRTE.Web/Toolbar/`), which builds and runs, so
you can open the real files alongside this guide.

It assumes the editor is already set up as described in the [README](README.md).

## Contents

1. [How toolbar items work](#how-toolbar-items-work)
2. [A button](#a-button)
3. [Using services (dependency injection)](#using-services-dependency-injection)
4. [A toggle that tracks formatting](#a-toggle-that-tracks-formatting)
5. [Registering items and adding them to the toolbar](#registering-items-and-adding-them-to-the-toolbar)
6. [A dropdown](#a-dropdown)
7. [A slide-out panel](#a-slide-out-panel)
8. [Overriding a built-in item](#overriding-a-built-in-item)
9. [Removing a built-in item](#removing-a-built-in-item)
10. [Custom layouts](#custom-layouts)
11. [Letting users pick their toolbar](#letting-users-pick-their-toolbar)
12. [Icons](#icons)
13. [Checklist](#checklist)
14. [The demo app](#the-demo-app)

## How toolbar items work

A toolbar item implements `IToolbarItem`. In practice you derive from `ToolbarItemBase`, which supplies sensible
defaults, so you override only `Id`, `Label` and `ExecuteAsync`.

| Member | Default in `ToolbarItemBase` | Purpose |
| --- | --- | --- |
| `Id` | abstract | Unique, case-insensitive key. Layouts refer to it, and registering another item with the same id replaces this one. |
| `Label` | abstract | Accessible name (`aria-label`). Shown as text when there's no icon. |
| `Tooltip` | `null` | Tooltip text; falls back to `Label`. The shortcut is appended automatically. |
| `IconSvg` | `null` | Trusted inline SVG markup. |
| `IconCss` | `null` | CSS classes for an icon font, rendered as given. |
| `Shortcut` | `null` | For example `Control+Shift+H`. `Control` means Ctrl, or Cmd on macOS. |
| `IsToggle` | `false` | Shows a pressed state (`aria-pressed`). |
| `Kind` | `ToolbarItemKind.Button` | `Button` or `Dropdown`. |
| `ChildItemIds` | `[]` | A dropdown's children, by id. |
| `TrackedTags` | `[]` | Tags that count as this item's formatting when they wrap the selection. |
| `PrimaryTag` | `null` | The canonical tag the tracked tags are reported as. |
| `IsEnabled(state)` | `false` when read-only or in source view | Whether the button can be used. |
| `IsActive(state)` | `PrimaryTag` is in `state.ActiveMarks` | Whether a toggle shows as pressed. |
| `ExecuteAsync(api, cancellationToken)` | abstract | The action. `api` is the `IEditorApi` of the editor that was clicked; `cancellationToken` is cancelled when that editor is disposed, so pass it on to the API and to your own async work. |

Behaviour worth knowing:

- **One instance per editor.** Items registered by type are created for each editor instance, so per-item fields are
  never shared between editors. If an item implements `IDisposable` or `IAsyncDisposable`, it's disposed with its
  editor.
- **Errors are handled for you.** If `ExecuteAsync` throws, the editor logs the exception and shows an error toast, so
  you don't need a `try`/`catch` per item. `OnCommandExecuted` is raised only when the item succeeds, and
  `OnCommandFailed` (with the item id and the exception) when it throws.
- **The policy still applies.** Anything you insert goes through the editor's `HtmlPolicy`. If your item emits a tag
  or attribute that isn't allowed by default (`mark`, `aside`, ...), add it to the policy (see
  [A toggle](#a-toggle-that-tracks-formatting)).

## A button

The simplest item inserts something. This one inserts the current time (`ZauberRTE.Web/Toolbar/InsertTimeItem.cs`):

```csharp
using System.Globalization;
using ZauberCMS.RTE.Models;

namespace ZauberRTE.Web.Toolbar;

public sealed class InsertTimeItem(TimeProvider timeProvider) : ToolbarItemBase
{
    public const string ItemId = "insertTime";

    public override string Id => ItemId;

    public override string Label => "Insert current time";

    public override string? IconSvg => DemoIcons.Clock;

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(api);
        return api.InsertTextAsync(timeProvider.GetLocalNow().ToString("HH:mm:ss", CultureInfo.InvariantCulture), cancellationToken);
    }
}
```

Exposing the id as a `const` lets layouts refer to `InsertTimeItem.ItemId` instead of repeating the string.

`IEditorApi` has everything an item needs: `InsertHtmlAsync`, `InsertTextAsync`, `ToggleMarkAsync`,
`WrapSelectionAsync`, `UnwrapSelectionAsync`, `SetBlockTypeAsync`, `SetBlockStyleAsync`, `ClearFormattingAsync`,
`GetSelectionAsync`, the link and image helpers, `UndoAsync`/`RedoAsync`, `OpenPanelAsync`, `ShowToastAsync`, and the
current `State` and `Settings`. The [README](README.md#programmatic-control-with-ieditorapi) lists them all.

## Using services (dependency injection)

Items registered with `AddToolbarItem<T>()` are created with `ActivatorUtilities` from the editor's scoped service
provider. So the primary constructor can take any registered service: singleton, scoped (per circuit on Server,
per app on WebAssembly) or transient. `InsertTimeItem` above injects `TimeProvider`. `AddZauberRte` registers
`TimeProvider.System` if nothing else has, and you can swap it in tests.

Your own services work the same way:

```csharp
using ZauberCMS.RTE.Models;

public interface ISignatureService
{
    Task<string> GetCurrentUserNameAsync(CancellationToken cancellationToken);
}

public sealed class SignatureItem(ISignatureService signatures) : ToolbarItemBase
{
    public override string Id => "signature";

    public override string Label => "Insert signature";

    public override async Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(api);
        var name = await signatures.GetCurrentUserNameAsync(cancellationToken);
        await api.InsertHtmlAsync(HtmlBuilder.Element("p").Child(HtmlBuilder.Element("em").Text(name)).Build(), cancellationToken);
    }
}
```

Register the service as usual (for example `builder.Services.AddScoped<ISignatureService, SignatureService>()`), and
the editor resolves it whenever it creates the item.

## A toggle that tracks formatting

A toggle shows as pressed when its formatting wraps the selection. Set `IsToggle`, list the tags in `TrackedTags`,
and name the canonical one in `PrimaryTag`. The base class's `IsActive` then works without an override. Here is
`ZauberRTE.Web/Toolbar/HighlightItem.cs`:

```csharp
using ZauberCMS.RTE.Models;

namespace ZauberRTE.Web.Toolbar;

public sealed class HighlightItem : ToolbarItemBase
{
    public const string ItemId = "highlight";
    private const string Tag = "mark";

    public override string Id => ItemId;

    public override string Label => "Highlight";

    public override string? IconSvg => DemoIcons.Highlighter;

    public override string? Shortcut => "Control+Shift+H";

    public override bool IsToggle => true;

    public override IReadOnlyList<string> TrackedTags { get; } = [Tag];

    public override string? PrimaryTag => Tag;

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(api);
        return api.ToggleMarkAsync(Tag, cancellationToken);
    }
}
```

When several tags mean the same thing, list them all and pick one as primary. The built-in bold item tracks
`strong` and `b`, and its primary tag is `strong`. The editor then reports either tag as `strong` in
`EditorState.ActiveMarks`.

`mark` isn't in the default `HtmlPolicy`, so the demo adds it to the editor's settings. Without that, the policy
would strip the highlight. The full file is under [Custom layouts](#custom-layouts).

For state that doesn't come from a tag, override `IsActive` and `IsEnabled` and read the `EditorState`. The fields
include `BlockType`, `HeadingLevel`, `Alignment`, `InLink`, `InImage`, `InTable`, `HasSelection`, `CanUndo`,
`IsReadOnly` and `IsSourceView`:

```csharp
public override bool IsActive(EditorState state) => state.InTable;

public override bool IsEnabled(EditorState state) => base.IsEnabled(state) && state.HasSelection;
```

## Registering items and adding them to the toolbar

Two steps make an item appear:

1. **Register it** in `AddZauberRte` (`ZauberRTE.Web/Program.cs`):

   ```csharp
   using ZauberCMS.RTE.Services;
   using ZauberRTE.Web.Toolbar;

   builder.Services.AddZauberRte(options =>
   {
       options.AddToolbarItem<HighlightItem>();
       options.AddToolbarItem<InsertTimeItem>();
       options.AddToolbarItem<CalloutItem>();
       options.AddToolbarItem<InkColorDropdownItem>();
       options.AddToolbarItem<RemoveInkColorItem>();
       foreach (var color in InkColorItem.Palette)
       {
           options.AddToolbarItem(color);
       }
   });
   ```

2. **Place its id in a layout** (see [Custom layouts](#custom-layouts)). Registered items that aren't in the layout
   aren't shown, and their shortcuts are inactive.

The registration options are:

| Method | Use |
| --- | --- |
| `AddToolbarItem<TItem>()` | The normal way. Creates one instance per editor through DI. Trim-safe. |
| `AddToolbarItem(IToolbarItem item)` | Registers an existing instance, shared by every editor, so it must be stateless. Handy for items built from data, like the colour palette above. |
| `RemoveToolbarItem(string id)` | Removes whatever is registered under that id at that point, including a built-in. |
| `AddToolbarItemsFromAssembly(Assembly assembly)` | Registers every concrete `IToolbarItem` type in an assembly, including items meant to be registered as instances. Types DI can't construct are skipped and logged as errors. This uses reflection, so it's marked `[RequiresUnreferencedCode]` and isn't trim-safe; prefer explicit registration. |

Registrations apply in order, after the built-ins. A later registration with the same id replaces the earlier one.
`AddZauberRte` can be called several times (for example by plugins), and every configuration callback is applied.

For **WebAssembly or Auto** render modes, the item types must be in an assembly the client references, and the client
project's `Program.cs` must make the same `AddZauberRte(...)` registrations.

## A dropdown

A dropdown is an item with `Kind => ToolbarItemKind.Dropdown` and a list of `ChildItemIds`. Each child is an ordinary
registered item. The editor never calls the dropdown's own `ExecuteAsync`: clicking it opens the menu, and choosing an
entry runs that child. While a child is active, the trigger shows that child's label and icon, as the headings
dropdown does.

The parent (`ZauberRTE.Web/Toolbar/InkColorDropdownItem.cs`):

```csharp
using ZauberCMS.RTE.Models;

namespace ZauberRTE.Web.Toolbar;

public sealed class InkColorDropdownItem : ToolbarItemBase
{
    public const string ItemId = "inkColor";

    public override string Id => ItemId;

    public override string Label => "Ink colour";

    public override string? IconSvg => DemoIcons.Palette;

    public override ToolbarItemKind Kind => ToolbarItemKind.Dropdown;

    public override IReadOnlyList<string> ChildItemIds { get; } =
        [.. InkColorItem.Palette.Select(item => item.Id), RemoveInkColorItem.ItemId];

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken) => Task.CompletedTask;
}
```

The children: one class, instantiated per colour and registered as shared instances
(`ZauberRTE.Web/Toolbar/InkColorItem.cs`):

```csharp
using ZauberCMS.RTE.Models;

namespace ZauberRTE.Web.Toolbar;

public sealed class InkColorItem(string color, string label) : ToolbarItemBase
{
    private const string Tag = "span";

    public static IReadOnlyList<InkColorItem> Palette { get; } =
    [
        new("red", "Red"),
        new("green", "Green"),
        new("blue", "Blue"),
        new("purple", "Purple")
    ];

    public override string Id => $"{InkColorDropdownItem.ItemId}-{color}";

    public override string Label => label;

    public override string? IconSvg => DemoIcons.Swatch(CssClass);

    private string CssClass => $"ink-{color}";

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(api);
        return api.WrapSelectionAsync(Tag, new Dictionary<string, string> { ["class"] = CssClass }, cancellationToken);
    }
}
```

And a "reset" child (`ZauberRTE.Web/Toolbar/RemoveInkColorItem.cs`):

```csharp
using ZauberCMS.RTE.Models;

namespace ZauberRTE.Web.Toolbar;

public sealed class RemoveInkColorItem : ToolbarItemBase
{
    public const string ItemId = $"{InkColorDropdownItem.ItemId}-none";

    public override string Id => ItemId;

    public override string Label => "Default ink";

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(api);
        return api.UnwrapSelectionAsync("span", cancellationToken);
    }
}
```

Put only the dropdown's id (`inkColor`) in the layout. The demo uses `inkColor` with CSS classes as an extension
example; the built-in `textColor` and `backgroundColor` dropdowns already apply inline colours. Its children don't
need layout entries, but they must be registered. Children are looked up by id, so a child id can also point at a
built-in item (`headings` lists `paragraph`, `h1` to `h6` and `codeBlock`), and overriding that id changes the dropdown
too. For a toggle child, the menu entry is a `menuitemradio` that shows a check mark while the child is active.

## A slide-out panel

A panel is a Razor component that derives from `PanelBase`. An item opens it with `api.OpenPanelAsync<TPanel>()`. The
editor shows it in a slide-out dialog that is moved to `<body>`, so overflow or transforms on your layout don't clip
it. The dialog traps focus, and Escape or the close button dismiss it.

What `PanelBase` gives you:

| Member | Purpose |
| --- | --- |
| `Api` | The `IEditorApi` of the editor that opened the panel. |
| `Title` (abstract) | The panel heading, also the dialog's accessible name. You must override it. The heading updates whenever the panel re-renders; if you override `ShouldRender`, call the base method. |
| `ElementId(name)` | An element id unique to this panel instance, for `<label for>` and `aria-*` references. |
| `CloseAsync()` | Closes the panel and returns focus to the editor. |

The demo's callout item saves the selection, then opens the panel (`ZauberRTE.Web/Toolbar/CalloutItem.cs`):

```csharp
using ZauberCMS.RTE.Models;

namespace ZauberRTE.Web.Toolbar;

public sealed class CalloutItem : ToolbarItemBase
{
    public const string ItemId = "callout";

    public override string Id => ItemId;

    public override string Label => "Insert callout";

    public override string? IconSvg => DemoIcons.Callout;

    public override async Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(api);
        await api.SaveSelectionAsync(cancellationToken);
        await api.OpenPanelAsync<CalloutPanel>(cancellationToken: cancellationToken);
    }
}
```

The panel's markup (`ZauberRTE.Web/Toolbar/CalloutPanel.razor`). It needs `@using ZauberCMS.RTE.Models` in scope, for
example from `_Imports.razor`. A panel in a folder that the app's `_Imports.razor` doesn't cover also needs
`@using Microsoft.AspNetCore.Components.Web`: without it, `@bind` and `@onsubmit` are emitted as literal attributes, the
form does a native page submit, and the build reports no error.

```razor
@inherits PanelBase

<form class="rte-panel-content" @onsubmit="InsertAsync" @onsubmit:preventDefault>
    <div class="rte-form-group">
        <label for="@ElementId("text")" class="rte-label">Text</label>
        <input id="@ElementId("text")" type="text" class="rte-input" @bind="_text" @bind:event="oninput" />
    </div>

    <div class="rte-form-group">
        <label for="@ElementId("style")" class="rte-label">Style</label>
        <select id="@ElementId("style")" class="rte-select" @bind="_style">
            @foreach (var style in Styles)
            {
                <option @key="style" value="@style">@style</option>
            }
        </select>
    </div>

    <div class="rte-panel-actions">
        <button type="button" class="rte-btn rte-btn-secondary" @onclick="CancelAsync">Cancel</button>
        <button type="submit" class="rte-btn rte-btn-primary" disabled="@string.IsNullOrWhiteSpace(_text)">Insert</button>
    </div>
</form>
```

Its code-behind (`ZauberRTE.Web/Toolbar/CalloutPanel.razor.cs`):

```csharp
using ZauberCMS.RTE.Models;

namespace ZauberRTE.Web.Toolbar;

public sealed partial class CalloutPanel
{
    private static readonly string[] Styles = ["info", "tip", "warning"];

    private string _text = string.Empty;
    private string _style = Styles[0];

    public override string Title => "Insert callout";

    private async Task InsertAsync()
    {
        var callout = HtmlBuilder.Element("aside")
            .Class($"callout callout-{_style}")
            .Child(HtmlBuilder.Element("p").Text(_text.Trim()));

        await Api.RestoreSelectionAsync();
        await Api.InsertHtmlAsync(callout.Build());
        await CloseAsync();
    }

    private async Task CancelAsync()
    {
        await Api.ClearSavedSelectionAsync();
        await CloseAsync();
    }
}
```

### The selection

Typing in the panel's fields moves focus out of the editor. The editor remembers its last caret position and applies
commands there, so a plain `InsertHtmlAsync` already lands in the right place. Saving and restoring the selection
explicitly, as above, makes that intent clear and survives anything the panel does to the page's selection:

- `SaveSelectionAsync()` before opening the panel.
- `RestoreSelectionAsync()` just before inserting.
- `ClearSavedSelectionAsync()` when the user cancels. `CloseAsync()` doesn't clear it for you.

To edit existing content, read it first. Use `GetSelectionAsync()` for the selected text, `GetLinkAtCursorAsync()` or
`GetImageAtCursorAsync()` for the element at the cursor, and `SelectLinkAtCursorAsync()` or
`SelectImageAtCursorAsync()` so that inserting replaces it.

### Building HTML safely

Use `HtmlBuilder` rather than string concatenation. It encodes text and attribute values, so whatever the user types
can't break out of the markup, and it rejects invalid tag and attribute names.

| Method | Produces |
| --- | --- |
| `HtmlBuilder.Element("p")` | An element. |
| `HtmlBuilder.Link(href, text)` | `<a href="...">text</a>` |
| `HtmlBuilder.Image(src, alt)` | `<img src="..." alt="...">` |
| `HtmlBuilder.Figure(src, alt, caption)` | `<figure class="rte-figure">` with an image and `<figcaption>` |
| `.Attr(name, value)` | An attribute; a `null` value leaves it out. |
| `.Attrs(dictionary)` | Several attributes. |
| `.Class(className)` | The `class` attribute. |
| `.Text(text)` | Encoded text. |
| `.Child(builder)` | A nested element. |
| `.Build()` | The HTML string. |

`InsertHtmlAsync` still sanitises the result with the editor's `HtmlPolicy`. The demo's callout emits `aside`, so the
demo adds `aside` to the policy.

### Passing parameters to a panel

Declare `[Parameter]` properties on the panel, and pass them by name. The parameter values are applied when the panel
first renders:

`NotePanel.razor` starts with `@inherits PanelBase`, like the callout panel. Its code-behind, `NotePanel.razor.cs`,
must use the same namespace as the `.razor` file (by default, the project's root namespace plus the folder path):

```csharp
using Microsoft.AspNetCore.Components;

namespace MyApp.Toolbar;

public sealed partial class NotePanel
{
    [Parameter]
    public string? InitialText { get; set; }

    public override string Title => "Edit note";
}
```

The item that opens it, in its `ExecuteAsync`:

```csharp
public override async Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken)
{
    ArgumentNullException.ThrowIfNull(api);
    var selection = await api.GetSelectionAsync(cancellationToken);
    Dictionary<string, object?> parameters = new()
    {
        [nameof(NotePanel.InitialText)] = selection?.Text
    };
    await api.OpenPanelAsync<NotePanel>(parameters, cancellationToken);
}
```

### Opening a panel chosen at runtime

When the panel type is only known at runtime, for example from configuration, pass the `Type` instead of a type
argument. It must be a concrete component derived from `PanelBase`; anything else throws `ArgumentException`:

```csharp
Type panelType = options.UseRichNotes ? typeof(RichNotePanel) : typeof(NotePanel);
await api.OpenPanelAsync(panelType, parameters, cancellationToken);
```

The parameter is annotated with `[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]`, so the trimmer
keeps the panel's members when you pass a `typeof(...)`. A `Type` loaded by name (`Type.GetType("...")`) isn't
trim-safe.

### Panel styling

These classes give your panel the same look as the built-in ones, in both themes:

| Class | Element |
| --- | --- |
| `rte-panel-content` | The panel body: a `<form>` laid out as a column. |
| `rte-form-group` | One label and field pair. |
| `rte-label` | A field label. |
| `rte-input`, `rte-select` | Text inputs, textareas and selects. |
| `rte-checkbox-label` | A label wrapping a checkbox. |
| `rte-form-error` | A validation message (use with `role="alert"`). |
| `rte-panel-actions` | The button row at the bottom. |
| `rte-btn`, `rte-btn-primary`, `rte-btn-secondary` | Buttons (`rte-btn` alone is the neutral style). |

If the panel throws, an error message replaces its content, and the editor keeps working. The built-in panels
submit with Enter. To do the same in your panel, put its fields in a `<form @onsubmit="...">` and make the primary
button `type="submit"`.

## Overriding a built-in item

Register an item with a built-in's id. It replaces the built-in everywhere: in layouts, in shortcuts, and inside
dropdowns that list it. Use the `ToolbarItemIds` constants. For example, a bold button that emits `<b>` instead of
`<strong>`:

```csharp
using ZauberCMS.RTE.Models;

public sealed class BoldAsBItem : ToolbarItemBase
{
    public override string Id => ToolbarItemIds.Bold;

    public override string Label => "Bold";

    public override string? Shortcut => "Control+b";

    public override bool IsToggle => true;

    public override IReadOnlyList<string> TrackedTags { get; } = ["b", "strong"];

    public override string? PrimaryTag => "b";

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(api);
        return api.ToggleMarkAsync("b", cancellationToken);
    }
}
```

```csharp
builder.Services.AddZauberRte(options => options.AddToolbarItem<BoldAsBItem>());
```

The replacement carries only what you give it. A built-in's icon, shortcut, tracked tags and enabled rules aren't
inherited, so set everything the item needs. With no icon, the button shows its `Label` as text.

You can override every built-in id: `bold`, `italic`, `underline`, `strike`, `subscript`, `superscript`, `code`,
`textColor` and its swatches (`textColor-red` ... `textColor-default`), `backgroundColor` and its swatches
(`backgroundColor-yellow` ... `backgroundColor-none`), `paragraph`, `h1`–`h6`, `codeBlock`, `headings`,
`blockquote`, `ul`, `ol`, `indent`, `outdent`, `alignLeft`, `alignCenter`, `alignRight`, `justified`, `link`,
`unlink`, `image`, `table`, `horizontalRule`, `specialCharacters`, `clean-html`, `undo`, `redo`, `findReplace`,
`pastePlainText`, `viewSource`, `fullscreen` and `themeToggle`. To replace the link or image dialog, override `link`
or `image` with an item that opens your own panel. To use your own palette, override `textColor` or
`backgroundColor` with a dropdown that lists your own swatch items.

## Removing a built-in item

```csharp
builder.Services.AddZauberRte(options =>
{
    options.RemoveToolbarItem(ToolbarItemIds.Image);
    options.RemoveToolbarItem(ToolbarItemIds.ViewSource);
});
```

A removed id is skipped wherever a layout references it, and the editor logs it at `Debug` level, so a built-in layout
that references a removed item stays quiet. An id that was never registered (a typo, or a custom item you forgot to
register) is logged at `Warning` level instead. Removal applies to everything registered under that id before the
call, so if you remove an id and then add your own item with it, put `AddToolbarItem` after `RemoveToolbarItem`. To
hide an item from one editor only, leave it out of that editor's layout instead of removing it.

## Custom layouts

A layout lists groups, single items and separators in order. The demo's layout mixes built-ins and custom items, and
its settings allow the extra tags those items emit (`ZauberRTE.Web/Toolbar/DemoToolbar.cs`):

```csharp
using System.Collections.Frozen;
using ZauberCMS.RTE.Models;

namespace ZauberRTE.Web.Toolbar;

public static class DemoToolbar
{
    public static ToolbarLayout Layout { get; } = new(
        new ToolbarGroup(ToolbarItemIds.Bold, ToolbarItemIds.Italic, HighlightItem.ItemId, InkColorDropdownItem.ItemId),
        new ToolbarSeparator(),
        new ToolbarGroup(ToolbarItemIds.Headings, ToolbarItemIds.UnorderedList, CalloutItem.ItemId, InsertTimeItem.ItemId),
        new ToolbarSeparator(),
        new ToolbarGroup(ToolbarItemIds.Undo, ToolbarItemIds.Redo, ToolbarItemIds.ViewSource));

    public static EditorSettings Settings { get; } = new()
    {
        ToolbarLayout = Layout,
        HtmlPolicy = new()
        {
            AllowedTags = FrozenSet.Create(StringComparer.OrdinalIgnoreCase, [.. new HtmlPolicy().AllowedTags, "mark", "aside"])
        }
    };
}
```

Apply it per editor with the `Toolbar` parameter, or through `EditorSettings.ToolbarLayout`:

```razor
<ZauberRichTextEditor @bind-Value="_html" Settings="DemoToolbar.Settings" />
<ZauberRichTextEditor @bind-Value="_notes" Toolbar="DemoToolbar.Layout" />
```

- `ToolbarItemRef(id)` places a single item outside a group.
- Every entry takes an optional `CssClass`, for example `new ToolbarGroup(...) { CssClass = "my-group" }`.
- Layouts round-trip through JSON with `ToJson()` and `ToolbarLayout.FromJson(json)`. The README shows the
  [JSON format](README.md#layouts-as-json).
- Keep layouts in `static` properties, as above. The editor rebuilds its toolbar only when it receives a different
  layout instance.

## Letting users pick their toolbar

`IToolbarItemCatalog` (registered by `AddZauberRte`, scoped) lists every registered item, so a CMS can show a "pick
your toolbar buttons" screen and store the result as a layout. `GetItems()` returns one `ToolbarItemDescriptor` per
item: `Id`, `Label`, `Tooltip`, `IconSvg`, `IconCss`, `Shortcut`, `Kind`, `ChildItemIds` and `IsBuiltIn`.

- Built-in items come first in their usual order, then custom items by id.
- Removed items aren't listed. An override of a built-in id is listed once, with `IsBuiltIn = false`.
- Items registered by type are created once per scope to describe them, and disposed with the scope.
- Dropdown children (such as `h1` to `h6`) are listed too; use `Kind` and `ChildItemIds` to group them.

```razor
@inject IToolbarItemCatalog Catalog

@foreach (var item in Catalog.GetItems())
{
    <label>
        <input type="checkbox" checked="@_chosen.Contains(item.Id)" @onchange="() => Toggle(item.Id)" />
        @item.Label
    </label>
}

<ZauberRichTextEditor @bind-Value="_html" Toolbar="new ToolbarLayout(new ToolbarGroup([.. _chosen]))" />
```

Save the chosen layout with `ToJson()` and load it with `ToolbarLayout.FromJson(json)`. The demo's
`/toolbar-catalog` page (`ZauberRTE.Web/Components/Pages/ToolbarCatalog.razor`) is a working example.

## Icons

The editor doesn't ship an icon font. It gives you two options:

- **`IconSvg` (recommended).** Trusted inline SVG markup, rendered inside `<span class="rte-icon" aria-hidden="true">`.
  Use `stroke="currentColor"` or `fill="currentColor"`, so the icon follows the theme and the active state. Sizing comes
  from `--rte-icon-size`. The demo keeps its icons as constants in `ZauberRTE.Web/Toolbar/DemoIcons.cs`. The markup is
  rendered as-is, so never build it from user input.

  ```csharp
  public override string? IconSvg =>
      """<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true" focusable="false"><circle cx="12" cy="12" r="9"/></svg>""";
  ```

- **`IconCss`.** CSS classes for an icon font you load yourself, rendered as `<i class="...">` exactly as given. For
  Font Awesome 6, add its stylesheet to your host page and return the full class list:

  ```csharp
  public override string? IconCss => "fa-solid fa-star";
  ```

When both are set, `IconSvg` wins. When neither is set, the button shows `Label` as text, which suits short labels
like "H1".

## Checklist

- [ ] Derive from `ToolbarItemBase`; set a unique `Id` (expose it as a `const`) and a meaningful `Label`.
- [ ] Give it an icon (`IconSvg` or `IconCss`), or a short `Label` that reads well as text.
- [ ] For toggles: `IsToggle`, `TrackedTags` and `PrimaryTag` (or override `IsActive`).
- [ ] Register it with `options.AddToolbarItem<T>()`, and in the client project too for WebAssembly or Auto.
- [ ] Add its id to the layout.
- [ ] Allow any new tags or attributes it emits in `HtmlPolicy`.
- [ ] For panels: override `Title`, use `ElementId` for label ids, build markup with `HtmlBuilder`, clear a saved
      selection on cancel, and finish with `CloseAsync()`.
- [ ] Check it in both themes and with two editors on one page.

## The demo app

The `ZauberRTE.Web` project (run it with `dotnet run --project ZauberRTE.Web`) has a page for each scenario:

| Page | File | Shows |
| --- | --- | --- |
| `/` | `Components/Pages/Home.razor` | The full toolbar, `OnChange`/`OnStateChanged`, and `EditForm` validation (`Components/Demo/ArticleForm.razor`). |
| `/custom-items` | `Components/Pages/CustomItems.razor` | Every item in this guide (`Toolbar/`). |
| `/multiple-editors` | `Components/Pages/MultipleEditors.razor` | Three independent editors with different layouts. |
| `/read-only` | `Components/Pages/ReadOnlyDemo.razor` | `ReadOnly` switched at runtime. |
| `/themes` | `Components/Pages/Themes.razor` | Light, dark and auto, and `@bind-Theme`. |
| `/large-content` | `Components/Pages/LargeContent.razor` | A 200 KB document with the default SignalR limits. |
| `/webassembly`, `/auto` | `ZauberRTE.Web.Client/Pages/` | WebAssembly and Auto render modes. |
