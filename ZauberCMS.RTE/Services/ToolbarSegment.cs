namespace ZauberCMS.RTE.Services;

internal sealed record ToolbarSegment(int Key, string CssClass, bool IsSeparator, IReadOnlyList<ToolbarSlot> Slots);
