using Microsoft.AspNetCore.Components;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Interop;

internal interface IEditorInterop : IAsyncDisposable
{
    Task ResetHtmlAsync(string html, int revision, CancellationToken cancellationToken);

    Task SetHtmlAsync(string html, CancellationToken cancellationToken);

    Task FlushAsync(CancellationToken cancellationToken);

    Task<string> GetHtmlAsync(long maxBytes, CancellationToken cancellationToken);

    Task<string> GetTextAsync(long maxBytes, CancellationToken cancellationToken);

    Task<EditorSnapshot?> GetStateAsync(CancellationToken cancellationToken);

    Task ToggleMarkAsync(string tag, CancellationToken cancellationToken);

    Task WrapAsync(string tag, IReadOnlyDictionary<string, string>? attributes, CancellationToken cancellationToken);

    Task UnwrapAsync(string tag, CancellationToken cancellationToken);

    Task SetBlockTypeAsync(string blockType, IReadOnlyDictionary<string, string>? attributes, CancellationToken cancellationToken);

    Task SetBlockStyleAsync(IReadOnlyDictionary<string, string> styles, CancellationToken cancellationToken);

    Task IndentAsync(CancellationToken cancellationToken);

    Task OutdentAsync(CancellationToken cancellationToken);

    Task SetTextStyleAsync(string styleName, string? value, CancellationToken cancellationToken);

    Task ClearFormattingAsync(CancellationToken cancellationToken);

    Task InsertHtmlAsync(string html, CancellationToken cancellationToken);

    Task InsertTextAsync(string text, CancellationToken cancellationToken);

    Task UndoAsync(CancellationToken cancellationToken);

    Task RedoAsync(CancellationToken cancellationToken);

    Task SaveSelectionAsync(CancellationToken cancellationToken);

    Task RestoreSelectionAsync(CancellationToken cancellationToken);

    Task ClearSavedSelectionAsync(CancellationToken cancellationToken);

    Task<SelectionInfo?> GetSelectionAsync(long maxBytes, CancellationToken cancellationToken);

    Task<LinkInfo?> GetLinkAsync(long maxBytes, CancellationToken cancellationToken);

    Task SelectLinkAsync(CancellationToken cancellationToken);

    Task<ImageInfo?> GetImageAsync(long maxBytes, CancellationToken cancellationToken);

    Task SelectImageAsync(CancellationToken cancellationToken);

    Task FocusAsync(CancellationToken cancellationToken);

    Task ConfigureAsync(EditorInitOptions options, CancellationToken cancellationToken);

    Task SetFullscreenAsync(bool fullscreen, CancellationToken cancellationToken);

    Task SetPlainTextPasteAsync(bool plainText, CancellationToken cancellationToken);

    Task<FindResult> FindAsync(string text, bool matchCase, CancellationToken cancellationToken);

    Task<FindResult> ReplaceAsync(string text, string replacement, bool matchCase, CancellationToken cancellationToken);

    Task<int> ReplaceAllAsync(string text, string replacement, bool matchCase, CancellationToken cancellationToken);

    Task EnterSourceViewAsync(CancellationToken cancellationToken);

    Task ExitSourceViewAsync(CancellationToken cancellationToken);

    Task OpenPanelAsync(ElementReference panel, CancellationToken cancellationToken);

    Task ClosePanelAsync(CancellationToken cancellationToken);
}
