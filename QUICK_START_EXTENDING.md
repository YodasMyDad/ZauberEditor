# Quick Start: Extending Zauber RTE

## 🚀 Creating Your First Custom Toolbar Item

### 1. Copy the Template
```bash
# Copy from Examples folder to your project
Examples/CustomToolbarItemTemplate.cs → YourProject/CustomToolbarItems/
```

### 2. Customize It
```csharp
public class EmojiItem : ToolbarItemBase
{
    public override string Id => "emoji";
    public override string Label => "Emoji";
    public override string IconCss => "fa-smile";
    public override ToolbarPlacement Placement => ToolbarPlacement.Insert;

    public override async Task ExecuteAsync(EditorApi api)
    {
        await api.InsertHtmlAsync("😀");
    }
}
```

### 3. Register It
```csharp
// Program.cs - will automatically scan for toolbar items
builder.Services.AddZauberRte(typeof(Program).Assembly);

// Or scan multiple assemblies
builder.Services.AddZauberRte(typeof(Program).Assembly, typeof(MyPlugin.Class).Assembly);
```

### 4. Add to Toolbar
```csharp
var settings = new EditorSettings
{
    ToolbarLayout = ToolbarLayout.FromItems(
        new ToolbarBlock("bold", "italic", "emoji") // Add your item!
    )
};
```

**Done! Your custom button is now in the editor.** ✨

---

## 🔄 Overriding Built-in Items

Customize any built-in toolbar button by using the same ID:

```csharp
// This replaces the default BoldItem
public class BoldItem : ToolbarItemBase
{
    public override string Id => "bold";  // ← Same ID replaces default!
    public override string Label => "My Bold";
    public override string IconCss => "fa-bold";
    public override string Shortcut => "Control+b";
    public override ToolbarPlacement Placement => ToolbarPlacement.Inline;
    public override bool IsToggle => true;
    
    public override bool IsActive(EditorState state) 
        => state.ActiveMarks.Contains("strong");
    
    public override async Task ExecuteAsync(EditorApi api)
    {
        await api.ToggleMarkAsync("strong");
        await api.ShowToastAsync("Custom bold applied!");
    }
}
```

**Overridable Built-in IDs:**
`bold`, `italic`, `underline`, `strike`, `code`, `subscript`, `superscript`, `h1`, `h2`, `h3`, `h4`, `h5`, `h6`, `headings`, `blockquote`, `ul`, `ol`, `alignLeft`, `alignCenter`, `alignRight`, `justified`, `link`, `unlink`, `image`, `clean-html`, `undo`, `redo`, `viewSource`, `themeToggle`

By default, overrides are enabled. To disable:
```csharp
builder.Services.AddZauberRte(options =>
{
    options.AllowOverrides = false;  // Prevent replacements
}, typeof(Program).Assembly);
```

---

## 💉 Dependency Injection in Toolbar Items

Toolbar items support full dependency injection. Just add constructor parameters:

```csharp
public class DatabaseSearchItem : ToolbarItemBase
{
    private readonly IMyDatabaseService _database;
    private readonly ILogger<DatabaseSearchItem> _logger;
    
    // Dependencies injected automatically
    public DatabaseSearchItem(
        IMyDatabaseService database, 
        ILogger<DatabaseSearchItem> logger)
    {
        _database = database;
        _logger = logger;
    }
    
    public override string Id => "db-search";
    public override string Label => "Search Database";
    public override string IconCss => "fa-database";
    public override ToolbarPlacement Placement => ToolbarPlacement.Custom;
    
    public override async Task ExecuteAsync(EditorApi api)
    {
        try
        {
            var results = await _database.SearchAsync(api.GetSelectedText());
            // ... insert results into editor
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database search failed");
            await api.ShowToastAsync("Search failed", ToastType.Error);
        }
    }
}
```

**Note**: Toolbar items are instantiated once during startup using `ActivatorUtilities.CreateInstance`, so all registered services are available for injection.

---

## 📝 Creating a Custom Panel

### 1. Copy the Template
```bash
Examples/CustomPanelTemplate.razor → YourProject/Components/Panels/
```

### 2. Create Toolbar Item That Opens It
```csharp
public class MyCustomItem : ToolbarItemBase
{
    public override string Id => "my-custom";
    public override string Label => "Custom";
    public override string IconCss => "fa-magic";
    public override ToolbarPlacement Placement => ToolbarPlacement.Insert;

    public override Task ExecuteAsync(EditorApi api) 
        => api.OpenPanelAsync(typeof(MyCustomPanel));
}
```

### 3. Customize Panel
```razor
@inherits PanelBase

<div class="rte-panel-content">
    <h3 class="rte-panel-title">My Panel</h3>
    
    <div class="rte-form-group">
        <input @bind="_value" class="rte-input" />
    </div>

    <div class="rte-panel-actions">
        <button class="rte-btn rte-btn-secondary" @onclick="CloseAsync">
            Cancel
        </button>
        <button class="rte-btn rte-btn-primary" @onclick="ApplyAsync">
            Apply
        </button>
    </div>
</div>

@code {
    private string _value = "";

    private async Task ApplyAsync()
    {
        var html = HtmlBuilder.Element("span").Text(_value).Build();
        await Api.InsertHtmlAsync(html);
        await CloseAsync();
    }
}
```

**Done! Your custom panel is ready.** ✨

---

## 🛠️ Common Helper Methods

### EditorApi Methods
```csharp
// Insert content
await api.InsertHtmlAsync("<p>Text</p>");
await api.InsertTextAsync("Plain text");

// Selection
var selection = await api.GetSelectionAsync();
await api.ReplaceSelectionAsync("<b>Bold</b>");

// Formatting
await api.ToggleMarkAsync("strong");
await api.SetBlockTypeAsync("h2", null);
await api.ClearFormattingAsync();

// Panel operations
await api.OpenPanelAsync(typeof(MyPanel));
await api.ClosePanelAsync();

// Selection management (for panels)
await api.SaveSelectionRangeAsync();
await api.RestoreSelectionRangeAsync();
await api.ClearSavedSelectionRangeAsync();

// Notifications
await api.ShowToastAsync("Success!", ToastType.Success);

// Content
var html = api.GetHtml();
await api.SetHtmlAsync("<p>New content</p>");
```

### HtmlBuilder Methods
```csharp
// Simple element
HtmlBuilder.Element("p").Text("Hello").Build()

// With attributes
HtmlBuilder.Element("div")
    .Class("my-class")
    .Id("my-id")
    .Style("color: red;")
    .Build()

// Helpers
HtmlBuilder.Link("url", "text").Build()
HtmlBuilder.Image("src", "alt").Build()
HtmlBuilder.Figure("src", "alt", "caption").Build()

// Nested
HtmlBuilder.Element("div")
    .Child(HtmlBuilder.Element("h2").Text("Title"))
    .Child(HtmlBuilder.Element("p").Text("Body"))
    .Build()
```

---

## 📋 Toolbar Item Checklist

- [ ] Unique `Id` (used in layout configuration)
- [ ] Descriptive `Label` (for accessibility)
- [ ] FontAwesome `IconCss` (e.g., "fa-star")
- [ ] Appropriate `Placement` (Inline, Block, Insert, Media, Custom)
- [ ] Implement `ExecuteAsync()` method
- [ ] Handle errors with try-catch
- [ ] Use `HtmlBuilder` for HTML generation
- [ ] Test with and without text selection

---

## 📋 Panel Checklist

- [ ] Inherits from `PanelBase`
- [ ] Save selection in `OnAfterRenderAsync()`
- [ ] Use standard CSS classes (`rte-panel-content`, `rte-form-group`, etc.)
- [ ] Restore selection before inserting content
- [ ] Clear saved selection after inserting
- [ ] Call `CloseAsync()` when done
- [ ] Handle errors and show toasts
- [ ] Disable submit button when invalid

---

## 🎨 CSS Classes Available

```html
<!-- Panel structure -->
<div class="rte-panel-content">
    <h3 class="rte-panel-title">Title</h3>
    
    <!-- Form controls -->
    <div class="rte-form-group">
        <label class="rte-label">Label</label>
        <input class="rte-input" />
    </div>
    
    <!-- Checkbox -->
    <label class="rte-checkbox-label">
        <input type="checkbox" />
        <span class="rte-checkbox-mark"></span>
        Label
    </label>
    
    <!-- Actions -->
    <div class="rte-panel-actions">
        <button class="rte-btn rte-btn-secondary">Cancel</button>
        <button class="rte-btn rte-btn-primary">Apply</button>
    </div>
</div>
```

---

## 💡 Pro Tips

1. **Always use `HtmlBuilder`** for clean, safe HTML generation
2. **Always save selection** when opening panels
3. **Use try-catch** and show user-friendly error messages
4. **Test edge cases**: no selection, empty selection, multi-line
5. **Copy existing panels** as reference for complex scenarios
6. **Use XML comments** for IntelliSense in your team

---

## 📚 More Resources

- Full guide: See `README.md` → Extension Guide section
- Templates: `Examples/` folder
- Reference implementations: All panels in `Components/Panels/`
- API docs: XML comments in `EditorApi.cs`

---

**Happy Extending!** 🎉

Need help? Check the comprehensive Extension Guide in README.md

