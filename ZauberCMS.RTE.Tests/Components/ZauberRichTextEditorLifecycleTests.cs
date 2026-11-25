using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ZauberCMS.RTE.Components.Internal;
using ZauberCMS.RTE.Interop;
using ZauberCMS.RTE.Models;
using ZauberCMS.RTE.Services;
using ZauberCMS.RTE.Tests.TestSupport;

namespace ZauberCMS.RTE.Tests.Components;

public sealed class ZauberRichTextEditorLifecycleTests : EditorTestContext
{
    [Fact]
    public void ScriptInstanceIsCreatedOnceWithTheEditorElements()
    {
        Services.AddZauberRte(options => options.AutoLoadStyles = false);
        var cut = RenderEditor();

        var call = Factory.ReceivedCalls().ShouldHaveSingleItem().GetArguments();
        call[0].ShouldBeOfType<ElementReference>().ShouldBeElementReferenceTo(cut.Find("div.zauber-rte"));
        call[1].ShouldBeOfType<ElementReference>().ShouldBeElementReferenceTo(cut.Find(".rte-content"));
        call[2].ShouldBeOfType<ElementReference>().ShouldBeElementReferenceTo(cut.Find(".rte-source-view"));
        call[3].ShouldBeOfType<ElementReference>().ShouldBeElementReferenceTo(cut.Find(".rte-toolbar"));

        cut.Render(parameters => parameters.Add(editor => editor.Placeholder, "Later"));

        Factory.ReceivedCalls().ShouldHaveSingleItem();
    }

    [Fact]
    public void EachEditorGetsItsOwnCallbacks()
    {
        RenderEditor();
        var first = CallbackReference;
        RenderEditor();

        CallbackReference.ShouldNotBeSameAs(first);
        CallbackReference.ShouldNotBeNull().Value.ShouldNotBeSameAs(first.ShouldNotBeNull().Value);
    }

    [Fact]
    public async Task ApiCallsWaitUntilTheScriptHasLoaded()
    {
        var creation = DeferCreation();
        var cut = RenderEditor();

        var command = cut.Instance.Api.ToggleMarkAsync("strong", TestContext.Current.CancellationToken);

        command.IsCompleted.ShouldBeFalse();
        creation.SetResult(Interop);
        await command.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await Interop.Received(1).ToggleMarkAsync("strong", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ScriptLoadFailureShowsAnErrorAndFailsApiCalls()
    {
        Factory.CreateAsync(Arg.Any<ElementReference>(), Arg.Any<ElementReference>(), Arg.Any<ElementReference>(), Arg.Any<ElementReference>(), Arg.Any<DotNetObjectReference<EditorCallbacks>>(), Arg.Any<EditorInitOptions>())
            .Returns(Task.FromException<IEditorInterop?>(new JSException("Module not found")));

        var cut = RenderEditor();

        cut.Find(".rte-toast-error .rte-toast-message").TextContent.ShouldBe("The editor could not be loaded.");
        await Should.ThrowAsync<JSException>(() => cut.Instance.Api.UndoAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task FocusAndBlurAreRaised()
    {
        List<string> events = [];
        RenderEditor(parameters => parameters
            .Add(editor => editor.OnFocus, () => events.Add("focus"))
            .Add(editor => editor.OnBlur, () => events.Add("blur")));

        await Callbacks.OnFocusChangedAsync(true);
        await Callbacks.OnFocusChangedAsync(false);

        events.ShouldBe(["focus", "blur"]);
    }

    [Fact]
    public async Task ImageResizeIsRaised()
    {
        ImageResizedArgs? resized = null;
        RenderEditor(parameters => parameters.Add(editor => editor.OnImageResized, (ImageResizedArgs args) => resized = args));

        await Callbacks.OnImageResizedAsync(640, 480);

        resized.ShouldBe(new ImageResizedArgs(640, 480));
    }

    [Fact]
    public async Task DisposeReleasesTheScriptInstanceCallbacksAndItems()
    {
        Services.AddZauberRte(options => options.AddToolbarItem<DisposableItem>());
        var cut = RenderEditor(parameters => parameters.Add(editor => editor.Toolbar, new ToolbarLayout(new ToolbarItemRef(DisposableItem.ItemId))));
        var reference = CallbackReference.ShouldNotBeNull();
        var item = cut.FindComponent<ToolbarButton>().Instance.Item.ShouldBeOfType<DisposableItem>();

        await DisposeComponentsAsync();

        await Interop.Received(1).DisposeAsync();
        Should.Throw<ObjectDisposedException>(() => reference.Value);
        item.IsDisposed.ShouldBeTrue();
    }

    [Fact]
    public async Task DisposeDeliversPendingContentBeforeReleasingTheScript()
    {
        List<string?> values = [];
        var cut = RenderEditor(parameters => parameters
            .Add(editor => editor.Value, "<p>a</p>")
            .Add(editor => editor.ValueChanged, (string? html) => values.Add(html)));
        var callbacks = Callbacks;
        Interop.FlushAsync(Arg.Any<CancellationToken>()).Returns(_ => callbacks.OnContentChangedAsync("<p>typed</p>", "input", 0));

        await cut.Instance.DisposeAsync();

        values.ShouldBe(["<p>typed</p>"]);
        Received.InOrder(() =>
        {
            Interop.FlushAsync(Arg.Any<CancellationToken>());
            Interop.DisposeAsync();
        });
    }

    [Fact]
    public async Task DisposeAfterTheConnectionIsLostStillReleasesEverything()
    {
        var cut = RenderEditor();
        Interop.FlushAsync(Arg.Any<CancellationToken>()).Returns(Task.FromException(new JSDisconnectedException("gone")));

        await Should.NotThrowAsync(cut.Instance.DisposeAsync().AsTask());

        await Interop.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task DisposingTwiceIsSafe()
    {
        var cut = RenderEditor();

        await cut.Instance.DisposeAsync();
        await cut.Instance.DisposeAsync();

        await Interop.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task ScriptInstanceCreatedAfterDisposalIsReleasedImmediately()
    {
        var creation = DeferCreation();
        var cut = RenderEditor();

        await cut.Instance.DisposeAsync();
        creation.SetResult(Interop);

        await cut.WaitForAssertionAsync(() => Interop.Received(1).DisposeAsync().AsTask());
        await Should.ThrowAsync<ObjectDisposedException>(() => cut.Instance.Api.UndoAsync(TestContext.Current.CancellationToken));
        Interop.ReceivedCalls().Count().ShouldBe(1);
    }

    [Fact]
    public async Task ScriptInstanceIsCreatedWithoutTheEditorLifetimeSoItCanAlwaysBeReleased()
    {
        var creation = DeferCreation();
        var cut = RenderEditor();

        await cut.Instance.DisposeAsync();

        Factory.ReceivedCalls().ShouldHaveSingleItem().GetArguments().OfType<CancellationToken>().ShouldBeEmpty();
        creation.SetResult(Interop);
        await cut.WaitForAssertionAsync(() => Interop.Received(1).DisposeAsync().AsTask());
    }

    [Fact]
    public async Task ApiCallsAfterDisposalFailConsistently()
    {
        var cut = RenderEditor();

        await cut.Instance.DisposeAsync();

        await Should.ThrowAsync<ObjectDisposedException>(() => cut.Instance.Api.UndoAsync(TestContext.Current.CancellationToken));
        await Should.ThrowAsync<ObjectDisposedException>(() => cut.Instance.Api.GetHtmlAsync(TestContext.Current.CancellationToken));
        await Interop.DidNotReceive().UndoAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AnyScriptLoadFailureShowsAnErrorAndFailsApiCallsInsteadOfHanging()
    {
        Factory.CreateAsync(Arg.Any<ElementReference>(), Arg.Any<ElementReference>(), Arg.Any<ElementReference>(), Arg.Any<ElementReference>(), Arg.Any<DotNetObjectReference<EditorCallbacks>>(), Arg.Any<EditorInitOptions>())
            .Returns(Task.FromException<IEditorInterop?>(new TaskCanceledException("Timed out.")));

        var cut = RenderEditor();

        cut.Find(".rte-toast-error .rte-toast-message").TextContent.ShouldBe("The editor could not be loaded.");
        await Should.ThrowAsync<TaskCanceledException>(() => cut.Instance.Api.UndoAsync(TestContext.Current.CancellationToken))
            .WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task HostOperationsCanBeCalledFromAnyThread()
    {
        var cut = RenderEditor();

        await Task.Run(() => cut.Instance.Api.ShowToastAsync("Done", ToastType.Success, TestContext.Current.CancellationToken), TestContext.Current.CancellationToken);
        await Task.Run(() => cut.Instance.Api.SetThemeAsync(Theme.Dark, TestContext.Current.CancellationToken), TestContext.Current.CancellationToken);
        await Task.Run(() => cut.Instance.Api.OpenPanelAsync<TablePanel>(cancellationToken: TestContext.Current.CancellationToken), TestContext.Current.CancellationToken);
        await Task.Run(() => cut.Instance.Api.ClosePanelAsync(TestContext.Current.CancellationToken), TestContext.Current.CancellationToken);

        cut.Find(".rte-toast-success .rte-toast-message").TextContent.ShouldBe("Done");
        cut.Find("div.zauber-rte").ClassList.ShouldContain("rte-theme-dark");
        await Interop.Received(1).OpenPanelAsync(Arg.Any<ElementReference>(), Arg.Any<CancellationToken>());
        cut.FindAll("[role=dialog]").ShouldBeEmpty();
    }

    [Fact]
    public async Task ExceptionsFromFocusAndImageHandlersAreReportedNotSwallowed()
    {
        var cut = RenderEditor(parameters => parameters
            .Add(editor => editor.OnFocus, () => throw new InvalidOperationException("consumer bug"))
            .Add(editor => editor.OnImageResized, (ImageResizedArgs _) => throw new InvalidOperationException("consumer bug")));

        await Should.NotThrowAsync(() => Callbacks.OnFocusChangedAsync(true));
        await Should.NotThrowAsync(() => Callbacks.OnImageResizedAsync(10, 10));

        cut.FindAll(".rte-toast-error").Count.ShouldBe(2);
    }
}
