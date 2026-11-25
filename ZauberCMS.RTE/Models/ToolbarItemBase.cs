namespace ZauberCMS.RTE.Models;

/// <summary>Base class for toolbar items with sensible defaults.</summary>
public abstract class ToolbarItemBase : IToolbarItem
{
    /// <inheritdoc />
    public abstract string Id { get; }

    /// <inheritdoc />
    public abstract string Label { get; }

    /// <inheritdoc />
    public virtual string? Tooltip => null;

    /// <inheritdoc />
    public virtual string? IconSvg => null;

    /// <inheritdoc />
    public virtual string? IconCss => null;

    /// <inheritdoc />
    public virtual string? Shortcut => null;

    /// <inheritdoc />
    public virtual bool IsToggle => false;

    /// <inheritdoc />
    public virtual ToolbarItemKind Kind => ToolbarItemKind.Button;

    /// <inheritdoc />
    public virtual IReadOnlyList<string> ChildItemIds => [];

    /// <inheritdoc />
    public virtual IReadOnlyList<string> TrackedTags => [];

    /// <inheritdoc />
    public virtual string? PrimaryTag => null;

    /// <summary>Enabled unless the editor is read-only or showing HTML source.</summary>
    /// <param name="state">The current editor state.</param>
    public virtual bool IsEnabled(EditorState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        return state is { IsReadOnly: false, IsSourceView: false };
    }

    /// <summary>Active when <see cref="PrimaryTag"/> wraps the selection.</summary>
    /// <param name="state">The current editor state.</param>
    public virtual bool IsActive(EditorState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        return PrimaryTag is not null && state.ActiveMarks.Contains(PrimaryTag);
    }

    /// <inheritdoc />
    public abstract Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken);
}
