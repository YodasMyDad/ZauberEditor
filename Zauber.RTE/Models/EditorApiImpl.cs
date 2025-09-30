using Zauber.RTE.Services;
using Zauber.RTE.Components;

namespace Zauber.RTE.Models;

/// <summary>
/// Implementation of EditorApi for toolbar items to interact with the editor
/// </summary>
internal class EditorApiImpl(ZauberRichTextEditor editor, IZauberJsRuntime jsRuntime, string editorId) : EditorApi
{
    private readonly ZauberRichTextEditor _editor = editor;
    private readonly IZauberJsRuntime _jsRuntime = jsRuntime;
    private readonly string _editorId = editorId;

    public EditorState GetState() => _editor.GetCurrentState();

    public async Task InsertHtmlAsync(string html) =>
        await _jsRuntime.Selection.InsertHtmlAsync(_editorId, html);

    public async Task InsertTextAsync(string text) =>
        await _jsRuntime.Selection.InsertHtmlAsync(_editorId, System.Net.WebUtility.HtmlEncode(text));

    public async Task WrapSelectionAsync(string tagName, Dictionary<string, string>? attributes = null) =>
        await _jsRuntime.Selection.WrapSelectionAsync(_editorId, tagName, attributes);

    public async Task UnwrapSelectionAsync(string tagName) =>
        await _jsRuntime.Selection.UnwrapSelectionAsync(_editorId, tagName);

    public async Task ReplaceSelectionAsync(string html) =>
        await _jsRuntime.Selection.InsertHtmlAsync(_editorId, html);

    public async Task ToggleMarkAsync(string markName) =>
        await _jsRuntime.Selection.ToggleMarkAsync(_editorId, markName);

    public async Task SetBlockTypeAsync(string blockType, Dictionary<string, string>? attributes = null) =>
        await _jsRuntime.Selection.SetBlockTypeAsync(_editorId, blockType, attributes);

    public async Task SetBlockStyleAsync(Dictionary<string, string> styles) =>
        await _jsRuntime.Selection.SetBlockStyleAsync(_editorId, styles);

    public async Task OpenPanelAsync(Type componentType, object? parameters = null) =>
        await _editor.OpenPanelAsync(componentType, parameters);

    public async Task ClosePanelAsync() =>
        await _editor.ClosePanelAsync();

    public async Task ShowToastAsync(string message, ToastType type = ToastType.Info) =>
        await _editor.ShowToastAsync(message, type);

    public SelectionInfo? GetSelection() =>
        _editor.GetCurrentSelection();

    public async Task<SelectionInfo?> GetSelectionAsync() =>
        await _jsRuntime.Selection.GetSelectionAsync(_editorId);

    public async Task FocusAsync() =>
        await _editor.FocusAsync();

    public async Task BlurAsync() =>
        await _jsRuntime.Mutation.GetHtmlAsync(_editorId); // Placeholder

    public async Task UndoAsync() =>
        await _jsRuntime.History.UndoAsync(_editorId);

    public async Task RedoAsync() =>
        await _jsRuntime.History.RedoAsync(_editorId);

    public async Task ClearFormattingAsync() =>
        await _jsRuntime.Selection.ClearFormattingAsync(_editorId);

    public string? GetHtml() =>
        _editor.GetCurrentHtml();

    public string? GetText() =>
        _editor.GetCurrentText();

    public async Task SetHtmlAsync(string html) =>
        await _editor.SetContentAsync(html);

    public async Task SetTextAsync(string text) =>
        await _editor.SetContentAsync(System.Net.WebUtility.HtmlEncode(text));

    public async Task ToggleSourceViewAsync() =>
        await _editor.ToggleSourceViewAsync();

    public async Task SaveSelectionRangeAsync() =>
        await _jsRuntime.Selection.SaveRangeAsync(_editorId);

    public async Task<bool> RestoreSelectionRangeAsync() =>
        await _jsRuntime.Selection.RestoreRangeAsync(_editorId);

    public async Task ClearSavedSelectionRangeAsync() =>
        await _jsRuntime.Selection.ClearSavedRangeAsync(_editorId);

    public async Task<LinkInfo?> GetLinkAtCursorAsync() =>
        await _jsRuntime.Selection.GetLinkAtCursorAsync(_editorId);

    public async Task<bool> SelectLinkAtCursorAsync() =>
        await _jsRuntime.Selection.SelectLinkAtCursorAsync(_editorId);
}
