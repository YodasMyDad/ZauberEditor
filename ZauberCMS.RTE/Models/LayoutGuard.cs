namespace ZauberCMS.RTE.Models;

internal static class LayoutGuard
{
    public static T NotNull<T>(T? value, string paramName)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        return value;
    }

    public static IReadOnlyList<T> NoNulls<T>(IReadOnlyList<T>? items, string paramName)
        where T : class =>
        NotNull(items, paramName).Any(item => item is null)
            ? throw new ArgumentException("The list must not contain null entries.", paramName)
            : items!;
}
