namespace ZauberCMS.RTE.Models;

/// <summary>An embed placeholder in the editor.</summary>
/// <param name="Provider">The provider name, from <c>data-embed</c>.</param>
/// <param name="Id">The video id, from <c>data-embed-id</c>.</param>
/// <param name="Url">The link the embed was made from, from <c>data-embed-url</c>.</param>
public sealed record EmbedInfo(string Provider, string Id, string? Url);
