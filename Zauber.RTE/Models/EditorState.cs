namespace Zauber.RTE.Models;

/// <summary>
/// Represents the current state of the editor
/// </summary>
public class EditorState
{
    /// <summary>
    /// Whether the editor has focus
    /// </summary>
    public bool HasFocus { get; set; }

    /// <summary>
    /// Whether there is an active text selection
    /// </summary>
    public bool HasSelection { get; set; }

    /// <summary>
    /// The currently active inline marks (formatting)
    /// </summary>
    public HashSet<string> ActiveMarks { get; set; } = new();

    /// <summary>
    /// The block type at the current caret position
    /// </summary>
    public string? CurrentBlockType { get; set; }

    /// <summary>
    /// The heading level if current block is a heading
    /// </summary>
    public int? CurrentHeadingLevel { get; set; }

    /// <summary>
    /// Whether the current selection can be formatted
    /// </summary>
    public bool CanFormat { get; set; } = true;

    /// <summary>
    /// Whether undo is available
    /// </summary>
    public bool CanUndo { get; set; }

    /// <summary>
    /// Whether redo is available
    /// </summary>
    public bool CanRedo { get; set; }

    /// <summary>
    /// Current text alignment at caret
    /// </summary>
    public TextAlignment CurrentAlignment { get; set; } = TextAlignment.Left;

    /// <summary>
    /// Whether the editor content has been modified since last save
    /// </summary>
    public bool IsDirty { get; set; }

    /// <summary>
    /// Current word count
    /// </summary>
    public int WordCount { get; set; }

    /// <summary>
    /// Current character count
    /// </summary>
    public int CharacterCount { get; set; }

    /// <summary>
    /// Whether the editor is in source view mode
    /// </summary>
    public bool IsSourceView { get; set; }

    /// <summary>
    /// Creates a copy of the current state
    /// </summary>
    public EditorState Clone() => new()
    {
        HasFocus = HasFocus,
        HasSelection = HasSelection,
        ActiveMarks = new HashSet<string>(ActiveMarks),
        CurrentBlockType = CurrentBlockType,
        CurrentHeadingLevel = CurrentHeadingLevel,
        CanFormat = CanFormat,
        CanUndo = CanUndo,
        CanRedo = CanRedo,
        CurrentAlignment = CurrentAlignment,
        IsDirty = IsDirty,
        WordCount = WordCount,
        CharacterCount = CharacterCount,
        IsSourceView = IsSourceView
    };
}

/// <summary>
/// Text alignment options
/// </summary>
public enum TextAlignment
{
    /// <summary>
    /// Left alignment
    /// </summary>
    Left,

    /// <summary>
    /// Center alignment
    /// </summary>
    Center,

    /// <summary>
    /// Right alignment
    /// </summary>
    Right,

    /// <summary>
    /// Justified alignment
    /// </summary>
    Justified
}

/// <summary>
/// Selection information
/// </summary>
public class SelectionInfo
{
    /// <summary>
    /// Start offset within the document
    /// </summary>
    public int StartOffset { get; set; }

    /// <summary>
    /// End offset within the document
    /// </summary>
    public int EndOffset { get; set; }

    /// <summary>
    /// Whether the selection is collapsed (caret)
    /// </summary>
    public bool IsCollapsed { get; set; }

    /// <summary>
    /// Selected text content
    /// </summary>
    public string? SelectedText { get; set; }

    /// <summary>
    /// HTML content of the selection
    /// </summary>
    public string? SelectedHtml { get; set; }
}
