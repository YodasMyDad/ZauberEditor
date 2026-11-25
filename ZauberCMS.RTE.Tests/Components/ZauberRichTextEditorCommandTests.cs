using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ZauberCMS.RTE.Components;
using ZauberCMS.RTE.Models;
using ZauberCMS.RTE.Services;
using ZauberCMS.RTE.Tests.TestSupport;

namespace ZauberCMS.RTE.Tests.Components;

public sealed class ZauberRichTextEditorCommandTests : EditorTestContext
{
    private readonly List<string> _executed = [];

    [Fact]
    public async Task ToolbarClickRunsTheItemAndRaisesOnCommandExecuted()
    {
        var cut = RenderWithCommands();

        await cut.Find(".rte-btn-bold").ClickAsync(new());

        await Interop.Received(1).ToggleMarkAsync("strong", Arg.Any<CancellationToken>());
        _executed.ShouldBe([ToolbarItemIds.Bold]);
    }

    [Fact]
    public async Task CustomItemsReceiveTheEditorsApi()
    {
        var item = new RecordingItem();
        Services.AddZauberRte(options => options.AddToolbarItem(item));
        var cut = RenderWithCommands(new ToolbarLayout(new ToolbarItemRef(RecordingItem.ItemId)));

        await cut.Find($".rte-btn-{RecordingItem.ItemId}").ClickAsync(new());

        item.Executions.ShouldHaveSingleItem().ShouldBeSameAs(cut.Instance.Api);
        _executed.ShouldBe([RecordingItem.ItemId]);
    }

    [Fact]
    public async Task ChoosingAMenuItemRunsItAndClosesTheMenu()
    {
        var cut = RenderWithCommands();
        await cut.Find(".rte-dropdown-headings > button").ClickAsync(new());

        await cut.Find(".rte-dropdown-h2").ClickAsync(new());

        await Interop.Received(1).SetBlockTypeAsync("h2", null, Arg.Any<CancellationToken>());
        _executed.ShouldBe(["h2"]);
        cut.FindAll("[role=menu]").ShouldBeEmpty();
    }

    [Fact]
    public async Task MenuKeyboardSelectionRunsTheFocusedItem()
    {
        var cut = RenderWithCommands();
        await cut.Find(".rte-dropdown-headings > button").KeyDownAsync(new KeyboardEventArgs { Key = "ArrowDown" });

        await cut.Find("[role=menu]").KeyDownAsync(new KeyboardEventArgs { Key = "ArrowDown" });
        await cut.Find("[role=menu]").KeyDownAsync(new KeyboardEventArgs { Key = "Enter" });

        await Interop.Received(1).SetBlockTypeAsync("h2", null, Arg.Any<CancellationToken>());
        cut.FindAll("[role=menu]").ShouldBeEmpty();
    }

    [Fact]
    public async Task EscapeClosesTheMenu()
    {
        var cut = RenderWithCommands();
        await cut.Find(".rte-dropdown-headings > button").ClickAsync(new());

        await cut.Find("[role=menu]").KeyDownAsync(new KeyboardEventArgs { Key = "Escape" });

        cut.FindAll("[role=menu]").ShouldBeEmpty();
        _executed.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShortcutRunsTheItem()
    {
        RenderWithCommands();

        await Callbacks.OnShortcutAsync("ITALIC");

        await Interop.Received(1).ToggleMarkAsync("em", Arg.Any<CancellationToken>());
        _executed.ShouldBe([ToolbarItemIds.Italic]);
    }

    [Fact]
    public async Task ShortcutForAnItemOutsideTheLayoutIsIgnored()
    {
        var item = new RecordingItem();
        Services.AddZauberRte(options => options.AddToolbarItem(item));
        RenderWithCommands(new ToolbarLayout(new ToolbarItemRef(ToolbarItemIds.Bold)));

        await Callbacks.OnShortcutAsync(RecordingItem.ItemId);
        await Callbacks.OnShortcutAsync(ToolbarItemIds.Italic);

        item.Executions.ShouldBeEmpty();
        _executed.ShouldBeEmpty();
        Interop.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact]
    public async Task ShortcutsAreIgnoredWhileReadOnlyEvenForItemsThatStayEnabled()
    {
        var item = new RecordingItem();
        Services.AddZauberRte(options => options.AddToolbarItem(item));
        RenderWithCommands(new ToolbarLayout(new ToolbarItemRef(RecordingItem.ItemId)), readOnly: true);

        await Callbacks.OnShortcutAsync(RecordingItem.ItemId);

        item.Executions.ShouldBeEmpty();
    }

    [Fact]
    public async Task ItemsReceiveTheEditorLifetimeToken()
    {
        var item = new RecordingItem();
        Services.AddZauberRte(options => options.AddToolbarItem(item));
        var cut = RenderWithCommands(new ToolbarLayout(new ToolbarItemRef(RecordingItem.ItemId)));
        await cut.Find($".rte-btn-{RecordingItem.ItemId}").ClickAsync(new());
        var token = item.Tokens.ShouldHaveSingleItem();

        token.IsCancellationRequested.ShouldBeFalse();
        await cut.Instance.DisposeAsync();

        token.IsCancellationRequested.ShouldBeTrue();
    }

    [Fact]
    public async Task CancellationThatIsNotDisposalIsReportedLikeAnyFailure()
    {
        Interop.ToggleMarkAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.FromException(new TaskCanceledException("Timed out.")));
        var cut = RenderWithCommands();

        await cut.Find(".rte-btn-bold").ClickAsync(new());

        cut.Find(".rte-toast-error").TextContent.ShouldContain("didn't work");
    }

    [Fact]
    public async Task UnknownShortcutIsIgnored()
    {
        RenderWithCommands();

        await Callbacks.OnShortcutAsync("does-not-exist");

        _executed.ShouldBeEmpty();
        Interop.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact]
    public async Task DisabledItemsDoNotRunFromShortcuts()
    {
        RenderWithCommands(readOnly: true);

        await Callbacks.OnShortcutAsync(ToolbarItemIds.Bold);
        await Callbacks.OnShortcutAsync(ToolbarItemIds.Undo);

        _executed.ShouldBeEmpty();
        Interop.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact]
    public async Task FailingItemShowsAnErrorToastAndTheEditorKeepsWorking()
    {
        Services.AddZauberRte(options => options.AddToolbarItem<FailingItem>());
        var cut = RenderWithCommands(new ToolbarLayout(new ToolbarGroup(FailingItem.ItemId, ToolbarItemIds.Bold)));

        await cut.Find($".rte-btn-{FailingItem.ItemId}").ClickAsync(new());

        var toast = cut.Find(".rte-toast-region > .rte-toast");
        toast.ClassList.ShouldContain("rte-toast-error");
        toast.TextContent.ShouldContain("didn't work");
        _executed.ShouldBeEmpty();

        await cut.Find(".rte-btn-bold").ClickAsync(new());

        _executed.ShouldBe([ToolbarItemIds.Bold]);
    }

    [Fact]
    public async Task ToastsExpireAndCanBeDismissed()
    {
        var cut = RenderWithCommands();
        await cut.InvokeAsync(() => cut.Instance.Api.ShowToastAsync("First", ToastType.Success, TestContext.Current.CancellationToken));
        Time.Advance(TimeSpan.FromSeconds(2));
        await cut.InvokeAsync(() => cut.Instance.Api.ShowToastAsync("Second", ToastType.Warning, TestContext.Current.CancellationToken));

        cut.FindAll(".rte-toast").Select(toast => toast.ClassName).ShouldBe(["rte-toast rte-toast-success", "rte-toast rte-toast-warning"]);

        Time.Advance(TimeSpan.FromSeconds(3));

        await cut.WaitForAssertionAsync(() => cut.FindAll(".rte-toast-message").Select(message => message.TextContent).ShouldBe(["Second"]));

        await cut.Find(".rte-toast-close").ClickAsync(new());

        cut.FindAll(".rte-toast").ShouldBeEmpty();
    }

    [Fact]
    public async Task RejectedInputShowsAWarningToast()
    {
        var cut = RenderWithCommands();

        await Callbacks.OnInputRejectedAsync("Too big");

        cut.Find(".rte-toast-warning .rte-toast-message").TextContent.ShouldBe("Too big");
    }

    [Fact]
    public async Task AtMostThreeToastsAreShown()
    {
        var cut = RenderWithCommands();

        foreach (var message in new[] { "1", "2", "3", "4" })
        {
            await cut.InvokeAsync(() => cut.Instance.Api.ShowToastAsync(message, cancellationToken: TestContext.Current.CancellationToken));
        }

        cut.FindAll(".rte-toast-message").Select(toast => toast.TextContent).ShouldBe(["2", "3", "4"]);
    }

    [Fact]
    public async Task PanelOpensAsAnAccessibleDialogAndClosesOnEscape()
    {
        var cut = RenderWithCommands();

        await cut.Find(".rte-btn-link").ClickAsync(new());

        var dialog = cut.Find(".rte-slideout-overlay > [role=dialog]");
        dialog.GetAttribute("aria-modal").ShouldBe("true");
        dialog.ClassList.ShouldContain("zauber-rte-portal");
        dialog.ClassList.ShouldContain("rte-theme-auto");
        var title = cut.Find($"#{dialog.GetAttribute("aria-labelledby")}");
        title.TextContent.ShouldBe("Link");
        cut.Find(".rte-slideout-close").GetAttribute("aria-label").ShouldBe("Close");
        cut.FindAll(".rte-slideout-content input[inputmode=url]").Count.ShouldBe(1);
        await Interop.Received(1).OpenPanelAsync(Arg.Any<ElementReference>(), Arg.Any<CancellationToken>());

        await Callbacks.OnPanelEscapeAsync();

        await Interop.Received(1).ClosePanelAsync(Arg.Any<CancellationToken>());
        cut.FindAll("[role=dialog]").ShouldBeEmpty();
    }

    [Fact]
    public async Task PanelHeadingFollowsTheTitleWhenThePanelReRenders()
    {
        var cut = RenderWithCommands();

        await cut.InvokeAsync(() => cut.Instance.Api.OpenPanelAsync<CountingPanel>(cancellationToken: TestContext.Current.CancellationToken));
        cut.Find(".rte-slideout-title").TextContent.ShouldBe("Count 0");

        await cut.Find(".counting-panel-next").ClickAsync(new());

        cut.Find(".rte-slideout-title").TextContent.ShouldBe("Count 1");
    }

    [Fact]
    public async Task PanelClosesFromItsCloseButtonAndOverlay()
    {
        var cut = RenderWithCommands();

        await cut.Find(".rte-btn-table").ClickAsync(new());
        cut.Find(".rte-slideout-title").TextContent.ShouldBe("Insert table");
        await cut.Find(".rte-slideout-close").ClickAsync(new());

        cut.FindAll("[role=dialog]").ShouldBeEmpty();

        await cut.Find(".rte-btn-table").ClickAsync(new());
        await cut.Find(".rte-slideout-overlay").ClickAsync(new());

        cut.FindAll("[role=dialog]").ShouldBeEmpty();
        await Interop.Received(2).ClosePanelAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task OpeningAnotherPanelClosesTheFirst()
    {
        var cut = RenderWithCommands();
        await cut.Find(".rte-btn-link").ClickAsync(new());

        await cut.Find(".rte-btn-table").ClickAsync(new());

        await Interop.Received(1).ClosePanelAsync(Arg.Any<CancellationToken>());
        cut.FindAll("[role=dialog]").ShouldHaveSingleItem();
        cut.Find(".rte-slideout-title").TextContent.ShouldBe("Insert table");
    }

    [Fact]
    public async Task PanelActionsUseTheEditor()
    {
        var cut = RenderWithCommands();
        await cut.Find(".rte-btn-table").ClickAsync(new());

        await cut.Find(".rte-slideout-content .rte-btn-primary").ClickAsync(new());

        await Interop.Received(1).InsertHtmlAsync(Arg.Is<string>(html => html.StartsWith("<table", StringComparison.Ordinal)), Arg.Any<CancellationToken>());
        cut.FindAll("[role=dialog]").ShouldBeEmpty();
    }

    private IRenderedComponent<ZauberRichTextEditor> RenderWithCommands(ToolbarLayout? toolbar = null, bool readOnly = false) =>
        RenderEditor(parameters => parameters
            .Add(editor => editor.Toolbar, toolbar)
            .Add(editor => editor.ReadOnly, readOnly)
            .Add(editor => editor.OnCommandExecuted, (CommandExecutedArgs args) => _executed.Add(args.ItemId)));
}
