# Blazor Rich Text Editor

[![NuGet](https://img.shields.io/nuget/v/ZauberCMS.RTE.svg)](https://www.nuget.org/packages/ZauberCMS.RTE/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/)

**This project is in Beta release, so I am still testing and fixing any bugs. Please report any issues you find.**

A modern, extensible rich-text editor component for Blazor applications. Built with performance, accessibility, and developer experience in mind.

## Features

- 🚀 **Blazor Native**: Fully built for Blazor Server and WebAssembly
- 🎨 **Highly Configurable**: Extensive customization options for toolbar, capabilities, and behavior
- 🔒 **Secure by Default**: Built-in HTML sanitization with configurable policies
- ⌨️ **Keyboard Shortcuts**: Full keyboard support with customizable shortcuts
- 🖼️ **Media Support**: Image upload, drag-and-drop, and embed support
- 📱 **Responsive**: Mobile-friendly with touch support
- 🎯 **Accessible**: ARIA compliant with screen reader support
- 🧩 **Extensible**: Plugin architecture for custom toolbar items and functionality
- 🎨 **Themes**: Light, dark theme support
- 📝 **Source View**: Toggle between rich text and HTML source editing

## Installation

### NuGet Package

```bash
dotnet add package ZauberCMS.RTE
```

### Manual Installation

1. Clone or download the ZauberCMS.RTE project
2. Reference the `ZauberCMS.RTE.csproj` in your Blazor project
3. Ensure static web assets are served correctly

## Quick Start

### 1. Register Services

In your `Program.cs`:

```csharp
using ZauberCMS.RTE.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Zauber RTE services
builder.Services.AddZauberRte();

// IMPORTANT: If using base64 image uploads (AllowBase64ImageUpload = true),
// increase SignalR message size limit to support large images
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 1 * 1024 * 1024; // 1 MB
});

var app = builder.Build();
```

**Note about SignalR Configuration:**
- Base64-encoded images can be large (a 50KB image becomes ~67KB when base64-encoded)
- The default SignalR message size limit is 32KB
- If you enable base64 image uploads, you must increase this limit
- Set `AllowBase64ImageUpload = false` in `ImageConstraints` if you only want URL-based images

### 2. Add to Your Page

```razor
@using ZauberCMS.RTE.Components
@using ZauberCMS.RTE.Models

<ZauberRichTextEditor @bind-Value="editorContent"
                      Settings="editorSettings"
                      OnChange="HandleContentChange" />

@code {
    private string editorContent = "";
    private EditorSettings editorSettings = EditorSettings.CmsDefault();

    private void HandleContentChange(EditorChangeArgs args)
    {
        // Handle content changes
        Console.WriteLine($"Content changed: {args.Html}");
    }
}
```

### 3. Include Required Assets

Add the required CSS and JavaScript files to your `App.razor` (or `_Host.cshtml` for Blazor Server):

```html
<head>
    <!-- Other head content -->
    
    <!-- Zauber RTE Styles (required) -->
    <link rel="stylesheet" href="_content/ZauberCMS.RTE/css/fontawesome.min.css" />
    <link rel="stylesheet" href="_content/ZauberCMS.RTE/css/zauber-editor.css" />
</head>

<body>
    <!-- Your app content -->
    <script src="_framework/blazor.web.js"></script>
    
    <!-- Zauber RTE Scripts (required) -->
    <script src="_content/ZauberCMS.RTE/js/lib/purify.min.js"></script>
    <script src="_content/ZauberCMS.RTE/js/zauber-rte.js"></script>
</body>
```

**Important Notes:**
- The Zauber RTE scripts must be loaded after the Blazor framework script
- Font Awesome is bundled for the toolbar icons
- All editor, panel, and component styles are consolidated in `zauber-editor.css`

## Configuration

### Basic Settings

```csharp
var settings = new EditorSettings
{
    // Configure capabilities
    Capabilities = new EditorCapabilities
    {
        TextFormatting = true,
        InteractiveElements = true,
        EmbedsAndMedia = true,
        Subscript = true,
        Superscript = true,
        TextAlign = true,
        Underline = true,
        Strike = true,
        ClearFormatting = true
    },

    // Set dimensions
    Dimensions = new EditorDimensions
    {
        Width = "100%",
        Height = "300px",
        MinHeight = 100,
        MaxHeight = 600
    },

    // Configure toolbar layout
    ToolbarLayout = ToolbarLayout.Default,

    // Image constraints
    ImageConstraints = new ImageConstraints
    {
        MaxWidth = 800,
        MaxHeight = 600,
        MaintainAspectRatio = true,
        AllowBase64ImageUpload = false, // Set to true to enable file uploads as base64 (requires SignalR config)
        AllowedImageTypes = new() { ".jpg", ".jpeg", ".gif", ".png" }
    }
};
```

### Predefined Configurations

```csharp
// CMS-ready configuration with all features
var cmsSettings = EditorSettings.CmsDefault();

// Minimal configuration for basic text editing
var minimalSettings = EditorSettings.Minimal();
```

### HTML Sanitization

```csharp
var settings = new EditorSettings
{
    HtmlPolicy = new HtmlPolicy
    {
        AllowedTags = new HashSet<string> { "p", "strong", "em", "a", "img" },
        AllowedAttributes = new Dictionary<string, HashSet<string>>
        {
            ["a"] = new() { "href", "target" },
            ["img"] = new() { "src", "alt", "width", "height" }
        },
        AllowDataUrls = false,
        AllowExternalImages = true
    }
};
```

### Keyboard Shortcuts

Keyboard shortcuts are defined directly on toolbar items, making them fully extensible:

```csharp
public class MyCustomItem : ToolbarItemBase
{
    public override string Id => "myCustom";
    public override string Label => "My Custom Action";
    public override string IconCss => "fa-star";
    public override string Shortcut => "Control+Shift+m";  // Define your shortcut here!
    
    public override Task ExecuteAsync(EditorApi api)
    {
        // Your custom logic
        return Task.CompletedTask;
    }
}
```

Built-in shortcuts:
- **Bold**: Ctrl+B
- **Italic**: Ctrl+I  
- **Underline**: Ctrl+U
- **Link**: Ctrl+K
- **Unordered List**: Ctrl+Shift+8
- **Ordered List**: Ctrl+Shift+7
- **Undo**: Ctrl+Z
- **Redo**: Ctrl+Y

## Component API

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Value` | `string?` | The HTML content of the editor |
| `Settings` | `EditorSettings` | Configuration settings |
| `ToolbarLayout` | `ToolbarLayout` | Toolbar layout configuration |
| `Theme` | `Theme` | Visual theme (Light, Dark, Auto) |
| `ReadOnly` | `bool` | Whether the editor is read-only |

### Events

| Event | Type | Description |
|-------|------|-------------|
| `ValueChanged` | `EventCallback<string?>` | Fired when content changes |
| `OnChange` | `EventCallback<EditorChangeArgs>` | Detailed change information |
| `OnKeyDown` | `EventCallback<ZauberKeyboardEventArgs>` | Keyboard events |
| `OnSelectionChanged` | `EventCallback<SelectionChangedArgs>` | Selection changes |
| `OnPaste` | `EventCallback<PasteArgs>` | Paste events |
| `OnImageResized` | `EventCallback<ImageResizedArgs>` | Image resize events |
| `OnCommandExecuted` | `EventCallback<CommandExecutedArgs>` | Toolbar command execution |

## Toolbar Customization

### Default Toolbar Items

The editor comes with comprehensive toolbar items:

**Text Formatting**: Bold, Italic, Underline, Strikethrough, Code, Subscript, Superscript

**Headings**: H1-H6 (available as individual buttons or dropdown)

**Lists**: Unordered List, Ordered List

**Alignment**: Left, Center, Right, Justified

**Insert**: Link, Image

**Utilities**: Clear Formatting, Undo, Redo, View Source, Theme Toggle

### Toolbar Item Types

The editor supports three types of toolbar items:

1. **Button** - Standard clickable button (Bold, Italic, etc.)
2. **Dropdown** - Menu with child items (e.g., Headings dropdown with H1-H6)
3. **Separator** - Visual divider between toolbar groups

### Flexible Toolbar Layouts

The new flexible layout system allows complete customization with blocks, separators, and individual items:

```csharp
using ZauberCMS.RTE.Models;

// Use predefined layouts
var settings = new EditorSettings
{
    ToolbarLayout = ToolbarLayout.Simple,  // Simple: basic formatting + alignment
    // ToolbarLayout = ToolbarLayout.Full,    // Full: all features logically grouped
    // ToolbarLayout = ToolbarLayout.Minimal, // Minimal: bold, italic, link only
    // ToolbarLayout = ToolbarLayout.Cms,     // CMS-focused layout
};

// Create custom layout with blocks and separators
var customLayout = ToolbarLayout.FromItems(
    new ToolbarBlock("bold", "italic", "underline"),  // Block of buttons
    new ToolbarSeparator(),                            // Visual separator
    new ToolbarBlock("headings"),                      // Headings dropdown (H1-H6)
    new ToolbarSeparator(),
    new ToolbarBlock("link", "image")
);

// Advanced: individual items with custom CSS (use IEnumerable for cssClass parameter)
var advancedLayout = ToolbarLayout.FromItems(
    new ToolbarBlock("my-custom-class", new[] { "undo", "redo" }),
    new ToolbarSeparator("my-separator-style"),
    new ToolbarItemReference("link", "my-link-style"),
    new ToolbarBlock("bold", "italic")
);
```

### Headings Dropdown

The new headings dropdown consolidates H1-H6 into a single dropdown menu:

```csharp
// Use the headings dropdown (recommended)
ToolbarLayout = ToolbarLayout.FromItems(
    new ToolbarBlock("bold", "italic"),
    new ToolbarSeparator(),
    new ToolbarBlock("headings")  // Dropdown with H1-H6 and reset option
);

// Or use individual heading buttons
ToolbarLayout = ToolbarLayout.FromItems(
    new ToolbarBlock("bold", "italic"),
    new ToolbarSeparator(),
    new ToolbarBlock("h1", "h2", "h3", "h4", "h5", "h6")  // Individual buttons
);
```

The headings dropdown automatically:
- Shows the currently active heading (H1, H2, etc.)
- Highlights the selected heading in the dropdown menu
- Displays heading previews with appropriate font sizes
- Closes when clicking outside the dropdown

### Layout Examples

```csharp
// Blog editor with all features
public static ToolbarLayout BlogLayout => ToolbarLayout.FromItems(
    new ToolbarBlock("undo", "redo", "viewSource"),
    new ToolbarSeparator(),
    new ToolbarBlock("headings", "blockquote"),
    new ToolbarSeparator(),
    new ToolbarBlock("bold", "italic", "underline", "strike", "code"),
    new ToolbarSeparator(),
    new ToolbarBlock("ul", "ol"),
    new ToolbarSeparator(),
    new ToolbarBlock("link", "unlink", "image"),
    new ToolbarSeparator(),
    new ToolbarBlock("alignLeft", "alignCenter", "alignRight")
);

// Simple comment editor
public static ToolbarLayout CommentLayout => ToolbarLayout.FromItems(
    new ToolbarBlock("bold", "italic", "link")
);

// Using helper methods
var layout = ToolbarLayout.FromItems(
    ToolbarLayoutExtensions.Block("bold", "italic"),
    ToolbarLayoutExtensions.Separator(),
    ToolbarLayoutExtensions.Item("headings")
);
```

### Custom Toolbar Items

Create custom toolbar items by implementing `IToolbarItem`. See the **Extension Guide** section below for detailed examples.

### Creating Custom Dropdowns

You can create your own dropdown toolbar items:

```csharp
public class MyDropdownItem : ToolbarItemBase
{
    public override string Id => "myDropdown";
    public override string Label => "My Options";
    public override string IconCss => "fa-list";
    public override ToolbarPlacement Placement => ToolbarPlacement.Custom;
    public override ToolbarItemType ItemType => ToolbarItemType.Dropdown;
    
    public override List<IToolbarItem> ChildItems =>
    [
        new MyOption1Item(),
        new MyOption2Item(),
        new MyOption3Item()
    ];
    
    public override Task ExecuteAsync(IEditorApi api) => Task.CompletedTask;
}
```

## Advanced Usage

### Programmatic Control

```csharp
@inject EditorApi EditorApi

<ZauberRichTextEditor @ref="editor" />

@code {
    private ZauberRichTextEditor? editor;

    private async Task InsertCustomContent()
    {
        if (editor?.EditorApi != null)
        {
            await editor.EditorApi.InsertHtmlAsync("<p>Custom content</p>");
            await editor.EditorApi.SetSelectionAsync("end");
        }
    }
}
```

### Image Handling

```csharp
private async Task HandleImageUpload(ImageUploadArgs args)
{
    // Handle image upload
    var imageUrl = await UploadImageToServer(args.File);

    // Insert the image
    await args.InsertImageAsync(imageUrl, args.AltText);
}
```

### Custom Panels

Create custom slide-out panels by inheriting from `PanelBase`:

```razor
<!-- CustomPanel.razor -->
@using ZauberCMS.RTE.Models
@inherits PanelBase

<div class="rte-panel-content">
    <h3 class="rte-panel-title">Custom Panel</h3>
    
    <div class="rte-form-group">
        <label class="rte-label">Enter Value</label>
        <input @bind="customValue" class="rte-input" />
    </div>

    <div class="rte-panel-actions">
        <button type="button" class="rte-btn rte-btn-secondary" @onclick="CloseAsync">
            Cancel
        </button>
        <button type="button" class="rte-btn rte-btn-primary" @onclick="ApplyAsync">
            Apply
        </button>
    </div>
</div>

@code {
    private string customValue = "";

    private async Task ApplyAsync()
    {
        if (Api == null) return;
        
        // Use HtmlBuilder for clean HTML generation
        var html = HtmlBuilder.Element("div")
            .Class("custom-content")
            .Text(customValue)
            .Build();
            
        await Api.InsertHtmlAsync(html);
        await CloseAsync();
    }
}
```

## Styling

### CSS Variables

Customize appearance using CSS variables:

```css
.zauber-rte {
    --rte-primary-color: #007acc;
    --rte-border-color: #e1e5e9;
    --rte-background-color: #ffffff;
    --rte-text-color: #333333;
    --rte-toolbar-height: 40px;
    --rte-font-family: 'Segoe UI', sans-serif;
}
```

### Themes

```razor
<!-- Light theme -->
<ZauberRichTextEditor Theme="Theme.Light" />

<!-- Dark theme -->
<ZauberRichTextEditor Theme="Theme.Dark" />

<!-- Auto theme (follows system preference) -->
<ZauberRichTextEditor Theme="Theme.Auto" />
```

## Security Considerations

- HTML content is sanitized by default using a configurable policy
- Data URLs can be disabled for images
- External images can be restricted
- XSS protection through DOMPurify integration
- Content Security Policy (CSP) friendly

## Browser Support

- Chrome 80+
- Firefox 75+
- Safari 13+
- Edge 80+

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

MIT License - see LICENSE file for details.

## Extensibility

Create custom toolbar buttons and panels in minutes. Zauber RTE provides a simple, powerful extension system.

```csharp
// Create a custom toolbar item
public class EmojiItem : ToolbarItemBase
{
    public override string Id => "emoji";
    public override string Label => "Emoji";
    public override string IconCss => "fa-smile";
    
    public override async Task ExecuteAsync(EditorApi api)
    {
        await api.InsertHtmlAsync("😀");
    }
}
```

### Override Built-in Items

Customize any built-in toolbar item by creating your own with the same ID:

```csharp
// Override the built-in Bold button with custom behavior
public class BoldItem : ToolbarItemBase
{
    public override string Id => "bold";  // Same ID = replaces default
    public override string Label => "Bold";
    public override string IconCss => "fa-bold";
    public override string Shortcut => "Control+b";
    
    public override async Task ExecuteAsync(EditorApi api)
    {
        // Your custom bold logic here
        await api.ToggleMarkAsync("strong");
        await api.ShowToastAsync("Bold applied!");
    }
}
```

By default, `AllowOverrides = true` lets your items replace built-in ones. Set to `false` to prevent overrides:

```csharp
builder.Services.AddZauberRte(options =>
{
    options.Assemblies.Add(typeof(Program).Assembly);
    options.AllowOverrides = false;  // Your items won't replace built-ins
});
```

**[→ Extension Guide](QUICK_START_EXTENDING.md)** - Full documentation with copy-paste templates for toolbar items and custom panels

