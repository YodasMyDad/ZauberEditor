using System.Text.Json.Serialization;

namespace ZauberCMS.RTE.Models;

/// <summary>
/// Type of layout item
/// </summary>
public enum LayoutItemType
{
    /// <summary>
    /// A block/group containing multiple toolbar items
    /// </summary>
    Block,
    
    /// <summary>
    /// A single toolbar item
    /// </summary>
    Item,
    
    /// <summary>
    /// A visual separator between blocks
    /// </summary>
    Separator
}

/// <summary>
/// Base class for layout items
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(ToolbarBlock), typeDiscriminator: "block")]
[JsonDerivedType(typeof(ToolbarItemReference), typeDiscriminator: "item")]
[JsonDerivedType(typeof(ToolbarSeparator), typeDiscriminator: "separator")]
public abstract class ToolbarLayoutItem
{
    /// <summary>
    /// Type of this layout item
    /// </summary>
    public abstract LayoutItemType Type { get; }
}

/// <summary>
/// A block/group of toolbar items that will be visually grouped together
/// </summary>
public class ToolbarBlock : ToolbarLayoutItem
{
    public override LayoutItemType Type => LayoutItemType.Block;
    
    /// <summary>
    /// Items in this block (can be item IDs or nested blocks)
    /// </summary>
    public List<string> Items { get; set; } = [];

    /// <summary>
    /// Optional CSS class for custom styling
    /// </summary>
    public string? CssClass { get; set; }

    public ToolbarBlock() { }

    public ToolbarBlock(params string[] items)
    {
        Items = [..items];
    }

    public ToolbarBlock(string? cssClass, IEnumerable<string> items)
    {
        CssClass = cssClass;
        Items = [..items];
    }
}

/// <summary>
/// A single toolbar item reference
/// </summary>
public class ToolbarItemReference : ToolbarLayoutItem
{
    public override LayoutItemType Type => LayoutItemType.Item;
    
    /// <summary>
    /// ID of the toolbar item
    /// </summary>
    public string ItemId { get; set; } = string.Empty;

    /// <summary>
    /// Optional CSS class for custom styling
    /// </summary>
    public string? CssClass { get; set; }

    public ToolbarItemReference() { }

    public ToolbarItemReference(string itemId, string? cssClass = null)
    {
        ItemId = itemId;
        CssClass = cssClass;
    }
}

/// <summary>
/// A visual separator
/// </summary>
public class ToolbarSeparator : ToolbarLayoutItem
{
    public override LayoutItemType Type => LayoutItemType.Separator;
    
    /// <summary>
    /// Optional CSS class for custom styling
    /// </summary>
    public string? CssClass { get; set; }

    public ToolbarSeparator() { }

    public ToolbarSeparator(string? cssClass = null)
    {
        CssClass = cssClass;
    }
}

/// <summary>
/// Extension methods for building toolbar layouts
/// </summary>
public static class ToolbarLayoutExtensions
{
    /// <summary>
    /// Creates a block with the specified items
    /// </summary>
    public static ToolbarBlock Block(params string[] items) => new(items);

    /// <summary>
    /// Creates a block with custom CSS class and items
    /// </summary>
    public static ToolbarBlock Block(string cssClass, IEnumerable<string> items) => new(cssClass, items);

    /// <summary>
    /// Creates an item reference
    /// </summary>
    public static ToolbarItemReference Item(string itemId, string? cssClass = null) => 
        new(itemId, cssClass);

    /// <summary>
    /// Creates a separator
    /// </summary>
    public static ToolbarSeparator Separator(string? cssClass = null) => new(cssClass);
}

