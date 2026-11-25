namespace ZauberCMS.RTE.Models;

/// <summary>A button or dropdown shown in the editor toolbar.</summary>
public interface IToolbarItem
{
    /// <summary>Unique, case-insensitive id used in layouts and to override built-in items.</summary>
    string Id { get; }

    /// <summary>Accessible name, also shown as text when the item has no icon.</summary>
    string Label { get; }

    /// <summary>Tooltip text; defaults to <see cref="Label"/> when null.</summary>
    string? Tooltip { get; }

    /// <summary>Trusted inline SVG markup for the icon.</summary>
    string? IconSvg { get; }

    /// <summary>CSS classes for an icon font, rendered as given (for example <c>fa-solid fa-star</c>).</summary>
    string? IconCss { get; }

    /// <summary>Keyboard shortcut such as <c>Control+Shift+7</c>; <c>Control</c> matches Ctrl or Cmd.</summary>
    string? Shortcut { get; }

    /// <summary>Whether the item shows a pressed state.</summary>
    bool IsToggle { get; }

    /// <summary>Whether the item is a button or a dropdown.</summary>
    ToolbarItemKind Kind { get; }

    /// <summary>Ids of the items listed in a dropdown.</summary>
    IReadOnlyList<string> ChildItemIds { get; }

    /// <summary>Tags that mark this item as active when they wrap the selection.</summary>
    IReadOnlyList<string> TrackedTags { get; }

    /// <summary>Canonical tag the tracked tags are reported as.</summary>
    string? PrimaryTag { get; }

    /// <summary>Whether the item can be used in the given state.</summary>
    /// <param name="state">The current editor state.</param>
    bool IsEnabled(EditorState state);

    /// <summary>Whether the item is active (pressed) in the given state.</summary>
    /// <param name="state">The current editor state.</param>
    bool IsActive(EditorState state);

    /// <summary>Performs the item's action.</summary>
    /// <param name="api">The editor the item belongs to.</param>
    /// <param name="cancellationToken">Cancelled when the editor is disposed.</param>
    Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken);
}
