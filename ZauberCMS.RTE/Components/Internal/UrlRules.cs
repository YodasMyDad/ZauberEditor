namespace ZauberCMS.RTE.Components.Internal;

internal static class UrlRules
{
    public static IReadOnlyList<string> LinkSchemes { get; } = ["http", "https", "mailto", "tel"];

    public static IReadOnlyList<string> ImageSchemes { get; } = ["http", "https"];

    public static bool HasAllowedScheme(string url, IReadOnlyList<string> schemes)
    {
        var trimmed = url.Trim();
        var colon = trimmed.IndexOf(':', StringComparison.Ordinal);
        if (colon < 0)
        {
            return true;
        }

        var delimiter = trimmed.AsSpan().IndexOfAny('/', '?', '#');
        return (delimiter >= 0 && delimiter < colon)
            || schemes.Contains(trimmed[..colon], StringComparer.OrdinalIgnoreCase);
    }
}
