using System.Reflection;
using Microsoft.JSInterop;
using ZauberCMS.RTE.Interop;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Tests.Interop;

public sealed class EditorCallbacksTests
{
    private readonly IEditorEventSink _sink = Substitute.For<IEditorEventSink>();
    private readonly EditorCallbacks _callbacks;

    public EditorCallbacksTests() => _callbacks = new(_sink);

    [Theory]
    [InlineData("input", ChangeSource.Input)]
    [InlineData("paste", ChangeSource.Paste)]
    [InlineData("command", ChangeSource.Command)]
    [InlineData("history", ChangeSource.History)]
    [InlineData("api", ChangeSource.Api)]
    [InlineData("sourceView", ChangeSource.SourceView)]
    [InlineData("imageResize", ChangeSource.ImageResize)]
    [InlineData("something-new", ChangeSource.Input)]
    [InlineData(null, ChangeSource.Input)]
    public void SourceNamesAreParsed(string? source, ChangeSource expected) =>
        EditorCallbacks.ParseSource(source).ShouldBe(expected);

    [Fact]
    public void ScriptCallbackNamesMatchTheContract() =>
        typeof(EditorCallbacks)
            .GetMethods()
            .Select(method => method.GetCustomAttribute<JSInvokableAttribute>()?.Identifier)
            .OfType<string>()
            .ShouldBe(["OnContentChanged", "OnStateChanged", "OnShortcut", "OnFocusChanged", "OnImageResized", "OnPanelEscape", "OnError", "OnInputRejected"], ignoreOrder: true);

    [Fact]
    public async Task ContentChangesAreForwardedWithTheirSource()
    {
        await _callbacks.OnContentChangedAsync("<p>x</p>", "paste", 3);

        await _sink.Received(1).ContentChangedAsync("<p>x</p>", ChangeSource.Paste, 3);
    }

    [Fact]
    public async Task ContentTooLargeToSendInlineIsForwardedAsNull()
    {
        await _callbacks.OnContentChangedAsync(null, "input", 0);

        await _sink.Received(1).ContentChangedAsync(null, ChangeSource.Input, 0);
    }

    [Fact]
    public async Task OtherCallbacksAreForwarded()
    {
        var snapshot = new EditorSnapshot(["em"], "p", "left", false, true, false, false, false, false, false, false);

        await _callbacks.OnStateChangedAsync(snapshot);
        await _callbacks.OnShortcutAsync("bold");
        await _callbacks.OnFocusChangedAsync(true);
        await _callbacks.OnImageResizedAsync(320, 200);
        await _callbacks.OnPanelEscapeAsync();
        _callbacks.OnError("broken");
        await _callbacks.OnInputRejectedAsync("too big");

        Received.InOrder(() =>
        {
            _sink.StateChangedAsync(snapshot);
            _sink.ShortcutAsync("bold");
            _sink.FocusChangedAsync(true);
            _sink.ImageResizedAsync(320, 200);
            _sink.PanelEscapeAsync();
            _sink.ScriptError("broken");
            _sink.InputRejectedAsync("too big");
        });
    }
}
