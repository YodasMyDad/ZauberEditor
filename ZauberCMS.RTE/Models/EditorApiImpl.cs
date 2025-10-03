using ZauberCMS.RTE.Components;
using ZauberCMS.RTE.Services;

namespace ZauberCMS.RTE.Models;

/// <summary>
/// Implementation of EditorApi for toolbar items to interact with the editor
/// </summary>
internal class EditorApi(ZauberRichTextEditor editor, IZauberJsRuntime jsRuntime, string editorId) : IEditorApi
{
    public EditorState GetState() => editor.GetCurrentState();

    public EditorSettings GetSettings() => editor.GetEditorSettings();

    public async Task<string> GetCurrentBlockTypeAsync() =>
        await jsRuntime.Selection.GetCurrentBlockTypeAsync(editorId) ?? "p";

    public async Task<int> GetCurrentHeadingLevelAsync() =>
        await jsRuntime.Selection.GetCurrentHeadingLevelAsync(editorId) ?? 0;

    public async Task InsertHtmlAsync(string html) =>
        await jsRuntime.Selection.InsertHtmlAsync(editorId, html);

    public async Task InsertTextAsync(string text) =>
        await jsRuntime.Selection.InsertHtmlAsync(editorId, System.Net.WebUtility.HtmlEncode(text));

    public async Task WrapSelectionAsync(string tagName, Dictionary<string, string>? attributes = null) =>
        await jsRuntime.Selection.WrapSelectionAsync(editorId, tagName, attributes);

    public async Task UnwrapSelectionAsync(string tagName) =>
        await jsRuntime.Selection.UnwrapSelectionAsync(editorId, tagName);

    public async Task ReplaceSelectionAsync(string html) =>
        await jsRuntime.Selection.InsertHtmlAsync(editorId, html);

    public async Task ToggleMarkAsync(string markName) =>
        await jsRuntime.Selection.ToggleMarkAsync(editorId, markName);

    public async Task SetBlockTypeAsync(string blockType, Dictionary<string, string>? attributes = null) =>
        await jsRuntime.Selection.SetBlockTypeAsync(editorId, blockType, attributes);

    public async Task SetBlockStyleAsync(Dictionary<string, string> styles) =>
        await jsRuntime.Selection.SetBlockStyleAsync(editorId, styles);

    public async Task OpenPanelAsync(Type componentType, object? parameters = null) =>
        await editor.OpenPanelAsync(componentType, parameters);

    public async Task ClosePanelAsync() =>
        await editor.ClosePanelAsync();

    public async Task ShowToastAsync(string message, ToastType type = ToastType.Info) =>
        await editor.ShowToastAsync(message, type);

    public SelectionInfo? GetSelection() =>
        editor.GetCurrentSelection();

    public async Task<SelectionInfo?> GetSelectionAsync() =>
        await jsRuntime.Selection.GetSelectionAsync(editorId);

    public async Task FocusAsync() =>
        await editor.FocusAsync();

    public async Task BlurAsync() =>
        await jsRuntime.Mutation.GetHtmlAsync(editorId); // Placeholder

    public async Task UndoAsync() =>
        await jsRuntime.History.UndoAsync(editorId);

    public async Task RedoAsync() =>
        await jsRuntime.History.RedoAsync(editorId);

    public async Task ClearFormattingAsync() =>
        await jsRuntime.Selection.ClearFormattingAsync(editorId);

    public async Task CleanHtmlAsync() =>
        await jsRuntime.Selection.CleanHtmlAsync(editorId);

    public string? GetHtml() =>
        editor.GetCurrentHtml();

    public string? GetText() =>
        editor.GetCurrentText();

    public async Task SetHtmlAsync(string html) =>
        await editor.SetContentAsync(html);

    public async Task SetTextAsync(string text) =>
        await editor.SetContentAsync(System.Net.WebUtility.HtmlEncode(text));

    public async Task ToggleSourceViewAsync() =>
        await editor.ToggleSourceViewAsync();

    public async Task SaveSelectionRangeAsync() =>
        await jsRuntime.Selection.SaveRangeAsync(editorId);

    public async Task<bool> RestoreSelectionRangeAsync() =>
        await jsRuntime.Selection.RestoreRangeAsync(editorId);

    public async Task ClearSavedSelectionRangeAsync() =>
        await jsRuntime.Selection.ClearSavedRangeAsync(editorId);

    public async Task<LinkInfo?> GetLinkAtCursorAsync() =>
        await jsRuntime.Selection.GetLinkAtCursorAsync(editorId);

    public async Task<bool> SelectLinkAtCursorAsync() =>
        await jsRuntime.Selection.SelectLinkAtCursorAsync(editorId);

    public async Task<ImageInfo?> GetImageAtCursorAsync() =>
        await jsRuntime.Selection.GetImageAtCursorAsync(editorId);

    public async Task<bool> SelectImageAtCursorAsync() =>
        await jsRuntime.Selection.SelectImageAtCursorAsync(editorId);
}
