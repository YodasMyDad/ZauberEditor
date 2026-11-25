using ZauberCMS.RTE.Components.Internal;
using ZauberCMS.RTE.Interop;
using ZauberCMS.RTE.Models;
using ZauberCMS.RTE.Tests.TestSupport;

namespace ZauberCMS.RTE.Tests.Components.Internal;

public sealed class EditorApiTests : IDisposable
{
    private static readonly Dictionary<string, string> Attributes = new() { ["class"] = "note" };
    private static readonly Dictionary<string, string> Styles = new() { ["text-align"] = "center" };

    private readonly IEditorHost _host = Substitute.For<IEditorHost>();
    private readonly IEditorInterop _interop = Substitute.For<IEditorInterop>();
    private readonly CancellationTokenSource _cancellation = new();
    private readonly EditorApi _api;

    public EditorApiTests()
    {
        _host.GetInteropAsync(Arg.Any<CancellationToken>()).Returns(_interop);
        _host.Settings.Returns(new EditorSettings { MaxContentBytes = 4096 });
        _host.State.Returns(EditorState.Empty);
        _api = new(_host);
    }

    public static TheoryData<Func<IEditorApi, CancellationToken, Task>, string, object?[]> InteropCommands => new()
    {
        { (api, token) => api.ToggleMarkAsync("strong", token), nameof(IEditorInterop.ToggleMarkAsync), ["strong"] },
        { (api, token) => api.WrapSelectionAsync("span", Attributes, token), nameof(IEditorInterop.WrapAsync), ["span", Attributes] },
        { (api, token) => api.WrapSelectionAsync("mark", null, token), nameof(IEditorInterop.WrapAsync), ["mark", null] },
        { (api, token) => api.UnwrapSelectionAsync("a", token), nameof(IEditorInterop.UnwrapAsync), ["a"] },
        { (api, token) => api.SetBlockTypeAsync("h2", Attributes, token), nameof(IEditorInterop.SetBlockTypeAsync), ["h2", Attributes] },
        { (api, token) => api.SetBlockStyleAsync(Styles, token), nameof(IEditorInterop.SetBlockStyleAsync), [Styles] },
        { (api, token) => api.ClearFormattingAsync(token), nameof(IEditorInterop.ClearFormattingAsync), [] },
        { (api, token) => api.InsertHtmlAsync("<p>x</p>", token), nameof(IEditorInterop.InsertHtmlAsync), ["<p>x</p>"] },
        { (api, token) => api.InsertTextAsync("x", token), nameof(IEditorInterop.InsertTextAsync), ["x"] },
        { (api, token) => api.SetHtmlAsync("<p>y</p>", token), nameof(IEditorInterop.SetHtmlAsync), ["<p>y</p>"] },
        { (api, token) => api.SaveSelectionAsync(token), nameof(IEditorInterop.SaveSelectionAsync), [] },
        { (api, token) => api.RestoreSelectionAsync(token), nameof(IEditorInterop.RestoreSelectionAsync), [] },
        { (api, token) => api.ClearSavedSelectionAsync(token), nameof(IEditorInterop.ClearSavedSelectionAsync), [] },
        { (api, token) => api.SelectLinkAtCursorAsync(token), nameof(IEditorInterop.SelectLinkAsync), [] },
        { (api, token) => api.SelectImageAtCursorAsync(token), nameof(IEditorInterop.SelectImageAsync), [] },
        { (api, token) => api.UndoAsync(token), nameof(IEditorInterop.UndoAsync), [] },
        { (api, token) => api.RedoAsync(token), nameof(IEditorInterop.RedoAsync), [] },
        { (api, token) => api.FocusAsync(token), nameof(IEditorInterop.FocusAsync), [] },
        { (api, token) => api.GetSelectionAsync(token), nameof(IEditorInterop.GetSelectionAsync), [4096L] },
        { (api, token) => api.GetLinkAtCursorAsync(token), nameof(IEditorInterop.GetLinkAsync), [4096L] },
        { (api, token) => api.GetImageAtCursorAsync(token), nameof(IEditorInterop.GetImageAsync), [4096L] },
        { (api, token) => api.GetHtmlAsync(token), nameof(IEditorInterop.GetHtmlAsync), [4096L] },
        { (api, token) => api.GetTextAsync(token), nameof(IEditorInterop.GetTextAsync), [4096L] },
        { (api, token) => api.IndentAsync(token), nameof(IEditorInterop.IndentAsync), [] },
        { (api, token) => api.OutdentAsync(token), nameof(IEditorInterop.OutdentAsync), [] },
        { (api, token) => api.SetTextStyleAsync("color", "#ef4444", token), nameof(IEditorInterop.SetTextStyleAsync), ["color", "#ef4444"] },
        { (api, token) => api.SetTextStyleAsync("color", null, token), nameof(IEditorInterop.SetTextStyleAsync), ["color", null] },
        { (api, token) => api.SetFullscreenAsync(true, token), nameof(IEditorInterop.SetFullscreenAsync), [true] },
        { (api, token) => api.SetPlainTextPasteAsync(true, token), nameof(IEditorInterop.SetPlainTextPasteAsync), [true] },
        { (api, token) => api.FindAsync("the", true, token), nameof(IEditorInterop.FindAsync), ["the", true] },
        { (api, token) => api.ReplaceAsync("the", "a", false, token), nameof(IEditorInterop.ReplaceAsync), ["the", "a", false] },
        { (api, token) => api.ReplaceAllAsync("the", "a", cancellationToken: token), nameof(IEditorInterop.ReplaceAllAsync), ["the", "a", false] }
    };

    public static TheoryData<Func<IEditorApi, CancellationToken, Task>, string, object?[]> HostCommands => new()
    {
        { (api, token) => api.FlushAsync(token), nameof(IEditorHost.FlushAsync), [] },
        { (api, token) => api.ToggleSourceViewAsync(token), nameof(IEditorHost.ToggleSourceViewAsync), [] },
        { (api, token) => api.SetThemeAsync(Theme.Dark, token), nameof(IEditorHost.SetThemeAsync), [Theme.Dark] },
        { (api, token) => api.ClosePanelAsync(token), nameof(IEditorHost.ClosePanelAsync), [] },
        { (api, token) => api.ShowToastAsync("Saved", ToastType.Success, token), nameof(IEditorHost.ShowToastAsync), ["Saved", ToastType.Success] },
        { (api, token) => api.ShowToastAsync("Hello", cancellationToken: token), nameof(IEditorHost.ShowToastAsync), ["Hello", ToastType.Info] }
    };

    public static TheoryData<Func<IEditorApi, Task>, Type> InvalidArguments => new()
    {
        { api => api.ToggleMarkAsync(" "), typeof(ArgumentException) },
        { api => api.WrapSelectionAsync(string.Empty), typeof(ArgumentException) },
        { api => api.UnwrapSelectionAsync(null!), typeof(ArgumentNullException) },
        { api => api.SetBlockTypeAsync(" "), typeof(ArgumentException) },
        { api => api.SetBlockStyleAsync(null!), typeof(ArgumentNullException) },
        { api => api.InsertHtmlAsync(null!), typeof(ArgumentNullException) },
        { api => api.InsertTextAsync(null!), typeof(ArgumentNullException) },
        { api => api.SetHtmlAsync(null!), typeof(ArgumentNullException) },
        { api => api.ShowToastAsync(" "), typeof(ArgumentException) },
        { api => api.SetTextStyleAsync(" ", "red"), typeof(ArgumentException) },
        { api => api.FindAsync(null!), typeof(ArgumentNullException) },
        { api => api.ReplaceAsync("a", null!), typeof(ArgumentNullException) },
        { api => api.ReplaceAllAsync(null!, "b"), typeof(ArgumentNullException) }
    };

    public void Dispose() => _cancellation.Dispose();

    [Theory]
    [MemberData(nameof(InteropCommands))]
    public async Task CommandsRunOnTheEditorInstanceWithTheCallersToken(
        Func<IEditorApi, CancellationToken, Task> command,
        string expectedMethod,
        object?[] expectedArguments)
    {
        var token = _cancellation.Token;

        await command(_api, token);

        var call = _interop.ReceivedCalls().ShouldHaveSingleItem();
        call.GetMethodInfo().Name.ShouldBe(expectedMethod);
        call.GetArguments().ShouldBe([.. expectedArguments, token]);
        await _host.Received(1).GetInteropAsync(token);
    }

    [Theory]
    [MemberData(nameof(HostCommands))]
    public async Task EditorLevelCommandsGoToTheHost(
        Func<IEditorApi, CancellationToken, Task> command,
        string expectedMethod,
        object?[] expectedArguments)
    {
        var token = _cancellation.Token;

        await command(_api, token);

        var call = SubstituteCalls.Commands(_host).ShouldHaveSingleItem();
        call.GetMethodInfo().Name.ShouldBe(expectedMethod);
        call.GetArguments().ShouldBe([.. expectedArguments, token]);
        _interop.ReceivedCalls().ShouldBeEmpty();
    }

    [Theory]
    [MemberData(nameof(InvalidArguments))]
    public async Task InvalidArgumentsAreRejectedBeforeReachingTheEditor(Func<IEditorApi, Task> command, Type exceptionType)
    {
        var exception = await Should.ThrowAsync(async () => await command(_api), exceptionType);

        exception.GetType().ShouldBe(exceptionType);
        _host.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact]
    public async Task ReadsReturnWhatTheEditorReports()
    {
        var token = _cancellation.Token;
        var selection = new SelectionInfo("text", IsCollapsed: false);
        var link = new LinkInfo("https://example.com", "Example", null, null) { Rel = "nofollow" };
        var image = new ImageInfo("/a.png", "A", null, 10, 20);
        _interop.GetSelectionAsync(4096, token).Returns(selection);
        _interop.GetLinkAsync(4096, token).Returns(link);
        _interop.GetImageAsync(4096, token).Returns(image);
        _interop.GetHtmlAsync(4096, token).Returns("<p>html</p>");
        _interop.GetTextAsync(4096, token).Returns("text");

        (await _api.GetSelectionAsync(token)).ShouldBeSameAs(selection);
        (await _api.GetLinkAtCursorAsync(token)).ShouldBeSameAs(link);
        (await _api.GetImageAtCursorAsync(token)).ShouldBeSameAs(image);
        (await _api.GetHtmlAsync(token)).ShouldBe("<p>html</p>");
        (await _api.GetTextAsync(token)).ShouldBe("text");
    }

    [Fact]
    public async Task ContentTooLargeToTransferIsReportedInsteadOfReadingAsEmpty()
    {
        _interop.GetHtmlAsync(Arg.Any<long>(), Arg.Any<CancellationToken>()).Returns(Task.FromException<string>(new EditorContentTooLargeException(5000, 4096)));
        _interop.GetTextAsync(Arg.Any<long>(), Arg.Any<CancellationToken>()).Returns(Task.FromException<string>(new EditorContentTooLargeException(5000, 4096)));

        (await Should.ThrowAsync<EditorContentTooLargeException>(() => _api.GetHtmlAsync(_cancellation.Token))).Length.ShouldBe(5000);
        await Should.ThrowAsync<EditorContentTooLargeException>(() => _api.GetTextAsync(_cancellation.Token));
    }

    [Fact]
    public async Task SetHtmlIsRefusedWhileReadOnly()
    {
        _host.State.Returns(EditorState.Empty with { IsReadOnly = true });

        await Should.ThrowAsync<InvalidOperationException>(() => _api.SetHtmlAsync("<p>x</p>", _cancellation.Token));

        _interop.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact]
    public async Task CommandsWaitUntilTheEditorIsReady()
    {
        var ready = new TaskCompletionSource<IEditorInterop>(TaskCreationOptions.RunContinuationsAsynchronously);
        _host.GetInteropAsync(Arg.Any<CancellationToken>()).Returns(ready.Task);

        var command = _api.ToggleMarkAsync("em", _cancellation.Token);

        command.IsCompleted.ShouldBeFalse();
        _interop.ReceivedCalls().ShouldBeEmpty();

        ready.SetResult(_interop);
        await command.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        await _interop.Received(1).ToggleMarkAsync("em", _cancellation.Token);
    }

    [Fact]
    public async Task OpenPanelPassesAPanelRequestToTheHost()
    {
        Dictionary<string, object?> parameters = new() { [nameof(ImagePanel.Image)] = null };

        await _api.OpenPanelAsync<ImagePanel>(parameters, _cancellation.Token);

        var call = SubstituteCalls.To(_host, nameof(IEditorHost.OpenPanelAsync)).ShouldHaveSingleItem();
        var request = call.GetArguments()[0].ShouldBeOfType<PanelRequest>();
        request.Content.ShouldNotBeNull();
        request.Instance.ShouldBeNull();
        call.GetArguments()[1].ShouldBe(_cancellation.Token);
    }

    [Fact]
    public async Task OpenPanelByTypePassesAPanelRequestToTheHost()
    {
        await _api.OpenPanelAsync(typeof(ImagePanel), null, _cancellation.Token);

        var call = SubstituteCalls.To(_host, nameof(IEditorHost.OpenPanelAsync)).ShouldHaveSingleItem();
        call.GetArguments()[0].ShouldBeOfType<PanelRequest>().Content.ShouldNotBeNull();
        call.GetArguments()[1].ShouldBe(_cancellation.Token);
    }

    [Theory]
    [InlineData(typeof(string))]
    [InlineData(typeof(PanelBase))]
    [InlineData(typeof(SlideOutPanel))]
    public async Task OpenPanelByTypeRejectsTypesThatAreNotConcretePanels(Type panelType)
    {
        var exception = await Should.ThrowAsync<ArgumentException>(() => _api.OpenPanelAsync(panelType));

        exception.ParamName.ShouldBe("panelType");
        _host.ReceivedCalls().Where(call => call.GetMethodInfo().Name == nameof(IEditorHost.OpenPanelAsync)).ShouldBeEmpty();
    }

    [Fact]
    public async Task OpenPanelByTypeRejectsNull() =>
        await Should.ThrowAsync<ArgumentNullException>(() => _api.OpenPanelAsync(null!));

    [Fact]
    public void StateAndSettingsComeFromTheHost()
    {
        var state = States.Block("h3");
        _host.State.Returns(state);

        _api.State.ShouldBeSameAs(state);
        _api.Settings.MaxContentBytes.ShouldBe(4096);
    }
}
