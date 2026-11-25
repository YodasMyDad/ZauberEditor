using System.Collections.Frozen;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Services;

internal sealed class ToolbarItemSet(
    FrozenDictionary<string, IToolbarItem> items,
    IReadOnlyList<IToolbarItem> created,
    FrozenSet<string> removedIds)
    : IAsyncDisposable
{
    public IReadOnlyDictionary<string, IToolbarItem> Items => items;

    public bool WasRemoved(string itemId) => removedIds.Contains(itemId);

    public async ValueTask DisposeAsync()
    {
        foreach (var item in created)
        {
            switch (item)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync();
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }
    }
}
