namespace ZauberCMS.RTE.Models;

/// <summary>
/// Represents the layout configuration for toolbar items
/// </summary>
public class ToolbarLayout
{
    /// <summary>
    /// Flexible layout system using blocks, separators, and items
    /// </summary>
    public List<ToolbarLayoutItem> LayoutItems { get; set; } = [];

    /// <summary>
    /// Gets the effective layout items
    /// </summary>
    public List<ToolbarLayoutItem> GetEffectiveLayout() => LayoutItems;

    /// <summary>
    /// Creates a toolbar layout from layout items
    /// </summary>
    public static ToolbarLayout FromItems(params ToolbarLayoutItem[] items) => new()
    {
        LayoutItems = [..items]
    };

    /// <summary>
    /// Simple toolbar layout - basic formatting only
    /// </summary>
    public static ToolbarLayout Simple => FromItems(
        new ToolbarBlock("bold", "italic", "underline"),
        new ToolbarSeparator(),
        new ToolbarBlock("clear"),
        new ToolbarSeparator(),
        new ToolbarBlock("alignLeft", "alignCenter", "alignRight")
    );

    /// <summary>
    /// Full toolbar layout - all features logically grouped
    /// </summary>
    public static ToolbarLayout Full => FromItems(
        new ToolbarBlock("undo", "redo"),
        new ToolbarSeparator(),
        new ToolbarBlock("viewSource"),
        new ToolbarSeparator(),
        new ToolbarBlock("headings", "blockquote"),
        new ToolbarSeparator(),
        new ToolbarBlock("bold", "italic", "underline", "strike", "code"),
        new ToolbarSeparator(),
        new ToolbarBlock("subscript", "superscript"),
        new ToolbarSeparator(),
        new ToolbarBlock("alignLeft", "alignCenter", "alignRight", "justified"),
        new ToolbarSeparator(),
        new ToolbarBlock("ul", "ol"),
        new ToolbarSeparator(),
        new ToolbarBlock("link", "unlink"),
        new ToolbarSeparator(),
        new ToolbarBlock("image", "table"),
        new ToolbarSeparator(),
        new ToolbarBlock("clear")
    );

    /// <summary>
    /// Default toolbar layout (points to Simple for backward compatibility)
    /// </summary>
    public static ToolbarLayout Default => Simple;

    /// <summary>
    /// Minimal toolbar layout
    /// </summary>
    public static ToolbarLayout Minimal => FromItems(
        new ToolbarBlock("bold", "italic", "link")
    );

    /// <summary>
    /// CMS-focused toolbar layout
    /// </summary>
    public static ToolbarLayout Cms => FromItems(
        new ToolbarBlock("viewSource", "headings", "bold", "italic", "underline", "strike", "clear"),
        new ToolbarSeparator(),
        new ToolbarBlock("alignLeft", "alignCenter", "alignRight", "justified"),
        new ToolbarSeparator(),
        new ToolbarBlock("ul", "ol", "blockquote", "code", "table", "link", "unlink", "image")
    );

    /// <summary>
    /// Serializes the layout to JSON
    /// </summary>
    public string ToJson() => System.Text.Json.JsonSerializer.Serialize(this);

    /// <summary>
    /// Deserializes a layout from JSON
    /// </summary>
    public static ToolbarLayout FromJson(string json) =>
        System.Text.Json.JsonSerializer.Deserialize<ToolbarLayout>(json) ?? new ToolbarLayout();
}

/// <summary>
/// Placement categories for toolbar items
/// </summary>
public enum ToolbarPlacement
{
    /// <summary>
    /// Inline formatting items (bold, italic, etc.)
    /// </summary>
    Inline,

    /// <summary>
    /// Block-level formatting items (headings, lists, etc.)
    /// </summary>
    Block,

    /// <summary>
    /// Insert items (links, images, tables, etc.)
    /// </summary>
    Insert,

    /// <summary>
    /// Media-related items
    /// </summary>
    Media,

    /// <summary>
    /// Custom or specialized items
    /// </summary>
    Custom
}

/// <summary>
/// Theme options for the editor
/// </summary>
public enum Theme
{
    /// <summary>
    /// Automatically detect system preference
    /// </summary>
    Auto,

    /// <summary>
    /// Force light theme
    /// </summary>
    Light,

    /// <summary>
    /// Force dark theme
    /// </summary>
    Dark
}
