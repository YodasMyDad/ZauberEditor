using System.Collections.Frozen;
using ZauberCMS.RTE.Models;
using ZauberCMS.RTE.Toolbar;

namespace ZauberCMS.RTE.Services;

internal sealed class ToolbarItemCatalog(ToolbarRegistry registry, IServiceProvider services) : IToolbarItemCatalog, IAsyncDisposable
{
    private static readonly FrozenDictionary<string, int> BuiltInPositions = BuiltInToolbarItems.All
        .Select((item, index) => KeyValuePair.Create(item.Id, index))
        .ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    private ToolbarItemSet? _set;
    private IReadOnlyList<ToolbarItemDescriptor>? _items;

    public IReadOnlyList<ToolbarItemDescriptor> GetItems() => _items ??= Describe(_set = registry.CreateItemSet(services));

    public ValueTask DisposeAsync() => _set?.DisposeAsync() ?? ValueTask.CompletedTask;

    private static ToolbarItemDescriptor[] Describe(ToolbarItemSet set) =>
    [
        .. set.Items.Values
            .Select(item => ToolbarItemDescriptor.From(item, BuiltInToolbarItems.All.Contains(item)))
            .OrderBy(item => BuiltInPositions.GetValueOrDefault(item.Id, int.MaxValue))
            .ThenBy(item => item.Id, StringComparer.OrdinalIgnoreCase)
    ];
}
