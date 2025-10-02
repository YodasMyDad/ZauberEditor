using ZauberCMS.RTE.Models.ToolbarItems;

namespace ZauberCMS.RTE.Models;

/// <summary>
/// Built-in toolbar items for common formatting operations
/// </summary>
public static class DefaultToolbarItems
{
    /// <summary>
    /// Registers all default toolbar items with the discovery service
    /// </summary>
    public static IEnumerable<IToolbarItem> GetAllDefaultItems()
    {
        yield return new BoldItem();
        yield return new ItalicItem();
        yield return new UnderlineItem();
        yield return new StrikeItem();
        yield return new CodeItem();
        yield return new SubscriptItem();
        yield return new SuperscriptItem();
        yield return new Heading1Item();
        yield return new Heading2Item();
        yield return new Heading3Item();
        yield return new BlockquoteItem();
        yield return new UnorderedListItem();
        yield return new OrderedListItem();
        yield return new AlignLeftItem();
        yield return new AlignCenterItem();
        yield return new AlignRightItem();
        yield return new JustifiedItem();
        yield return new LinkItem();
        yield return new UnlinkItem();
        yield return new ImageItem();
        yield return new ClearFormattingItem();
        yield return new UndoItem();
        yield return new RedoItem();
        yield return new ViewSourceItem();
        yield return new SettingsItem();
        yield return new ThemeToggleItem();
    }
}

