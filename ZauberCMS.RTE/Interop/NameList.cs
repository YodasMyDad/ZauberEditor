namespace ZauberCMS.RTE.Interop;

internal static class NameList
{
    public static string[] Normalise(IEnumerable<string> names) =>
        [.. names.Select(name => name.ToLowerInvariant()).Distinct().Order(StringComparer.Ordinal)];
}
