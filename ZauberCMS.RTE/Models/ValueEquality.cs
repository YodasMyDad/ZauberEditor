namespace ZauberCMS.RTE.Models;

internal static class ValueEquality
{
    public static bool Sequence<T>(IReadOnlyList<T> left, IReadOnlyList<T> right) =>
        ReferenceEquals(left, right) || left.SequenceEqual(right);

    public static int SequenceHash<T>(IEnumerable<T> items)
    {
        HashCode hash = new();
        foreach (var item in items)
        {
            hash.Add(item);
        }

        return hash.ToHashCode();
    }

    public static bool Set(IReadOnlySet<string> left, IReadOnlySet<string> right) =>
        ReferenceEquals(left, right) || (left.SetEquals(right) && right.SetEquals(left));

    public static bool SetMap(IReadOnlyDictionary<string, IReadOnlySet<string>> left, IReadOnlyDictionary<string, IReadOnlySet<string>> right) =>
        ReferenceEquals(left, right)
        || (left.Count == right.Count && left.All(pair => right.TryGetValue(pair.Key, out var set) && Set(pair.Value, set)));
}
