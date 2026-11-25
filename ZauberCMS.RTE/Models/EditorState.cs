using System.Collections.Frozen;

namespace ZauberCMS.RTE.Models;

/// <summary>An immutable snapshot of the editor's selection and mode.</summary>
public sealed record EditorState
{
    /// <summary>The state before the editor has reported anything.</summary>
    public static EditorState Empty { get; } = new();

    /// <summary>Primary tags of the marks around the selection (case-insensitive).</summary>
    public IReadOnlySet<string> ActiveMarks { get; init; } = FrozenSet<string>.Empty;

    /// <summary>Lower-case tag of the current block, such as <c>p</c>, <c>h2</c> or <c>ul</c>.</summary>
    public string BlockType { get; init; } = "p";

    /// <summary>Heading level of the current block, or 0 when it is not a heading.</summary>
    public int HeadingLevel => BlockType is ['h', var digit and >= '1' and <= '6'] ? digit - '0' : 0;

    /// <summary>Text alignment of the current block.</summary>
    public TextAlignment Alignment { get; init; }

    /// <summary>Whether text is selected.</summary>
    public bool HasSelection { get; init; }

    /// <summary>Whether the editor has focus.</summary>
    public bool HasFocus { get; init; }

    /// <summary>Whether there is a change to undo.</summary>
    public bool CanUndo { get; init; }

    /// <summary>Whether there is a change to redo.</summary>
    public bool CanRedo { get; init; }

    /// <summary>Whether the current block is indented or in a list, so it can be outdented.</summary>
    public bool CanOutdent { get; init; }

    /// <summary>Whether the cursor is inside a link.</summary>
    public bool InLink { get; init; }

    /// <summary>Whether an image is selected.</summary>
    public bool InImage { get; init; }

    /// <summary>Whether the cursor is inside a table.</summary>
    public bool InTable { get; init; }

    /// <summary>Whether the HTML source view is showing.</summary>
    public bool IsSourceView { get; init; }

    /// <summary>Whether the editor fills the browser window.</summary>
    public bool IsFullscreen { get; init; }

    /// <summary>Whether paste inserts plain text only.</summary>
    public bool IsPlainTextPaste { get; init; }

    /// <summary>Whether the editor is read-only.</summary>
    public bool IsReadOnly { get; init; }

    /// <summary>Number of words in the content, updated when a content change is delivered.</summary>
    public int WordCount { get; init; }

    /// <summary>Number of characters in the content, including spaces but not line breaks.</summary>
    public int CharacterCount { get; init; }

    /// <summary>The theme setting.</summary>
    public Theme Theme { get; init; }

    /// <summary>Whether the user's system prefers a dark colour scheme.</summary>
    public bool PrefersDarkScheme { get; init; }

    /// <summary>Whether the editor currently renders dark, resolving <see cref="Theme.Auto"/>.</summary>
    public bool IsDark => Theme is Theme.Dark || (Theme is Theme.Auto && PrefersDarkScheme);
}
