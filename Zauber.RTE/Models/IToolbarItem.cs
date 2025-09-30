namespace Zauber.RTE.Models;

/// <summary>
/// Interface for toolbar items that can be plugged into the editor
/// </summary>
public interface IToolbarItem
{
    /// <summary>
    /// Unique identifier for this toolbar item
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Accessible name/label for this item
    /// </summary>
    string Label { get; }

    /// <summary>
    /// Optional tooltip text
    /// </summary>
    string? Tooltip { get; }

    /// <summary>
    /// Placement category for this item
    /// </summary>
    ToolbarPlacement Placement { get; }

    /// <summary>
    /// Font Awesome icon class (e.g., "fa-bold", "fa-align-left")
    /// </summary>
    string IconCss { get; }

    /// <summary>
    /// Whether this is a toggle item (can be on/off)
    /// </summary>
    bool IsToggle { get; }

    /// <summary>
    /// Determines if this item should be enabled given the current editor state
    /// </summary>
    bool IsEnabled(EditorState state);

    /// <summary>
    /// Determines if this toggle item is currently active
    /// </summary>
    bool IsActive(EditorState state);

    /// <summary>
    /// Executes the action for this toolbar item
    /// </summary>
    Task ExecuteAsync(EditorApi api);

    /// <summary>
    /// Optional Blazor component type to render in a slide-out panel
    /// </summary>
    Type? PanelComponent { get; }
}

/// <summary>
/// Abstract base class for toolbar items
/// </summary>
public abstract class ToolbarItemBase : IToolbarItem
{
    public abstract string Id { get; }
    public abstract string Label { get; }
    public virtual string? Tooltip => null;
    public abstract ToolbarPlacement Placement { get; }
    public abstract string IconCss { get; }
    public virtual bool IsToggle => false;
    public virtual Type? PanelComponent => null;

    public virtual bool IsEnabled(EditorState state) => true;
    public virtual bool IsActive(EditorState state) => false;
    public abstract Task ExecuteAsync(EditorApi api);
}
