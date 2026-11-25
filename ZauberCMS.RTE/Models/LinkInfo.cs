namespace ZauberCMS.RTE.Models;

/// <summary>A link in the editor.</summary>
/// <param name="Href">The link target.</param>
/// <param name="Text">The link text.</param>
/// <param name="Title">The title attribute.</param>
/// <param name="Target">The target attribute, such as <c>_blank</c>.</param>
public sealed record LinkInfo(string Href, string Text, string? Title, string? Target);
