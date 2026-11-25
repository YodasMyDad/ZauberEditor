using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Interop;

internal sealed record EmbedProviderOptions(string Name, string DisplayName, IReadOnlyList<string> Patterns, string? Thumbnail)
{
    public static EmbedProviderOptions From(EmbedProvider provider) =>
        new(provider.Name, provider.DisplayName, provider.UrlPatterns, provider.ThumbnailUrlTemplate);
}
