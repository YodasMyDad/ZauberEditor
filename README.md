# Zauber Rich Text Editor

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
- 🎨 **Themes**: Light, dark, and auto theme support
- 📝 **Source View**: Toggle between rich text and HTML source editing

## Installation

### NuGet Package

```bash
dotnet add package Zauber.RTE
```

### Manual Installation

1. Clone or download the Zauber.RTE project
2. Reference the `Zauber.RTE.csproj` in your Blazor project
3. Ensure static web assets are served correctly

## Quick Start

### 1. Register Services

In your `Program.cs`:

```csharp
using Zauber.RTE.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Zauber RTE services
builder.Services.AddZauberRte();

var app = builder.Build();
```

### 2. Add to Your Page

```razor
@using Zauber.RTE.Components
@using Zauber.RTE.Models

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

### 3. Add Styles

Include the CSS in your `_Host.cshtml` or `index.html`:

```html
<link href="_content/Zauber.RTE/zauber-rte.css" rel="stylesheet" />
```

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
        MaintainAspectRatio = true
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

```csharp
var settings = new EditorSettings
{
    Shortcuts = new ShortcutMap
    {
        Bold = "Control+b",
        Italic = "Control+i",
        Underline = "Control+u",
        Link = "Control+k",
        UnorderedList = "Control+Shift+8",
        OrderedList = "Control+Shift+7",
        Save = "Control+s"
    }
};
```

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

**Headings**: H1, H2, H3, Paragraph

**Lists**: Unordered List, Ordered List

**Alignment**: Left, Center, Right, Justified

**Insert**: Link, Image, Table

**Utilities**: Clear Formatting, Undo, Redo, View Source, Settings

### Custom Toolbar Items

Create custom toolbar items by implementing `IToolbarItem`:

```csharp
public class CustomButtonItem : ToolbarItemBase
{
    public override string Id => "custom-button";
    public override string Label => "Custom Button";
    public override string SvgPath => "M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5";
    public override ToolbarPlacement Placement => ToolbarPlacement.Custom;

    public override Task ExecuteAsync(EditorApi api)
    {
        // Your custom logic here
        await api.InsertHtmlAsync("<div>Custom content</div>");
        return Task.CompletedTask;
    }
}
```

Register custom items during service configuration:

```csharp
builder.Services.AddZauberRte(options =>
{
    // Register custom toolbar items
    options.Assemblies.Add(typeof(CustomButtonItem).Assembly);
});
```

### Toolbar Layouts

Configure toolbar layout:

```csharp
// Use predefined layouts
ToolbarLayout = ToolbarLayout.Default;
ToolbarLayout = ToolbarLayout.Compact;
ToolbarLayout = ToolbarLayout.Minimal;

// Or create custom layout
var customLayout = new ToolbarLayout
{
    ShowInlineFormatting = true,
    ShowBlockFormatting = true,
    ShowInsertItems = true,
    // ... configure other options
};
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

Create custom slide-out panels:

```razor
<!-- CustomPanel.razor -->
@inherits PanelBase

<div class="custom-panel">
    <h3>Custom Panel</h3>
    <input @bind="customValue" />
    <button @onclick="ApplyChanges">Apply</button>
</div>

@code {
    private string customValue = "";

    private async Task ApplyChanges()
    {
        await Api.InsertHtmlAsync($"<div>{customValue}</div>");
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

## Roadmap

- [ ] Table editing improvements
- [ ] Collaborative editing support
- [ ] Plugin marketplace
- [ ] Voice-to-text integration
- [ ] Advanced image editing
- [ ] Export to PDF/Word
- [ ] Version history
- [ ] Real-time collaboration

## Support

- 📖 [Documentation](https://github.com/zauber-rte/zauber-rte/wiki)
- 💬 [Discussions](https://github.com/zauber-rte/zauber-rte/discussions)
- 🐛 [Issue Tracker](https://github.com/zauber-rte/zauber-rte/issues)
- 📧 [Email Support](mailto:support@zauber-rte.com)
