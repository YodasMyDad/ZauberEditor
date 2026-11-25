using System.Diagnostics.CodeAnalysis;

namespace ZauberCMS.RTE.Models;

/// <summary>Programmatic control of one editor instance.</summary>
public interface IEditorApi
{
    /// <summary>The latest editor state.</summary>
    EditorState State { get; }

    /// <summary>The editor's settings.</summary>
    EditorSettings Settings { get; }

    /// <summary>Toggles an inline mark such as <c>strong</c> on the selection.</summary>
    Task ToggleMarkAsync(string tag, CancellationToken cancellationToken = default);

    /// <summary>Wraps the selection in an element.</summary>
    Task WrapSelectionAsync(string tag, IReadOnlyDictionary<string, string>? attributes = null, CancellationToken cancellationToken = default);

    /// <summary>Removes the given element from around the selection.</summary>
    Task UnwrapSelectionAsync(string tag, CancellationToken cancellationToken = default);

    /// <summary>Changes the current block to another type such as <c>h2</c> or <c>p</c>.</summary>
    Task SetBlockTypeAsync(string blockType, IReadOnlyDictionary<string, string>? attributes = null, CancellationToken cancellationToken = default);

    /// <summary>Sets inline styles on the current block.</summary>
    Task SetBlockStyleAsync(IReadOnlyDictionary<string, string> styles, CancellationToken cancellationToken = default);

    /// <summary>Indents the current blocks, or nests list items under the item before them.</summary>
    Task IndentAsync(CancellationToken cancellationToken = default);

    /// <summary>Outdents the current blocks, or lifts list items out of their list.</summary>
    Task OutdentAsync(CancellationToken cancellationToken = default);

    /// <summary>Sets an inline style such as <c>color</c> on the selected text; a null <paramref name="value"/> removes it.</summary>
    Task SetTextStyleAsync(string styleName, string? value, CancellationToken cancellationToken = default);

    /// <summary>Removes inline formatting from the selection.</summary>
    Task ClearFormattingAsync(CancellationToken cancellationToken = default);

    /// <summary>Inserts HTML at the selection after sanitising it with the editor's policy.</summary>
    Task InsertHtmlAsync(string html, CancellationToken cancellationToken = default);

    /// <summary>Inserts plain text at the selection.</summary>
    Task InsertTextAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>Delivers any pending edits through <c>ValueChanged</c> and waits until its handler has finished. Await it before saving the bound value.</summary>
    Task FlushAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets the editor's current HTML.</summary>
    /// <exception cref="EditorContentTooLargeException">The content is larger than <see cref="EditorSettings.MaxContentBytes"/>.</exception>
    Task<string> GetHtmlAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets the editor's current text content.</summary>
    /// <exception cref="EditorContentTooLargeException">The text is larger than <see cref="EditorSettings.MaxContentBytes"/>.</exception>
    Task<string> GetTextAsync(CancellationToken cancellationToken = default);

    /// <summary>Replaces the content with sanitised HTML as an undoable change.</summary>
    /// <exception cref="InvalidOperationException">The editor is read-only; set its <c>Value</c> instead.</exception>
    Task SetHtmlAsync(string html, CancellationToken cancellationToken = default);

    /// <summary>Gets the current selection, or null when there is none in the editor.</summary>
    Task<SelectionInfo?> GetSelectionAsync(CancellationToken cancellationToken = default);

    /// <summary>Remembers the current selection.</summary>
    Task SaveSelectionAsync(CancellationToken cancellationToken = default);

    /// <summary>Restores the selection saved by <see cref="SaveSelectionAsync"/>.</summary>
    Task RestoreSelectionAsync(CancellationToken cancellationToken = default);

    /// <summary>Forgets the saved selection.</summary>
    Task ClearSavedSelectionAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets the link at the cursor, or null.</summary>
    Task<LinkInfo?> GetLinkAtCursorAsync(CancellationToken cancellationToken = default);

    /// <summary>Selects the whole link at the cursor.</summary>
    Task SelectLinkAtCursorAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets the selected image, or null.</summary>
    Task<ImageInfo?> GetImageAtCursorAsync(CancellationToken cancellationToken = default);

    /// <summary>Selects the image at the cursor.</summary>
    Task SelectImageAtCursorAsync(CancellationToken cancellationToken = default);

    /// <summary>Undoes the last change.</summary>
    Task UndoAsync(CancellationToken cancellationToken = default);

    /// <summary>Redoes the last undone change.</summary>
    Task RedoAsync(CancellationToken cancellationToken = default);

    /// <summary>Moves focus into the editor.</summary>
    Task FocusAsync(CancellationToken cancellationToken = default);

    /// <summary>Switches between the rich view and the HTML source view.</summary>
    Task ToggleSourceViewAsync(CancellationToken cancellationToken = default);

    /// <summary>Makes the editor fill the browser window, or returns it to the page.</summary>
    Task SetFullscreenAsync(bool fullscreen, CancellationToken cancellationToken = default);

    /// <summary>Turns plain-text paste on or off.</summary>
    Task SetPlainTextPasteAsync(bool plainText, CancellationToken cancellationToken = default);

    /// <summary>Selects the next match of <paramref name="text"/> after the selection, wrapping to the start.</summary>
    /// <param name="text">The text to find.</param>
    /// <param name="matchCase">Whether the case must match.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    Task<FindResult> FindAsync(string text, bool matchCase = false, CancellationToken cancellationToken = default);

    /// <summary>Replaces the selected match of <paramref name="text"/> as one undoable step, then selects the next match.</summary>
    /// <param name="text">The text to find.</param>
    /// <param name="replacement">The replacement text.</param>
    /// <param name="matchCase">Whether the case must match.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    Task<FindResult> ReplaceAsync(string text, string replacement, bool matchCase = false, CancellationToken cancellationToken = default);

    /// <summary>Replaces every match of <paramref name="text"/> as one undoable step.</summary>
    /// <param name="text">The text to find.</param>
    /// <param name="replacement">The replacement text.</param>
    /// <param name="matchCase">Whether the case must match.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <returns>The number of matches replaced.</returns>
    Task<int> ReplaceAllAsync(string text, string replacement, bool matchCase = false, CancellationToken cancellationToken = default);

    /// <summary>Changes the theme and raises <c>ThemeChanged</c>.</summary>
    Task SetThemeAsync(Theme theme, CancellationToken cancellationToken = default);

    /// <summary>Opens a panel in the slide-out.</summary>
    /// <typeparam name="TPanel">The panel component.</typeparam>
    /// <param name="parameters">Parameters passed to the panel.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    Task OpenPanelAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TPanel>(
        IReadOnlyDictionary<string, object?>? parameters = null,
        CancellationToken cancellationToken = default)
        where TPanel : PanelBase;

    /// <summary>Opens a panel chosen at runtime in the slide-out.</summary>
    /// <param name="panelType">A concrete component type derived from <see cref="PanelBase"/>.</param>
    /// <param name="parameters">Parameters passed to the panel.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <exception cref="ArgumentException"><paramref name="panelType"/> is not a concrete <see cref="PanelBase"/>.</exception>
    Task OpenPanelAsync(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type panelType,
        IReadOnlyDictionary<string, object?>? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>Closes the open panel and returns focus to the editor.</summary>
    Task ClosePanelAsync(CancellationToken cancellationToken = default);

    /// <summary>Shows a short message in the editor.</summary>
    Task ShowToastAsync(string message, ToastType type = ToastType.Info, CancellationToken cancellationToken = default);
}
