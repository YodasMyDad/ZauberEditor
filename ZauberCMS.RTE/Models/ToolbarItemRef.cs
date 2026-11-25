namespace ZauberCMS.RTE.Models;

/// <summary>A single toolbar item outside a group.</summary>
/// <param name="ItemId">The id of the item.</param>
public sealed record ToolbarItemRef(string ItemId) : ToolbarLayoutEntry
{
    /// <summary>The id of the item.</summary>
    public string ItemId { get; init => field = LayoutGuard.NotNull(value, nameof(ItemId)); } = LayoutGuard.NotNull(ItemId, nameof(ItemId));
}
