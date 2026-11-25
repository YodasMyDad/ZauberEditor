namespace ZauberCMS.RTE.Models;

/// <summary>Describes a registered toolbar item, for example to let users pick their toolbar.</summary>
/// <param name="Id">The item id used in layouts.</param>
/// <param name="Label">The item's accessible name.</param>
/// <param name="Tooltip">The tooltip text, or null to use the label.</param>
/// <param name="IconSvg">Inline SVG markup for the icon.</param>
/// <param name="IconCss">Icon font CSS classes.</param>
/// <param name="Shortcut">The keyboard shortcut, such as <c>Control+b</c>.</param>
/// <param name="Kind">Whether the item is a button or a dropdown.</param>
/// <param name="ChildItemIds">Ids of the items a dropdown lists.</param>
/// <param name="IsBuiltIn">Whether the item is the editor's own, not a custom item or an override.</param>
public sealed record ToolbarItemDescriptor(
    string Id,
    string Label,
    string? Tooltip,
    string? IconSvg,
    string? IconCss,
    string? Shortcut,
    ToolbarItemKind Kind,
    IReadOnlyList<string> ChildItemIds,
    bool IsBuiltIn)
{
    internal static ToolbarItemDescriptor From(IToolbarItem item, bool isBuiltIn) => new(
        item.Id,
        item.Label,
        item.Tooltip,
        item.IconSvg,
        item.IconCss,
        item.Shortcut,
        item.Kind,
        [.. item.ChildItemIds],
        isBuiltIn);

    /// <summary>Whether both describe the same item, comparing child ids by value.</summary>
    /// <param name="other">The descriptor to compare with.</param>
    public bool Equals(ToolbarItemDescriptor? other) =>
        other is not null
        && (Id, Label, Tooltip, IconSvg, IconCss, Shortcut, Kind, IsBuiltIn)
            == (other.Id, other.Label, other.Tooltip, other.IconSvg, other.IconCss, other.Shortcut, other.Kind, other.IsBuiltIn)
        && ValueEquality.Sequence(ChildItemIds, other.ChildItemIds);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Id, Label, Kind, IsBuiltIn);
}
