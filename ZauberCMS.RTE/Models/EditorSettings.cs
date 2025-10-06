namespace ZauberCMS.RTE.Models;

/// <summary>
/// Configuration settings for the Zauber Rich Text Editor
/// </summary>
public class EditorSettings
{
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
    /// Default theme for the editor
    /// </summary>
    public Theme DefaultTheme { get; set; } = Theme.Auto;

    /// <summary>
    /// Block-level HTML tags recognized by the editor.
    /// These are used for navigation, selection, and formatting operations.
    /// Customize this to support custom block elements.
    /// </summary>
    public HashSet<string> BlockTags { get; set; } = 
    [
        "p", "div", "h1", "h2", "h3", "h4", "h5", "h6", 
        "blockquote", "pre", "li", "td", "th", "ul", "ol", "table"
    ];

    /// <summary>
    /// Creates a default settings configuration suitable for CMS usage with full toolbar
    /// </summary>
    public static EditorSettings CmsDefault() => new()
    {
        ToolbarLayout = ToolbarLayout.Full,
        ImageConstraints = new ImageConstraints
        {
            MaxWidth = 800,
            MaxHeight = 600
        }
    };

    /// <summary>
    /// Creates a minimal settings configuration with simple toolbar
    /// </summary>
    public static EditorSettings Minimal() => new()
    {
        ToolbarLayout = ToolbarLayout.Simple
    };
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

    /// <summary>
    /// Allowed image file types for upload (case insensitive)
    /// </summary>
    public HashSet<string> AllowedImageTypes { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".gif", ".png"
    };

    /// <summary>
    /// Whether to allow uploading images from disk as base64 data URLs.
    /// If false, only URL-based image insertion is available.
    /// Note: Enabling this requires increasing SignalR MaximumReceiveMessageSize (see documentation).
    /// </summary>
    public bool AllowBase64ImageUpload { get; set; } = false;
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
        ["*"] = new(StringComparer.OrdinalIgnoreCase) { "class", "id", "data-*", "title", "lang", "aria-*", "role", "dir", "tabindex" },
        ["a"] = new(StringComparer.OrdinalIgnoreCase) { "href", "target", "rel" },
        ["img"] = new(StringComparer.OrdinalIgnoreCase) { "src", "alt", "width", "height", "max-width", "style" },
        ["table"] = new(StringComparer.OrdinalIgnoreCase) { "border", "cellpadding", "cellspacing" },
        ["td"] = new(StringComparer.OrdinalIgnoreCase) { "colspan", "rowspan", "align", "valign" },
        ["th"] = new(StringComparer.OrdinalIgnoreCase) { "colspan", "rowspan", "align", "valign" }
    };

    /// <summary>
    /// Whether to allow data URLs in images (base64 encoded images)
    /// </summary>
    public bool AllowDataUrls { get; set; } = true;

    /// <summary>
    /// Whether to allow external images
    /// </summary>
    public bool AllowExternalImages { get; set; } = true;
}
