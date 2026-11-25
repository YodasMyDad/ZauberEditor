using System.Collections.Frozen;
using ZauberCMS.RTE.Models;

namespace ZauberRTE.Web.Toolbar;

public static class DemoToolbar
{
    public static ToolbarLayout Layout { get; } = new(
        new ToolbarGroup(ToolbarItemIds.Bold, ToolbarItemIds.Italic, HighlightItem.ItemId, TextColorDropdownItem.ItemId),
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
