namespace ZauberCMS.RTE.Models;

/// <summary>A group of toolbar items rendered together.</summary>
/// <param name="ItemIds">The ids of the items in the group.</param>
public sealed record ToolbarGroup(params IReadOnlyList<string> ItemIds) : ToolbarLayoutEntry
{
    /// <summary>The ids of the items in the group.</summary>
    public IReadOnlyList<string> ItemIds { get; init => field = LayoutGuard.NoNulls(value, nameof(ItemIds)); } = LayoutGuard.NoNulls(ItemIds, nameof(ItemIds));

    /// <summary>Whether both groups have the same CSS class and item ids in the same order.</summary>
    /// <param name="other">The group to compare with.</param>
    public bool Equals(ToolbarGroup? other) => other is not null && base.Equals(other) && ValueEquality.Sequence(ItemIds, other.ItemIds);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), ValueEquality.SequenceHash(ItemIds));
}
