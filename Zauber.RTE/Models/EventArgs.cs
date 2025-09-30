using Microsoft.AspNetCore.Components.Web;

namespace Zauber.RTE.Models;

/// <summary>
/// Base class for editor events
/// </summary>
public abstract class EditorEventArgs : EventArgs
{
    /// <summary>
    /// Timestamp of the event
    /// </summary>
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}

/// <summary>
/// Arguments for editor change events
/// </summary>
public class EditorChangeArgs : EditorEventArgs
{
    /// <summary>
    /// The new HTML content
    /// </summary>
    public string? Html { get; set; }

    /// <summary>
    /// The new plain text content
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Whether this change was triggered by user input
    /// </summary>
    public bool IsUserInput { get; set; } = true;

    /// <summary>
    /// The source of the change (typing, paste, command, etc.)
    /// </summary>
    public ChangeSource Source { get; set; } = ChangeSource.User;
}

/// <summary>
/// Source of a content change
/// </summary>
public enum ChangeSource
{
    /// <summary>
    /// User typing or editing
    /// </summary>
    User,

    /// <summary>
    /// Paste operation
    /// </summary>
    Paste,

    /// <summary>
    /// Toolbar command execution
    /// </summary>
    Command,

    /// <summary>
    /// Programmatic change (API call)
    /// </summary>
    Programmatic,

    /// <summary>
    /// Undo/redo operation
    /// </summary>
    UndoRedo
}

/// <summary>
/// Arguments for paste events
/// </summary>
public class PasteArgs : EditorEventArgs
{
    /// <summary>
    /// The original HTML content being pasted
    /// </summary>
    public string? OriginalHtml { get; set; }

    /// <summary>
    /// The cleaned HTML content after processing
    /// </summary>
    public string? CleanedHtml { get; set; }

    /// <summary>
    /// The plain text content
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Whether the paste was cancelled
    /// </summary>
    public bool Cancelled { get; set; }

    /// <summary>
    /// Cancel the paste operation
    /// </summary>
    public void Cancel() => Cancelled = true;
}

/// <summary>
/// Arguments for image resize events
/// </summary>
public class ImageResizedArgs : EditorEventArgs
{
    /// <summary>
    /// The image element that was resized
    /// </summary>
    public string? ImageSrc { get; set; }

    /// <summary>
    /// The new width in pixels
    /// </summary>
    public int? NewWidth { get; set; }

    /// <summary>
    /// The new height in pixels
    /// </summary>
    public int? NewHeight { get; set; }

    /// <summary>
    /// The original width in pixels
    /// </summary>
    public int? OriginalWidth { get; set; }

    /// <summary>
    /// The original height in pixels
    /// </summary>
    public int? OriginalHeight { get; set; }

    /// <summary>
    /// Whether aspect ratio was maintained
    /// </summary>
    public bool MaintainedAspectRatio { get; set; }
}

/// <summary>
/// Arguments for command execution events
/// </summary>
public class CommandExecutedArgs : EditorEventArgs
{
    /// <summary>
    /// The command that was executed
    /// </summary>
    public string? Command { get; set; }

    /// <summary>
    /// Parameters passed to the command
    /// </summary>
    public object? Parameters { get; set; }

    /// <summary>
    /// Whether the command execution was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if the command failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Arguments for selection change events
/// </summary>
public class SelectionChangedArgs : EditorEventArgs
{
    /// <summary>
    /// Information about the current selection
    /// </summary>
    public SelectionInfo? Selection { get; set; }

    /// <summary>
    /// The current editor state
    /// </summary>
    public EditorState? EditorState { get; set; }
}

/// <summary>
/// Arguments for keyboard events
/// </summary>
public class ZauberKeyboardEventArgs : EditorEventArgs
{
    /// <summary>
    /// The key that was pressed
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// Whether Ctrl key was pressed
    /// </summary>
    public bool CtrlKey { get; set; }

    /// <summary>
    /// Whether Shift key was pressed
    /// </summary>
    public bool ShiftKey { get; set; }

    /// <summary>
    /// Whether Alt key was pressed
    /// </summary>
    public bool AltKey { get; set; }

    /// <summary>
    /// Whether Meta key was pressed (Cmd on Mac)
    /// </summary>
    public bool MetaKey { get; set; }

    /// <summary>
    /// Whether the event was cancelled
    /// </summary>
    public bool Cancelled { get; set; }

    /// <summary>
    /// Cancel the keyboard event
    /// </summary>
    public void Cancel() => Cancelled = true;

    /// <summary>
    /// Check if this matches a keyboard shortcut
    /// </summary>
    public bool MatchesShortcut(string shortcut)
    {
        if (string.IsNullOrEmpty(shortcut)) return false;

        var parts = shortcut.Split('+');
        var ctrl = parts.Contains("Control") || parts.Contains("Ctrl");
        var shift = parts.Contains("Shift");
        var alt = parts.Contains("Alt");
        var meta = parts.Contains("Meta") || parts.Contains("Cmd");
        var key = parts.Last().ToLower();

        return CtrlKey == ctrl &&
               ShiftKey == shift &&
               AltKey == alt &&
               MetaKey == meta &&
               (Key?.ToLower() == key || Key == key);
    }
}
