using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Services;

internal sealed record ToolbarArrangement(
    IReadOnlyList<ToolbarSegment> Segments,
    IReadOnlyList<ToolbarSlot> Slots,
    IReadOnlyList<IToolbarItem> Items,
    IReadOnlyList<string> MissingIds)
{
    public static ToolbarArrangement Empty { get; } = new([], [], [], []);

    public IToolbarItem? Find(string itemId) =>
        Items.FirstOrDefault(item => string.Equals(item.Id, itemId, StringComparison.OrdinalIgnoreCase));

    public static ToolbarArrangement Create(ToolbarLayout layout, IReadOnlyDictionary<string, IToolbarItem> items)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(items);
        List<ToolbarSegment> segments = [];
        List<ToolbarSlot> slots = [];
        List<string> missing = [];

        foreach (var entry in layout.Entries)
        {
            var (baseClass, ids) = entry switch
            {
                ToolbarGroup group => ("rte-toolbar-block", group.ItemIds),
                ToolbarItemRef item => ("rte-toolbar-single-item", (IReadOnlyList<string>?)[item.ItemId]),
                _ => ("rte-toolbar-separator", null)
            };
            var cssClass = entry.CssClass is { Length: > 0 } extra ? $"{baseClass} {extra}" : baseClass;

            if (ids is null)
            {
                segments.Add(new(segments.Count, cssClass, true, []));
                continue;
            }

            var first = slots.Count;
            foreach (var id in ids)
            {
                if (items.TryGetValue(id, out var item))
                {
                    slots.Add(new(slots.Count, item, ResolveChildren(item, items, missing)));
                }
                else
                {
                    missing.Add(id);
                }
            }

            if (slots.Count > first)
            {
                segments.Add(new(segments.Count, cssClass, false, slots[first..]));
            }
        }

        IToolbarItem[] all = [.. slots.SelectMany(slot => slot.Children.Prepend(slot.Item)).Distinct()];
        return new(segments, slots, all, missing);
    }

    private static IToolbarItem[] ResolveChildren(
        IToolbarItem item,
        IReadOnlyDictionary<string, IToolbarItem> items,
        List<string> missing)
    {
        if (item.Kind is not ToolbarItemKind.Dropdown)
        {
            return [];
        }

        missing.AddRange(item.ChildItemIds.Where(id => !items.ContainsKey(id)));
        return [.. item.ChildItemIds.Where(items.ContainsKey).Select(id => items[id])];
    }
}
