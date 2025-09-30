using System.Text.Json.Serialization;

namespace Zauber.RTE.Models;

/// <summary>
/// Represents the layout configuration for toolbar items
/// </summary>
public class ToolbarLayout
{
    /// <summary>
    /// Rows of toolbar items, where each row is an array of item IDs
    /// </summary>
    public List<string[]> Rows { get; set; } = new();

    /// <summary>
    /// Creates a toolbar layout from row arrays
    /// </summary>
    public static ToolbarLayout FromRows(params string[][] rows) => new()
    {
        Rows = new List<string[]>(rows)
    };

    /// <summary>
    /// Default toolbar layout with common items
    /// </summary>
    public static ToolbarLayout Default => FromRows(
        ["bold", "italic", "underline", "strike"],
        ["h1", "h2", "h3", "blockquote"],
        ["ul", "ol", "link", "image"],
        ["alignLeft", "alignCenter", "alignRight"]
    );

    /// <summary>
    /// Minimal toolbar layout
    /// </summary>
    public static ToolbarLayout Minimal => FromRows(
        ["bold", "italic", "link"]
    );

    /// <summary>
    /// Full toolbar layout with all available items
    /// </summary>
    public static ToolbarLayout Full => FromRows(
        ["viewSource", "h1", "h2", "h3", "bold", "italic", "underline", "strike", "subscript", "superscript", "clear"],
        ["alignLeft", "alignCenter", "alignRight", "justified"],
        ["ul", "ol", "blockquote", "codeBlock", "table", "link", "image", "upload"]
    );

    /// <summary>
    /// CMS-focused toolbar layout
    /// </summary>
    public static ToolbarLayout Cms => FromRows(
        ["viewSource", "h1", "bold", "italic", "underline", "strike", "clear"],
        ["alignLeft", "alignCenter", "alignRight", "justified"],
        ["ul", "ol", "blockquote", "codeBlock", "table", "link", "image", "upload"]
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
