namespace ZauberCMS.RTE.Models;

/// <summary>The selection inside the editor.</summary>
/// <param name="Text">The selected text.</param>
/// <param name="IsCollapsed">Whether the selection is just a cursor.</param>
public sealed record SelectionInfo(string Text, bool IsCollapsed);
