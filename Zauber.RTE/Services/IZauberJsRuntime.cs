using Microsoft.JSInterop;
using Zauber.RTE.Models;

namespace Zauber.RTE.Services;

/// <summary>
/// Interface for Zauber RTE JavaScript runtime interop
/// </summary>
public interface IZauberJsRuntime
{
    /// <summary>
    /// Selection manipulation and reading
    /// </summary>
    SelectionInterop Selection { get; }

    /// <summary>
    /// Clipboard operations and paste handling
    /// </summary>
    ClipboardInterop Clipboard { get; }

    /// <summary>
    /// Image resize handles and manipulation
    /// </summary>
    ImageInterop Image { get; }

    /// <summary>
    /// DOM mutation observation for change events
    /// </summary>
    MutationInterop Mutation { get; }

    /// <summary>
    /// Slide-out panel animations and focus management
    /// </summary>
    PanelInterop Panel { get; }

    /// <summary>
    /// JavaScript interop for undo/redo operations
    /// </summary>
    HistoryInterop History { get; }
}

/// <summary>
/// JavaScript interop for selection manipulation
/// </summary>
public interface SelectionInterop
{
    /// <summary>
    /// Gets the current selection state
    /// </summary>
    Task<SelectionInfo?> GetSelectionAsync(string editorId);

    /// <summary>
    /// Sets the selection to a specific range
    /// </summary>
    Task SetSelectionAsync(string editorId, int startOffset, int endOffset);

    /// <summary>
    /// Collapses the selection to the start or end
    /// </summary>
    Task CollapseSelectionAsync(string editorId, bool toStart = false);

    /// <summary>
    /// Wraps the current selection with HTML tags
    /// </summary>
    Task WrapSelectionAsync(string editorId, string tagName, Dictionary<string, string>? attributes = null);

    /// <summary>
    /// Unwraps the current selection from HTML tags
    /// </summary>
    Task UnwrapSelectionAsync(string editorId, string tagName);

    /// <summary>
    /// Inserts HTML content at the current caret position
    /// </summary>
    Task InsertHtmlAsync(string editorId, string html);

    /// <summary>
    /// Gets the active marks (formatting) at the current selection
    /// </summary>
    Task<HashSet<string>> GetActiveMarksAsync(string editorId);

    /// <summary>
    /// Gets the block type at the current caret position
    /// </summary>
    Task<string?> GetCurrentBlockTypeAsync(string editorId);

    /// <summary>
    /// Gets the heading level if current block is a heading
    /// </summary>
    Task<int?> GetCurrentHeadingLevelAsync(string editorId);

    /// <summary>
    /// Applies or removes an inline mark
    /// </summary>
    Task ToggleMarkAsync(string editorId, string markName);

    /// <summary>
    /// Changes the block type at the current position
    /// </summary>
    Task SetBlockTypeAsync(string editorId, string blockType, Dictionary<string, string>? attributes = null);

    /// <summary>
    /// Applies CSS styles to the current block element
    /// </summary>
    Task SetBlockStyleAsync(string editorId, Dictionary<string, string> styles);

    /// <summary>
    /// Clears all formatting from the current selection
    /// </summary>
    Task ClearFormattingAsync(string editorId);

    /// <summary>
    /// Saves the current selection range for later restoration
    /// </summary>
    Task SaveRangeAsync(string editorId);

    /// <summary>
    /// Restores a previously saved selection range
    /// </summary>
    Task<bool> RestoreRangeAsync(string editorId);

    /// <summary>
    /// Clears the saved selection range
    /// </summary>
    Task ClearSavedRangeAsync(string editorId);

    /// <summary>
    /// Gets information about a link at the current cursor position
    /// </summary>
    Task<LinkInfo?> GetLinkAtCursorAsync(string editorId);

    /// <summary>
    /// Selects the entire link element at the cursor position
    /// </summary>
    Task<bool> SelectLinkAtCursorAsync(string editorId);

    /// <summary>
    /// Gets information about an image at the current cursor position
    /// </summary>
    Task<ImageInfo?> GetImageAtCursorAsync(string editorId);

    /// <summary>
    /// Selects the image element at the cursor position
    /// </summary>
    Task<bool> SelectImageAtCursorAsync(string editorId);
}

/// <summary>
/// JavaScript interop for clipboard operations
/// </summary>
public interface ClipboardInterop
{
    /// <summary>
    /// Reads HTML content from clipboard on paste
    /// </summary>
    Task<string?> GetClipboardHtmlAsync();

    /// <summary>
    /// Sanitizes and cleans pasted HTML content
    /// </summary>
    Task<string> CleanHtmlAsync(string html, HtmlPolicy policy);

    /// <summary>
    /// Inserts cleaned HTML content at caret position
    /// </summary>
    Task InsertCleanedHtmlAsync(string editorId, string html);
}

/// <summary>
/// JavaScript interop for image manipulation
/// </summary>
public interface ImageInterop
{
    /// <summary>
    /// Shows resize handles for an image element
    /// </summary>
    Task ShowResizeHandlesAsync(string editorId, object imageElement);

    /// <summary>
    /// Hides resize handles
    /// </summary>
    Task HideResizeHandlesAsync(string editorId);

    /// <summary>
    /// Updates image dimensions while maintaining aspect ratio
    /// </summary>
    Task ResizeImageAsync(string editorId, string imageSelector, int width, int height, bool maintainAspectRatio = true);

    /// <summary>
    /// Gets the natural dimensions of an image
    /// </summary>
    Task<(int width, int height)> GetImageDimensionsAsync(string imageSrc);

    /// <summary>
    /// Toggles aspect ratio preservation during resize
    /// </summary>
    Task<bool> ToggleAspectRatioAsync();
}

/// <summary>
/// JavaScript interop for DOM mutation observation
/// </summary>
public interface MutationInterop
{
    /// <summary>
    /// Starts observing DOM mutations for change detection
    /// </summary>
    Task StartObservingAsync(string editorId);

    /// <summary>
    /// Stops observing DOM mutations
    /// </summary>
    Task StopObservingAsync(string editorId);

    /// <summary>
    /// Gets the current HTML content from the editor
    /// </summary>
    Task<string?> GetHtmlAsync(string editorId);

    /// <summary>
    /// Sets the HTML content of the editor
    /// </summary>
    Task SetHtmlAsync(string editorId, string html);

    /// <summary>
    /// Serializes and normalizes the editor content
    /// </summary>
    Task<string> SerializeHtmlAsync(string editorId);
}

    /// <summary>
    /// JavaScript interop for panel animations and focus management
    /// </summary>
    public interface PanelInterop
    {
        /// <summary>
        /// Opens a slide-out panel with animation
        /// </summary>
        Task OpenPanelAsync(string editorId, string panelId);

        /// <summary>
        /// Closes the slide-out panel with animation
        /// </summary>
        Task ClosePanelAsync(string editorId, string panelId);

        /// <summary>
        /// Sets focus trap within the panel
        /// </summary>
        Task TrapFocusAsync(string panelId);

        /// <summary>
        /// Releases focus trap
        /// </summary>
        Task ReleaseFocusAsync(string panelId);
    }

    /// <summary>
    /// JavaScript interop for undo/redo operations
    /// </summary>
    public interface HistoryInterop
    {
        /// <summary>
        /// Records the current state for undo history
        /// </summary>
        Task RecordStateAsync(string editorId);

        /// <summary>
        /// Performs an undo operation
        /// </summary>
        Task<bool> UndoAsync(string editorId);

        /// <summary>
        /// Performs a redo operation
        /// </summary>
        Task<bool> RedoAsync(string editorId);

        /// <summary>
        /// Checks if undo is available
        /// </summary>
        Task<bool> CanUndoAsync(string editorId);

        /// <summary>
        /// Checks if redo is available
        /// </summary>
        Task<bool> CanRedoAsync(string editorId);

        /// <summary>
        /// Clears the undo/redo history
        /// </summary>
        Task ClearHistoryAsync(string editorId);
    }
