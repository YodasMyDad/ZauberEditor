using System.ComponentModel;

namespace Zauber.RTE.Models;

/// <summary>
/// Configuration settings for the Zauber Rich Text Editor
/// </summary>
public class EditorSettings
{
    /// <summary>
    /// Capabilities that can be enabled/disabled
    /// </summary>
    public EditorCapabilities Capabilities { get; set; } = new();

    /// <summary>
    /// Toolbar layout configuration
    /// </summary>
    public ToolbarLayout ToolbarLayout { get; set; } = ToolbarLayout.Default;

    /// <summary>
    /// Editor dimensions
    /// </summary>
    public EditorDimensions Dimensions { get; set; } = new();

    /// <summary>
    /// Image constraints
    /// </summary>
    public ImageConstraints ImageConstraints { get; set; } = new();

    /// <summary>
    /// HTML sanitization policy
    /// </summary>
    public HtmlPolicy HtmlPolicy { get; set; } = new();

    /// <summary>
    /// Keyboard shortcuts configuration
    /// </summary>
    public ShortcutMap Shortcuts { get; set; } = new();

    /// <summary>
    /// Creates a default settings configuration suitable for CMS usage
    /// </summary>
    public static EditorSettings CmsDefault() => new()
    {
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
        ImageConstraints = new ImageConstraints
        {
            MaxWidth = 800,
            MaxHeight = 600
        }
    };

    /// <summary>
    /// Creates a minimal settings configuration
    /// </summary>
    public static EditorSettings Minimal() => new()
    {
        Capabilities = new EditorCapabilities
        {
            TextFormatting = true
        }
    };
}

/// <summary>
/// Capabilities that can be enabled/disabled in the editor
/// </summary>
public class EditorCapabilities
{
    /// <summary>
    /// Rich text formatting (bold, italic, etc.)
    /// </summary>
    public bool TextFormatting { get; set; } = true;

    /// <summary>
    /// Interactive elements (links, tables)
    /// </summary>
    public bool InteractiveElements { get; set; } = true;

    /// <summary>
    /// Embeds and media (images, videos, etc.)
    /// </summary>
    public bool EmbedsAndMedia { get; set; } = true;

    /// <summary>
    /// Subscript formatting
    /// </summary>
    public bool Subscript { get; set; }

    /// <summary>
    /// Superscript formatting
    /// </summary>
    public bool Superscript { get; set; }

    /// <summary>
    /// Text alignment controls
    /// </summary>
    public bool TextAlign { get; set; }

    /// <summary>
    /// Underline formatting
    /// </summary>
    public bool Underline { get; set; }

    /// <summary>
    /// Strike-through formatting
    /// </summary>
    public bool Strike { get; set; }

    /// <summary>
    /// Clear formatting button
    /// </summary>
    public bool ClearFormatting { get; set; }
}

/// <summary>
/// Editor dimensions configuration
/// </summary>
public class EditorDimensions
{
    /// <summary>
    /// Width in pixels or CSS unit (null for auto)
    /// </summary>
    public string? Width { get; set; }

    /// <summary>
    /// Height in pixels or CSS unit (null for auto)
    /// </summary>
    public string? Height { get; set; } = "300px";

    /// <summary>
    /// Minimum height in pixels
    /// </summary>
    public int? MinHeight { get; set; } = 100;

    /// <summary>
    /// Maximum height in pixels (null for unlimited)
    /// </summary>
    public int? MaxHeight { get; set; }
}

/// <summary>
/// Image size constraints
/// </summary>
public class ImageConstraints
{
    /// <summary>
    /// Maximum width in pixels
    /// </summary>
    public int MaxWidth { get; set; } = 800;

    /// <summary>
    /// Maximum height in pixels
    /// </summary>
    public int MaxHeight { get; set; } = 600;

    /// <summary>
    /// Whether to maintain aspect ratio during resize
    /// </summary>
    public bool MaintainAspectRatio { get; set; } = true;
}

/// <summary>
/// HTML sanitization and validation policy
/// </summary>
public class HtmlPolicy
{
    /// <summary>
    /// Allowed HTML tags
    /// </summary>
    public HashSet<string> AllowedTags { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "p", "br", "div", "span", "strong", "em", "b", "i", "u", "s", "sub", "sup",
        "h1", "h2", "h3", "blockquote", "pre", "code",
        "ul", "ol", "li", "table", "thead", "tbody", "tr", "th", "td",
        "a", "img", "figure", "figcaption", "hr"
    };

    /// <summary>
    /// Allowed HTML attributes
    /// </summary>
    public Dictionary<string, HashSet<string>> AllowedAttributes { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ["*"] = new(StringComparer.OrdinalIgnoreCase) { "class", "id" },
        ["a"] = new(StringComparer.OrdinalIgnoreCase) { "href", "target", "rel" },
        ["img"] = new(StringComparer.OrdinalIgnoreCase) { "src", "alt", "width", "height", "max-width" },
        ["table"] = new(StringComparer.OrdinalIgnoreCase) { "border", "cellpadding", "cellspacing" },
        ["td"] = new(StringComparer.OrdinalIgnoreCase) { "colspan", "rowspan", "align", "valign" },
        ["th"] = new(StringComparer.OrdinalIgnoreCase) { "colspan", "rowspan", "align", "valign" }
    };

    /// <summary>
    /// Whether to allow data URLs in images
    /// </summary>
    public bool AllowDataUrls { get; set; }

    /// <summary>
    /// Whether to allow external images
    /// </summary>
    public bool AllowExternalImages { get; set; } = true;
}

/// <summary>
/// Keyboard shortcuts configuration
/// </summary>
public class ShortcutMap
{
    /// <summary>
    /// Bold shortcut (default: Ctrl+B)
    /// </summary>
    public string Bold { get; set; } = "Control+b";

    /// <summary>
    /// Italic shortcut (default: Ctrl+I)
    /// </summary>
    public string Italic { get; set; } = "Control+i";

    /// <summary>
    /// Underline shortcut (default: Ctrl+U)
    /// </summary>
    public string Underline { get; set; } = "Control+u";

    /// <summary>
    /// Link shortcut (default: Ctrl+K)
    /// </summary>
    public string Link { get; set; } = "Control+k";

    /// <summary>
    /// Unordered list shortcut (default: Ctrl+Shift+8)
    /// </summary>
    public string UnorderedList { get; set; } = "Control+Shift+8";

    /// <summary>
    /// Ordered list shortcut (default: Ctrl+Shift+7)
    /// </summary>
    public string OrderedList { get; set; } = "Control+Shift+7";

    /// <summary>
    /// Save shortcut (default: Ctrl+S)
    /// </summary>
    public string Save { get; set; } = "Control+s";
}
