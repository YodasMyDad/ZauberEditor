using ZauberCMS.RTE.Toolbar;

namespace ZauberCMS.RTE.Models;

/// <summary>A keyboard shortcut handled by the editor without a toolbar button.</summary>
/// <param name="Keys">The key combination, such as <c>Control+s</c>; <c>Control</c> matches Ctrl or Cmd.</param>
/// <param name="Handler">Runs when the combination is pressed in the editor.</param>
/// <exception cref="ArgumentException"><paramref name="Keys"/> is not a valid key combination.</exception>
public sealed record EditorShortcut(string Keys, Func<IEditorApi, CancellationToken, Task> Handler)
{
    /// <summary>The key combination, such as <c>Control+s</c>.</summary>
    public string Keys { get; init => field = Validate(value, nameof(Keys)); } = Validate(Keys, nameof(Keys));

    /// <summary>Runs when the combination is pressed in the editor.</summary>
    public Func<IEditorApi, CancellationToken, Task> Handler { get; init => field = LayoutGuard.NotNull(value, nameof(Handler)); } =
        LayoutGuard.NotNull(Handler, nameof(Handler));

    private static string Validate(string keys, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keys, paramName);
        return KeyboardShortcut.TryParse(keys, out _)
            ? keys
            : throw new ArgumentException($"'{keys}' is not a valid key combination, such as Control+s.", paramName);
    }
}
