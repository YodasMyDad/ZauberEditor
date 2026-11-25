using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Services;

/// <summary>Lists the toolbar items registered with <c>AddZauberRte</c>, for example for a toolbar picker.</summary>
public interface IToolbarItemCatalog
{
    /// <summary>Gets every registered item: built-ins first in their usual order, then custom items by id. Removed items are not listed.</summary>
    IReadOnlyList<ToolbarItemDescriptor> GetItems();
}
