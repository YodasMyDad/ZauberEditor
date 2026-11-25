using System.Diagnostics.CodeAnalysis;
using Microsoft.JSInterop;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Interop;

internal sealed class EditorCallbacks(IEditorEventSink sink)
{
    [JSInvokable("OnContentChanged")]
    public Task OnContentChangedAsync(string? html, string source, int revision) =>
        sink.ContentChangedAsync(html, ParseSource(source), revision);

    [JSInvokable("OnStateChanged")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties, typeof(EditorSnapshot))]
    public Task OnStateChangedAsync(EditorSnapshot snapshot) => sink.StateChangedAsync(snapshot);

    [JSInvokable("OnShortcut")]
    public Task OnShortcutAsync(string itemId) => sink.ShortcutAsync(itemId);

    [JSInvokable("OnFocusChanged")]
    public Task OnFocusChangedAsync(bool focused) => sink.FocusChangedAsync(focused);

    [JSInvokable("OnImageResized")]
    public Task OnImageResizedAsync(int width, int height) => sink.ImageResizedAsync(width, height);

    [JSInvokable("OnPanelEscape")]
    public Task OnPanelEscapeAsync() => sink.PanelEscapeAsync();

    [JSInvokable("OnError")]
    public void OnError(string message) => sink.ScriptError(message);

    [JSInvokable("OnInputRejected")]
    public Task OnInputRejectedAsync(string message) => sink.InputRejectedAsync(message);

    internal static ChangeSource ParseSource(string? source) => source switch
    {
        "paste" => ChangeSource.Paste,
        "command" => ChangeSource.Command,
        "history" => ChangeSource.History,
        "api" => ChangeSource.Api,
        "sourceView" => ChangeSource.SourceView,
        "imageResize" => ChangeSource.ImageResize,
        _ => ChangeSource.Input
    };
}
