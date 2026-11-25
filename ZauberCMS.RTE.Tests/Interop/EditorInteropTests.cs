using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using ZauberCMS.RTE.Interop;
using ZauberCMS.RTE.Models;
using ZauberCMS.RTE.Tests.TestSupport;

namespace ZauberCMS.RTE.Tests.Interop;

public sealed class EditorInteropTests : IDisposable
{
    private static readonly Dictionary<string, string> Attributes = new() { ["class"] = "note" };
    private static readonly Dictionary<string, string> Styles = new() { ["text-align"] = "right" };
    private static readonly ElementReference Panel = new("panel");
    private static readonly EditorInitOptions Options = EditorInitOptions.Create(null, true, null, new EditorSettings(), []);

    private static readonly Dictionary<string, (Func<IEditorInterop, CancellationToken, Task> Call, string Identifier, object?[] Arguments)> Calls = new()
    {
        ["reset"] = ((interop, token) => interop.ResetHtmlAsync("<p>a</p>", 4, token), "setHtml", ["<p>a</p>", "parameter", 4]),
        ["set"] = ((interop, token) => interop.SetHtmlAsync("<p>b</p>", token), "setHtml", ["<p>b</p>", "api"]),
        ["state"] = ((interop, token) => interop.GetStateAsync(token), "getState", []),
        ["toggleMark"] = ((interop, token) => interop.ToggleMarkAsync("em", token), "toggleMark", ["em"]),
        ["wrap"] = ((interop, token) => interop.WrapAsync("span", Attributes, token), "wrap", ["span", Attributes]),
        ["unwrap"] = ((interop, token) => interop.UnwrapAsync("a", token), "unwrap", ["a"]),
        ["blockType"] = ((interop, token) => interop.SetBlockTypeAsync("h2", null, token), "setBlockType", ["h2", null]),
        ["blockStyle"] = ((interop, token) => interop.SetBlockStyleAsync(Styles, token), "setBlockStyle", [Styles]),
        ["clear"] = ((interop, token) => interop.ClearFormattingAsync(token), "clearFormatting", []),
        ["insertHtml"] = ((interop, token) => interop.InsertHtmlAsync("<hr>", token), "insertHtml", ["<hr>"]),
        ["insertText"] = ((interop, token) => interop.InsertTextAsync("x", token), "insertText", ["x"]),
        ["undo"] = ((interop, token) => interop.UndoAsync(token), "undo", []),
        ["redo"] = ((interop, token) => interop.RedoAsync(token), "redo", []),
        ["save"] = ((interop, token) => interop.SaveSelectionAsync(token), "saveSelection", []),
        ["restore"] = ((interop, token) => interop.RestoreSelectionAsync(token), "restoreSelection", []),
        ["forget"] = ((interop, token) => interop.ClearSavedSelectionAsync(token), "clearSavedSelection", []),
        ["selectLink"] = ((interop, token) => interop.SelectLinkAsync(token), "selectLink", []),
        ["selectImage"] = ((interop, token) => interop.SelectImageAsync(token), "selectImage", []),
        ["focus"] = ((interop, token) => interop.FocusAsync(token), "focus", []),
        ["configure"] = ((interop, token) => interop.ConfigureAsync(Options, token), "configure", [Options]),
        ["enterSource"] = ((interop, token) => interop.EnterSourceViewAsync(token), "enterSourceView", []),
        ["exitSource"] = ((interop, token) => interop.ExitSourceViewAsync(token), "exitSourceView", []),
        ["openPanel"] = ((interop, token) => interop.OpenPanelAsync(Panel, token), "openPanel", [Panel]),
        ["closePanel"] = ((interop, token) => interop.ClosePanelAsync(token), "closePanel", [])
    };

    private readonly IJSObjectReference _instance = Substitute.For<IJSObjectReference>();
    private readonly ILogger _logger = Substitute.For<ILogger>();
    private readonly CancellationTokenSource _cancellation = new();
    private readonly EditorInterop _interop;

    public EditorInteropTests()
    {
        _logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        _interop = new(_instance, _logger);
    }

    private static readonly Dictionary<string, (Func<IEditorInterop, long, CancellationToken, Task> Read, string Identifier)> Reads = new()
    {
        [nameof(IEditorInterop.GetHtmlAsync)] = ((interop, max, token) => interop.GetHtmlAsync(max, token), "getHtmlStream"),
        [nameof(IEditorInterop.GetTextAsync)] = ((interop, max, token) => interop.GetTextAsync(max, token), "getTextStream"),
        [nameof(IEditorInterop.GetSelectionAsync)] = ((interop, max, token) => interop.GetSelectionAsync(max, token), "getSelectionStream"),
        [nameof(IEditorInterop.GetLinkAsync)] = ((interop, max, token) => interop.GetLinkAsync(max, token), "getLinkStream"),
        [nameof(IEditorInterop.GetImageAsync)] = ((interop, max, token) => interop.GetImageAsync(max, token), "getImageStream")
    };

    public static TheoryData<string> CallNames => [.. Calls.Keys];

    public static TheoryData<string> ReadNames => [.. Reads.Keys];

    public void Dispose() => _cancellation.Dispose();

    [Theory]
    [MemberData(nameof(CallNames))]
    public async Task EachOperationCallsTheMatchingScriptMethod(string name)
    {
        var (call, identifier, arguments) = Calls[name];

        await call(_interop, _cancellation.Token);

        var received = _instance.ReceivedCalls().ShouldHaveSingleItem().GetArguments();
        received[0].ShouldBe(identifier);
        received[1].ShouldBe(_cancellation.Token);
        received[2].ShouldBeOfType<object?[]>().ShouldBe(arguments);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ContentIsReadAsUtf8FromAStream(bool html)
    {
        var stream = StreamOf("<p>héllo 👋</p>");
        Returns(html ? "getHtmlStream" : "getTextStream", stream);

        var content = html
            ? await _interop.GetHtmlAsync(1024, _cancellation.Token)
            : await _interop.GetTextAsync(1024, _cancellation.Token);

        content.ShouldBe("<p>héllo 👋</p>");
        await stream.Received(1).OpenReadStreamAsync(1024, _cancellation.Token);
        await stream.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task ContentOverTheLimitIsReportedAndNotTransferred()
    {
        var stream = StreamOf(new string('x', 2048));
        Returns("getHtmlStream", stream);

        var exception = await Should.ThrowAsync<EditorContentTooLargeException>(() => _interop.GetHtmlAsync(1024, _cancellation.Token));

        (exception.Length, exception.MaxBytes).ShouldBe((2048L, 1024L));
        await stream.DidNotReceive().OpenReadStreamAsync(Arg.Any<long>(), Arg.Any<CancellationToken>());
        await stream.Received(1).DisposeAsync();
        LoggedCount().ShouldBe(1);
    }

    [Theory]
    [MemberData(nameof(ReadNames))]
    public async Task EveryReadIsStreamedSoNoReturnValueHitsTheMessageSizeLimit(string name)
    {
        var (read, identifier) = Reads[name];
        Returns(identifier, StreamOf(string.Empty));

        await read(_interop, 1024, _cancellation.Token);

        var received = _instance.ReceivedCalls().ShouldHaveSingleItem();
        received.GetMethodInfo().GetGenericArguments().ShouldBe([typeof(IJSStreamReference)]);
        received.GetArguments()[0].ShouldBe(identifier);
        received.GetArguments()[2].ShouldBeOfType<object?[]>().ShouldBeEmpty();
    }

    [Fact]
    public void OnlyTheBoundedStateSnapshotIsReturnedInline() =>
        typeof(IEditorInterop).GetMethods()
            .Where(method => method.ReturnType.IsGenericType)
            .Select(method => method.Name)
            .Except(Reads.Keys)
            .ShouldBe([nameof(IEditorInterop.GetStateAsync)]);

    [Theory]
    [MemberData(nameof(ReadNames))]
    public async Task ReadsOverTheLimitAreReported(string name)
    {
        var (read, identifier) = Reads[name];
        Returns(identifier, StreamOf(new string('x', 2048)));

        await Should.ThrowAsync<EditorContentTooLargeException>(() => read(_interop, 1024, _cancellation.Token));
    }

    [Fact]
    public async Task SelectionLinkAndImageAreReadAsJson()
    {
        var src = $"data:image/png;base64,{new string('A', 40_000)}";
        Returns("getSelectionStream", StreamOf("""{"text":"héllo","isCollapsed":false}"""));
        Returns("getLinkStream", StreamOf("""{"href":"https://example.com","text":"Example","title":null,"target":"_blank"}"""));
        Returns("getImageStream", StreamOf($$"""{"src":"{{src}}","alt":"A","title":null,"width":10,"height":null}"""));

        (await _interop.GetSelectionAsync(100_000, _cancellation.Token)).ShouldBe(new SelectionInfo("héllo", false));
        (await _interop.GetLinkAsync(100_000, _cancellation.Token)).ShouldBe(new LinkInfo("https://example.com", "Example", null, "_blank"));
        (await _interop.GetImageAsync(100_000, _cancellation.Token)).ShouldBe(new ImageInfo(src, "A", null, 10, null));
    }

    [Fact]
    public async Task EmptyStreamMeansNothingAtTheCursor()
    {
        Returns("getSelectionStream", StreamOf(string.Empty));
        Returns("getLinkStream", StreamOf(string.Empty));
        Returns("getImageStream", StreamOf(string.Empty));

        (await _interop.GetSelectionAsync(1024, _cancellation.Token)).ShouldBeNull();
        (await _interop.GetLinkAsync(1024, _cancellation.Token)).ShouldBeNull();
        (await _interop.GetImageAsync(1024, _cancellation.Token)).ShouldBeNull();
    }

    [Fact]
    public async Task DisconnectedCircuitIsIgnoredByCommandsButSurfacedByReads()
    {
        _instance.InvokeAsync<IJSVoidResult>(Arg.Any<string>(), Arg.Any<CancellationToken>(), Arg.Any<object?[]?>()).Returns(Disconnected<IJSVoidResult>());
        _instance.InvokeAsync<IJSStreamReference>(Arg.Any<string>(), Arg.Any<CancellationToken>(), Arg.Any<object?[]?>()).Returns(Disconnected<IJSStreamReference>());

        await _interop.UndoAsync(_cancellation.Token);
        await Should.ThrowAsync<JSDisconnectedException>(() => _interop.GetHtmlAsync(1024, _cancellation.Token));
        LoggedCount().ShouldBe(0);
    }

    [Fact]
    public async Task ScriptErrorsAreLoggedAndRethrown()
    {
        var failure = new JSException("broken");
        _instance.InvokeAsync<IJSVoidResult>(Arg.Any<string>(), Arg.Any<CancellationToken>(), Arg.Any<object?[]?>()).Returns(ValueTask.FromException<IJSVoidResult>(failure));

        (await Should.ThrowAsync<JSException>(() => _interop.ToggleMarkAsync("em", _cancellation.Token))).ShouldBeSameAs(failure);
        LoggedCount().ShouldBe(1);
    }

    [Fact]
    public async Task DisposeTearsDownTheScriptInstanceThenReleasesIt()
    {
        await _interop.DisposeAsync();

        var calls = _instance.ReceivedCalls().ToArray();
        calls.Select(call => call.GetMethodInfo().Name).ShouldBe(["InvokeAsync", "DisposeAsync"]);
        calls[0].GetArguments()[0].ShouldBe("dispose");
    }

    [Fact]
    public async Task DisposeIgnoresADisconnectedCircuit()
    {
        _instance.InvokeAsync<IJSVoidResult>(Arg.Any<string>(), Arg.Any<object?[]?>()).Returns(Disconnected<IJSVoidResult>());

        await Should.NotThrowAsync(_interop.DisposeAsync().AsTask());
        LoggedCount().ShouldBe(0);
    }

    [Fact]
    public async Task DisposeLogsScriptErrorsAndStillReleasesTheReference()
    {
        _instance.InvokeAsync<IJSVoidResult>(Arg.Any<string>(), Arg.Any<object?[]?>()).Returns(ValueTask.FromException<IJSVoidResult>(new JSException("broken")));

        await Should.NotThrowAsync(_interop.DisposeAsync().AsTask());

        LoggedCount().ShouldBe(1);
        await _instance.Received(1).DisposeAsync();
    }

    private static IJSStreamReference StreamOf(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var stream = Substitute.For<IJSStreamReference>();
        stream.Length.Returns(bytes.Length);
        stream.OpenReadStreamAsync(Arg.Any<long>(), Arg.Any<CancellationToken>()).Returns(_ => new ValueTask<Stream>(new MemoryStream(bytes)));
        return stream;
    }

    private static ValueTask<TValue> Disconnected<TValue>() => ValueTask.FromException<TValue>(new JSDisconnectedException("Circuit gone."));

    private void Returns(string identifier, IJSStreamReference stream) =>
        _instance.InvokeAsync<IJSStreamReference>(identifier, Arg.Any<CancellationToken>(), Arg.Any<object?[]?>()).Returns(new ValueTask<IJSStreamReference>(stream));

    private int LoggedCount() => SubstituteCalls.To(_logger, nameof(ILogger.Log)).Length;
}
