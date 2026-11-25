using Bunit;
using Microsoft.AspNetCore.Components.Forms;
using ZauberCMS.RTE.Components;
using ZauberCMS.RTE.Models;
using ZauberCMS.RTE.Services;
using ZauberCMS.RTE.Tests.TestSupport;

namespace ZauberCMS.RTE.Tests.Components;

public sealed class ZauberRichTextEditorContentTests : EditorTestContext
{
    private readonly List<string?> _values = [];
    private readonly List<EditorChangeArgs> _changes = [];

    [Fact]
    public void InitialValueIsPassedToTheScriptOnCreation()
    {
        RenderEditor(parameters => parameters.Add(editor => editor.Value, "<p>Start</p>").Add(editor => editor.Placeholder, "Type"));

        var options = InitOptions.ShouldNotBeNull();
        options.Html.ShouldBe("<p>Start</p>");
        options.Placeholder.ShouldBe("Type");
        options.ReadOnly.ShouldBeFalse();
        options.Shortcuts.ShouldContain(shortcut => shortcut.ItemId == "bold");
        Interop.ReceivedCalls().ShouldBeEmpty();
    }

    [Theory]
    [InlineData("input", ChangeSource.Input)]
    [InlineData("paste", ChangeSource.Paste)]
    [InlineData("history", ChangeSource.History)]
    [InlineData("sourceView", ChangeSource.SourceView)]
    public async Task ContentChangeRaisesValueChangedAndOnChange(string source, ChangeSource expected)
    {
        RenderBound("<p>a</p>");

        await Callbacks.OnContentChangedAsync("<p>ab</p>", source, 0);

        _values.ShouldBe(["<p>ab</p>"]);
        _changes.ShouldBe([new EditorChangeArgs("<p>ab</p>", expected)]);
    }

    [Fact]
    public async Task UnchangedContentRaisesNothing()
    {
        RenderBound("<p>a</p>");

        await Callbacks.OnContentChangedAsync("<p>a</p>", "input", 0);

        _values.ShouldBeEmpty();
        _changes.ShouldBeEmpty();
    }

    [Fact]
    public async Task LargeContentArrivesInChunksAndIsDeliveredWithTheLastOne()
    {
        var settings = new EditorSettings { MaxContentBytes = 50_000 };
        var large = $"<p>{new string('x', 20_000)}</p>";
        RenderBound("<p>a</p>", parameters => parameters.Add(editor => editor.Settings, settings));

        await Callbacks.OnContentChunkAsync(large[..12_000]);
        _values.ShouldBeEmpty();
        await Callbacks.OnContentChangedAsync(large[12_000..], "paste", 0);

        _values.ShouldBe([large]);
        _changes.ShouldBe([new EditorChangeArgs(large, ChangeSource.Paste)]);
        await Interop.DidNotReceive().GetHtmlAsync(Arg.Any<long>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ValueChangedRunsBeforeTheNotificationYieldsSoAQueuedSaveSeesTheLatestValue()
    {
        RenderBound("<p>a</p>");

        var chunk = Callbacks.OnContentChunkAsync("<p>first half, ");
        var last = Callbacks.OnContentChangedAsync("second half</p>", "input", 0);

        chunk.IsCompletedSuccessfully.ShouldBeTrue();
        last.IsCompletedSuccessfully.ShouldBeTrue();
        _values.ShouldBe(["<p>first half, second half</p>"]);
    }

    [Fact]
    public async Task ContentTooLargeToDeliverLeavesTheValueAloneAndWarnsTheUser()
    {
        var cut = RenderBound("<p>a</p>");

        await Callbacks.OnContentChangedAsync(null, "input", 0);

        _values.ShouldBeEmpty();
        _changes.ShouldBeEmpty();
        cut.Find(".rte-toast-warning .rte-toast-message").TextContent.ShouldContain("too large to save");
    }

    [Fact]
    public async Task ChunksBeyondMaxContentBytesAreDiscardedAndTheNextChangeStartsClean()
    {
        var cut = RenderBound("<p>a</p>", parameters => parameters.Add(editor => editor.Settings, new EditorSettings { MaxContentBytes = 10 }));

        await Callbacks.OnContentChunkAsync("<p>12345");
        await Callbacks.OnContentChangedAsync("678</p>", "input", 0);

        _values.ShouldBeEmpty();
        cut.Find(".rte-toast-warning .rte-toast-message").TextContent.ShouldContain("too large to save");
        await Callbacks.OnContentChangedAsync("<p>b</p>", "input", 0);
        _values.ShouldBe(["<p>b</p>"]);
    }

    [Fact]
    public async Task ChunksOfANotificationFromAnEarlierRevisionAreDiscarded()
    {
        var cut = RenderBound("<p>a</p>");
        cut.Render(parameters => parameters.Add(editor => editor.Value, "<p>reset</p>"));

        await Callbacks.OnContentChunkAsync("<p>stale ");
        await Callbacks.OnContentChangedAsync("content</p>", "input", 0);
        await Callbacks.OnContentChangedAsync("<p>fresh</p>", "input", 1);

        _values.ShouldBe(["<p>fresh</p>"]);
    }

    [Fact]
    public async Task FlushAsyncWaitsUntilValueChangedHasFinishedForPendingContent()
    {
        var stored = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var cut = RenderEditor(parameters => parameters
            .Add(editor => editor.Value, "<p>a</p>")
            .Add(editor => editor.ValueChanged, async (string? html) =>
            {
                await stored.Task;
                _values.Add(html);
            }));
        Interop.FlushAsync(Arg.Any<CancellationToken>()).Returns(call =>
        {
            _ = Callbacks.OnContentChangedAsync("<p>pending</p>", "input", 0);
            return Task.CompletedTask;
        });

        var flush = cut.Instance.Api.FlushAsync(TestContext.Current.CancellationToken);

        flush.IsCompleted.ShouldBeFalse();
        _values.ShouldBeEmpty();
        stored.SetResult();
        await flush.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        _values.ShouldBe(["<p>pending</p>"]);
    }

    [Fact]
    public async Task FlushAsyncAlsoWaitsForContentAlreadyBeingDelivered()
    {
        var stored = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var host = Render<BoundEditorHost>(parameters => parameters
            .Add(bound => bound.InitialHtml, "<p>a</p>")
            .Add(bound => bound.BeforeStoring, _ => stored.Task));
        var editor = host.FindComponent<ZauberRichTextEditor>().Instance;
        var notification = Callbacks.OnContentChangedAsync("<p>typed</p>", "input", 0);

        var flush = editor.Api.FlushAsync(TestContext.Current.CancellationToken);

        flush.IsCompleted.ShouldBeFalse();
        stored.SetResult();
        await Task.WhenAll(notification, flush).WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        host.Instance.Html.ShouldBe("<p>typed</p>");
    }

    [Fact]
    public async Task FlushAsyncWithNothingPendingCompletesWithoutRaisingChanges()
    {
        var cut = RenderBound("<p>a</p>");

        await cut.Instance.Api.FlushAsync(TestContext.Current.CancellationToken);

        await Interop.Received(1).FlushAsync(Arg.Any<CancellationToken>());
        _values.ShouldBeEmpty();
        _changes.ShouldBeEmpty();
    }

    [Fact]
    public async Task ParentEchoDuringTheCallbackIsNotWrittenBack()
    {
        var host = Render<BoundEditorHost>(parameters => parameters.Add(bound => bound.InitialHtml, "<p>a</p>"));

        await Callbacks.OnContentChangedAsync("<p>ab</p>", "input", 0);

        host.Instance.Html.ShouldBe("<p>ab</p>");
        await Interop.DidNotReceive().ResetHtmlAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StaleEchoFromASlowParentIsNotWrittenBack()
    {
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var host = Render<BoundEditorHost>(parameters => parameters
            .Add(bound => bound.InitialHtml, string.Empty)
            .Add(bound => bound.BeforeStoring, html => html == "abc" ? release.Task : Task.CompletedTask));

        var slow = Callbacks.OnContentChangedAsync("abc", "input", 0);
        await Callbacks.OnContentChangedAsync("abcd", "input", 0);
        release.SetResult();
        await slow.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        host.Instance.Html.ShouldBe("abc");
        await Interop.DidNotReceive().ResetHtmlAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task OneWayBindingRestoringAnEarlierReportedValueIsWritten()
    {
        var cut = RenderBound("<p>start</p>");
        foreach (var html in new[] { "A", "B", "C" })
        {
            await Callbacks.OnContentChangedAsync(html, "input", 0);
        }

        cut.Render(parameters => parameters.Add(editor => editor.Value, "A"));

        await Interop.Received(1).ResetHtmlAsync("A", 1, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ParentChangeIsWrittenWithANewRevisionWithoutRaisingValueChanged()
    {
        var cut = RenderBound("<p>a</p>");

        cut.Render(parameters => parameters.Add(editor => editor.Value, "<p>from parent</p>"));
        cut.Render(parameters => parameters.Add(editor => editor.Value, "<p>again</p>"));

        Received.InOrder(() =>
        {
            Interop.ResetHtmlAsync("<p>from parent</p>", 1, Arg.Any<CancellationToken>());
            Interop.ResetHtmlAsync("<p>again</p>", 2, Arg.Any<CancellationToken>());
        });
        _values.ShouldBeEmpty();
        _changes.ShouldBeEmpty();
    }

    [Fact]
    public async Task NotificationSentBeforeTheParentPushIsDropped()
    {
        var cut = RenderBound("<p>a</p>");
        cut.Render(parameters => parameters.Add(editor => editor.Value, "<p>reset</p>"));

        await Callbacks.OnContentChangedAsync("<p>typed before the reset</p>", "input", 0);

        _values.ShouldBeEmpty();
        await Callbacks.OnContentChangedAsync("<p>typed after the reset</p>", "input", 1);
        _values.ShouldBe(["<p>typed after the reset</p>"]);
    }

    [Fact]
    public async Task ContentNotificationsAreIgnoredWhileReadOnly()
    {
        RenderBound("<p>a</p>", parameters => parameters.Add(editor => editor.ReadOnly, true));

        await Callbacks.OnContentChangedAsync("<p>tampered</p>", "input", 0);

        _values.ShouldBeEmpty();
        _changes.ShouldBeEmpty();
    }

    [Fact]
    public async Task RevertingToAnEarlierValueIsWritten()
    {
        var cut = RenderBound("<p>original</p>");
        await Callbacks.OnContentChangedAsync("<p>edited</p>", "input", 0);
        cut.Render(parameters => parameters.Add(editor => editor.Value, "<p>edited</p>"));

        cut.Render(parameters => parameters.Add(editor => editor.Value, "<p>original</p>"));

        await Interop.Received(1).ResetHtmlAsync("<p>original</p>", 1, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ClearingTheValueEmptiesTheEditor()
    {
        var cut = RenderBound("<p>a</p>");

        cut.Render(parameters => parameters.Add(editor => editor.Value, null));

        await Interop.Received(1).ResetHtmlAsync(string.Empty, 1, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UnboundEditorStillReportsChanges()
    {
        RenderEditor(parameters => parameters
            .Add(editor => editor.Value, "<p>a</p>")
            .Add(editor => editor.OnChange, (EditorChangeArgs change) => _changes.Add(change)));

        await Callbacks.OnContentChangedAsync("<p>b</p>", "input", 0);

        _changes.ShouldBe([new EditorChangeArgs("<p>b</p>", ChangeSource.Input)]);
    }

    [Fact]
    public async Task ChangesNotifyTheEditContext()
    {
        var model = new FormModel { Html = "<p>a</p>" };
        var editContext = new EditContext(model);
        List<FieldIdentifier> changed = [];
        editContext.OnFieldChanged += (_, args) => changed.Add(args.FieldIdentifier);
        RenderEditor(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(editor => editor.Value, model.Html)
            .Add(editor => editor.ValueChanged, (string? value) => model.Html = value)
            .Add(editor => editor.ValueExpression, () => model.Html));

        await Callbacks.OnContentChangedAsync("<p>b</p>", "input", 0);

        model.Html.ShouldBe("<p>b</p>");
        changed.ShouldBe([new FieldIdentifier(model, nameof(FormModel.Html))]);
    }

    [Fact]
    public async Task ValueChangedBeforeTheScriptLoadsIsWrittenOnceItHas()
    {
        var creation = DeferCreation();
        var cut = RenderBound("<p>first</p>");

        cut.Render(parameters => parameters.Add(editor => editor.Value, "<p>second</p>"));
        creation.SetResult(Interop);

        await cut.WaitForAssertionAsync(() => Interop.Received(1).ResetHtmlAsync("<p>second</p>", 1, Arg.Any<CancellationToken>()));
        InitOptions.ShouldNotBeNull().Html.ShouldBe("<p>first</p>");
    }

    [Fact]
    public async Task FailedWriteShowsAnErrorInsteadOfThrowing()
    {
        Interop.ResetHtmlAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(Task.FromException(new InvalidOperationException("broken")));
        var cut = RenderBound("<p>a</p>");

        cut.Render(parameters => parameters.Add(editor => editor.Value, "<p>b</p>"));

        cut.Find(".rte-toast-error").TextContent.ShouldContain("didn't work");
        _values.ShouldBeEmpty();
        await Callbacks.OnContentChangedAsync("<p>typed</p>", "input", 1);
        _values.ShouldBe(["<p>typed</p>"]);
    }

    [Fact]
    public async Task ExceptionsFromValueChangedHandlersAreReportedNotSwallowed()
    {
        var cut = RenderEditor(parameters => parameters
            .Add(editor => editor.Value, "<p>a</p>")
            .Add(editor => editor.ValueChanged, (string? _) => throw new InvalidOperationException("consumer bug")));

        await Should.NotThrowAsync(() => Callbacks.OnContentChangedAsync("<p>b</p>", "input", 0));

        cut.Find(".rte-toast-error").TextContent.ShouldContain("didn't work");
    }

    [Fact]
    public async Task ToolbarItemsWaitForContentStillBeingReported()
    {
        var item = new RecordingItem();
        Services.AddZauberRte(options => options.AddToolbarItem(item));
        var stored = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var cut = RenderEditor(parameters => parameters
            .Add(editor => editor.Value, "<p>a</p>")
            .Add(editor => editor.ValueChanged, async (string? html) =>
            {
                await stored.Task;
                _values.Add(html);
            })
            .Add(editor => editor.Toolbar, new ToolbarLayout(new ToolbarItemRef(RecordingItem.ItemId))));

        var notification = Callbacks.OnContentChangedAsync("<p>latest</p>", "input", 0);
        var click = cut.Find($".rte-btn-{RecordingItem.ItemId}").ClickAsync(new());

        item.Executions.ShouldBeEmpty();
        stored.SetResult();
        await Task.WhenAll(notification, click).WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        _values.ShouldBe(["<p>latest</p>"]);
        item.Executions.ShouldHaveSingleItem();
    }

    private IRenderedComponent<ZauberRichTextEditor> RenderBound(
        string? value,
        Action<ComponentParameterCollectionBuilder<ZauberRichTextEditor>>? configure = null) =>
        RenderEditor(parameters =>
        {
            parameters
                .Add(editor => editor.Value, value)
                .Add(editor => editor.ValueChanged, (string? html) => _values.Add(html))
                .Add(editor => editor.OnChange, (EditorChangeArgs change) => _changes.Add(change));
            configure?.Invoke(parameters);
        });
}
