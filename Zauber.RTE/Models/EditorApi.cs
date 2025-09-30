namespace Zauber.RTE.Models;

/// <summary>
/// API interface for toolbar items to interact with the editor
/// </summary>
public interface EditorApi
{
    /// <summary>
    /// Gets the current editor state
    /// </summary>
    EditorState GetState();

    /// <summary>
    /// Inserts HTML content at the current caret position
    /// </summary>
    Task InsertHtmlAsync(string html);

    /// <summary>
    /// Inserts plain text at the current caret position
    /// </summary>
    Task InsertTextAsync(string text);

    /// <summary>
    /// Wraps the current selection with the specified HTML tags
    /// </summary>
    Task WrapSelectionAsync(string tagName, Dictionary<string, string>? attributes = null);

    /// <summary>
    /// Unwraps the current selection from the specified HTML tags
    /// </summary>
    Task UnwrapSelectionAsync(string tagName);

    /// <summary>
    /// Replaces the current selection with new HTML content
    /// </summary>
    Task ReplaceSelectionAsync(string html);

    /// <summary>
    /// Applies an inline mark to the current selection
    /// </summary>
    Task ToggleMarkAsync(string markName);

    /// <summary>
    /// Changes the block type at the current position
    /// </summary>
    Task SetBlockTypeAsync(string blockType, Dictionary<string, string>? attributes = null);

    /// <summary>
    /// Applies CSS styles to the current block element
    /// </summary>
    Task SetBlockStyleAsync(Dictionary<string, string> styles);

    /// <summary>
    /// Opens a slide-out panel with the specified component
    /// </summary>
    Task OpenPanelAsync(Type componentType, object? parameters = null);

    /// <summary>
    /// Closes the currently open slide-out panel
    /// </summary>
    Task ClosePanelAsync();

    /// <summary>
    /// Shows a toast notification
    /// </summary>
    Task ShowToastAsync(string message, ToastType type = ToastType.Info);

    /// <summary>
    /// Gets the current selection information
    /// </summary>
    SelectionInfo? GetSelection();

    /// <summary>
    /// Focuses the editor
    /// </summary>
    Task FocusAsync();

    /// <summary>
    /// Blurs the editor
    /// </summary>
    Task BlurAsync();

    /// <summary>
    /// Executes an undo operation
    /// </summary>
    Task UndoAsync();

    /// <summary>
    /// Executes a redo operation
    /// </summary>
    Task RedoAsync();

    /// <summary>
    /// Clears all formatting from the current selection
    /// </summary>
    Task ClearFormattingAsync();

    /// <summary>
    /// Gets the current HTML content
    /// </summary>
    string? GetHtml();

    /// <summary>
    /// Gets the current plain text content
    /// </summary>
    string? GetText();

    /// <summary>
    /// Sets the HTML content (replaces all content)
    /// </summary>
    Task SetHtmlAsync(string html);

    /// <summary>
    /// Sets the plain text content (replaces all content)
    /// </summary>
    Task SetTextAsync(string text);

    /// <summary>
    /// Toggles between rich text and source view modes
    /// </summary>
    Task ToggleSourceViewAsync();
}

/// <summary>
/// Toast notification types
/// </summary>
public enum ToastType
{
    /// <summary>
    /// Informational message
    /// </summary>
    Info,

    /// <summary>
    /// Success message
    /// </summary>
    Success,

    /// <summary>
    /// Warning message
    /// </summary>
    Warning,

    /// <summary>
    /// Error message
    /// </summary>
    Error
}

/// <summary>
/// Parameters for panel components
/// </summary>
public class PanelParameters
{
    /// <summary>
    /// The editor API instance
    /// </summary>
    public EditorApi Api { get; set; } = null!;

    /// <summary>
    /// Additional custom parameters
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();
}
