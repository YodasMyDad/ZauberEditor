namespace ZauberCMS.RTE.Models;

/// <summary>The outcome of a search in the editor.</summary>
/// <param name="Count">The number of matches in the content.</param>
/// <param name="Current">The 1-based position of the selected match, or 0 when none is selected.</param>
public sealed record FindResult(int Count, int Current)
{
    /// <summary>No matches.</summary>
    public static FindResult None { get; } = new(0, 0);
}
