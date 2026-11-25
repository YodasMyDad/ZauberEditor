using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Interop;

internal sealed class EditorInterop(IJSObjectReference instance, ILogger logger) : IEditorInterop
{
    private const DynamicallyAccessedMemberTypes JsonSerialized =
        DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties;

    public static async Task<TValue?> GuardAsync<TValue>(
        Func<string, CancellationToken, object?[]?, ValueTask<TValue>> invoke,
        string identifier,
        object?[] args,
        ILogger logger,
        CancellationToken cancellationToken,
        bool surfaceDisconnect = false)
    {
        try
        {
            return await invoke(identifier, cancellationToken, args);
        }
        catch (JSDisconnectedException) when (!surfaceDisconnect)
        {
            return default;
        }
        catch (JSException exception) when (IsEmptyStream(exception))
        {
            return default;
        }
        catch (JSException exception)
        {
            logger.InvocationFailed(identifier, exception);
            throw;
        }
    }

    private static bool IsEmptyStream(JSException exception) =>
        exception.InnerException is ArgumentOutOfRangeException { ParamName: "totalLength" };

    public Task ResetHtmlAsync(string html, int revision, CancellationToken cancellationToken) =>
        InvokeVoidAsync("setHtml", cancellationToken, html, "parameter", revision);

    public Task SetHtmlAsync(string html, CancellationToken cancellationToken) =>
        InvokeVoidAsync("setHtml", cancellationToken, html, "api");

    public Task FlushAsync(CancellationToken cancellationToken) => InvokeVoidAsync("flush", cancellationToken);

    public Task<string> GetHtmlAsync(long maxBytes, CancellationToken cancellationToken) =>
        ReadStreamAsync("getHtmlStream", maxBytes, cancellationToken);

    public Task<string> GetTextAsync(long maxBytes, CancellationToken cancellationToken) =>
        ReadStreamAsync("getTextStream", maxBytes, cancellationToken);

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties, typeof(EditorSnapshot))]
    public Task<EditorSnapshot?> GetStateAsync(CancellationToken cancellationToken) =>
        InvokeAsync<EditorSnapshot>("getState", cancellationToken);

    public Task ToggleMarkAsync(string tag, CancellationToken cancellationToken) =>
        InvokeVoidAsync("toggleMark", cancellationToken, tag);

    public Task WrapAsync(string tag, IReadOnlyDictionary<string, string>? attributes, CancellationToken cancellationToken) =>
        InvokeVoidAsync("wrap", cancellationToken, tag, attributes);

    public Task UnwrapAsync(string tag, CancellationToken cancellationToken) =>
        InvokeVoidAsync("unwrap", cancellationToken, tag);

    public Task SetBlockTypeAsync(string blockType, IReadOnlyDictionary<string, string>? attributes, CancellationToken cancellationToken) =>
        InvokeVoidAsync("setBlockType", cancellationToken, blockType, attributes);

    public Task SetBlockStyleAsync(IReadOnlyDictionary<string, string> styles, CancellationToken cancellationToken) =>
        InvokeVoidAsync("setBlockStyle", cancellationToken, styles);

    public Task IndentAsync(CancellationToken cancellationToken) => InvokeVoidAsync("indent", cancellationToken);

    public Task OutdentAsync(CancellationToken cancellationToken) => InvokeVoidAsync("outdent", cancellationToken);

    public Task SetTextStyleAsync(string styleName, string? value, CancellationToken cancellationToken) =>
        InvokeVoidAsync("setTextStyle", cancellationToken, styleName, value);

    public Task ClearFormattingAsync(CancellationToken cancellationToken) =>
        InvokeVoidAsync("clearFormatting", cancellationToken);

    public Task InsertHtmlAsync(string html, CancellationToken cancellationToken) =>
        InvokeVoidAsync("insertHtml", cancellationToken, html);

    public Task InsertTextAsync(string text, CancellationToken cancellationToken) =>
        InvokeVoidAsync("insertText", cancellationToken, text);

    public Task UndoAsync(CancellationToken cancellationToken) => InvokeVoidAsync("undo", cancellationToken);

    public Task RedoAsync(CancellationToken cancellationToken) => InvokeVoidAsync("redo", cancellationToken);

    public Task SaveSelectionAsync(CancellationToken cancellationToken) =>
        InvokeVoidAsync("saveSelection", cancellationToken);

    public Task RestoreSelectionAsync(CancellationToken cancellationToken) =>
        InvokeVoidAsync("restoreSelection", cancellationToken);

    public Task ClearSavedSelectionAsync(CancellationToken cancellationToken) =>
        InvokeVoidAsync("clearSavedSelection", cancellationToken);

    public Task<SelectionInfo?> GetSelectionAsync(long maxBytes, CancellationToken cancellationToken) =>
        ReadJsonAsync("getSelectionStream", InteropJsonContext.Default.SelectionInfo, maxBytes, cancellationToken);

    public Task<LinkInfo?> GetLinkAsync(long maxBytes, CancellationToken cancellationToken) =>
        ReadJsonAsync("getLinkStream", InteropJsonContext.Default.LinkInfo, maxBytes, cancellationToken);

    public Task SelectLinkAsync(CancellationToken cancellationToken) => InvokeVoidAsync("selectLink", cancellationToken);

    public Task<ImageInfo?> GetImageAsync(long maxBytes, CancellationToken cancellationToken) =>
        ReadJsonAsync("getImageStream", InteropJsonContext.Default.ImageInfo, maxBytes, cancellationToken);

    public Task SelectImageAsync(CancellationToken cancellationToken) => InvokeVoidAsync("selectImage", cancellationToken);

    public Task FocusAsync(CancellationToken cancellationToken) => InvokeVoidAsync("focus", cancellationToken);

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(EditorInitOptions))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(HtmlPolicyOptions))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(ImageOptions))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(ShortcutBinding))]
    public Task ConfigureAsync(EditorInitOptions options, CancellationToken cancellationToken) =>
        InvokeVoidAsync("configure", cancellationToken, options);

    public Task SetFullscreenAsync(bool fullscreen, CancellationToken cancellationToken) =>
        InvokeVoidAsync("setFullscreen", cancellationToken, fullscreen);

    public Task SetPlainTextPasteAsync(bool plainText, CancellationToken cancellationToken) =>
        InvokeVoidAsync("setPlainTextPaste", cancellationToken, plainText);

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties, typeof(FindResult))]
    public async Task<FindResult> FindAsync(string text, bool matchCase, CancellationToken cancellationToken) =>
        await InvokeAsync<FindResult>("find", cancellationToken, text, matchCase) ?? FindResult.None;

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties, typeof(FindResult))]
    public async Task<FindResult> ReplaceAsync(string text, string replacement, bool matchCase, CancellationToken cancellationToken) =>
        await InvokeAsync<FindResult>("replace", cancellationToken, text, replacement, matchCase) ?? FindResult.None;

    public Task<int> ReplaceAllAsync(string text, string replacement, bool matchCase, CancellationToken cancellationToken) =>
        InvokeAsync<int>("replaceAll", cancellationToken, text, replacement, matchCase);

    public Task EnterSourceViewAsync(CancellationToken cancellationToken) =>
        InvokeVoidAsync("enterSourceView", cancellationToken);

    public Task ExitSourceViewAsync(CancellationToken cancellationToken) =>
        InvokeVoidAsync("exitSourceView", cancellationToken);

    public Task OpenPanelAsync(ElementReference panel, CancellationToken cancellationToken) =>
        InvokeVoidAsync("openPanel", cancellationToken, panel);

    public Task ClosePanelAsync(CancellationToken cancellationToken) => InvokeVoidAsync("closePanel", cancellationToken);

    public async ValueTask DisposeAsync()
    {
        try
        {
            try
            {
                await instance.InvokeVoidAsync("dispose");
            }
            finally
            {
                await instance.DisposeAsync();
            }
        }
        catch (JSDisconnectedException)
        {
            return;
        }
        catch (JSException exception)
        {
            logger.DisposeFailed(exception);
        }
    }

    private Task<TValue?> InvokeAsync<[DynamicallyAccessedMembers(JsonSerialized)] TValue>(
        string identifier,
        CancellationToken cancellationToken,
        params object?[] args) =>
        GuardAsync(instance.InvokeAsync<TValue>, identifier, args, logger, cancellationToken);

    private async Task InvokeVoidAsync(string identifier, CancellationToken cancellationToken, params object?[] args) =>
        await InvokeAsync<IJSVoidResult>(identifier, cancellationToken, args);

    private async Task<TValue?> ReadJsonAsync<TValue>(
        string identifier,
        JsonTypeInfo<TValue> typeInfo,
        long maxBytes,
        CancellationToken cancellationToken)
        where TValue : class =>
        await ReadStreamAsync(identifier, maxBytes, cancellationToken) is { Length: > 0 } json
            ? JsonSerializer.Deserialize(json, typeInfo)
            : null;

    private async Task<string> ReadStreamAsync(string identifier, long maxBytes, CancellationToken cancellationToken)
    {
        var reference = await GuardAsync(instance.InvokeAsync<IJSStreamReference>, identifier, [], logger, cancellationToken, surfaceDisconnect: true);
        if (reference is null)
        {
            return string.Empty;
        }

        await using (reference)
        {
            if (reference.Length > maxBytes)
            {
                logger.ContentTooLarge(reference.Length, maxBytes);
                throw new EditorContentTooLargeException(reference.Length, maxBytes);
            }

            await using var stream = await reference.OpenReadStreamAsync(maxBytes, cancellationToken);
            using var reader = new StreamReader(stream, Encoding.UTF8);
            return await reader.ReadToEndAsync(cancellationToken);
        }
    }
}
