using ZauberCMS.RTE.Components.Internal;
using ZauberCMS.RTE.Models;
using ZauberCMS.RTE.Tests.TestSupport;

namespace ZauberCMS.RTE.Tests.Toolbar;

public sealed class ToolbarItemExecutionTests
{
    private readonly IEditorApi _api = Substitute.For<IEditorApi>();

    public static TheoryData<string, EditorState, Action<IEditorApi>> Commands => new()
    {
        { ToolbarItemIds.Bold, States.Ready, api => api.ToggleMarkAsync("strong", Arg.Any<CancellationToken>()) },
        { ToolbarItemIds.Italic, States.Ready, api => api.ToggleMarkAsync("em", Arg.Any<CancellationToken>()) },
        { ToolbarItemIds.Underline, States.Ready, api => api.ToggleMarkAsync("u", Arg.Any<CancellationToken>()) },
        { ToolbarItemIds.Strike, States.Marks("s"), api => api.ToggleMarkAsync("s", Arg.Any<CancellationToken>()) },
        { ToolbarItemIds.Subscript, States.Ready, api => api.ToggleMarkAsync("sub", Arg.Any<CancellationToken>()) },
        { ToolbarItemIds.Superscript, States.Ready, api => api.ToggleMarkAsync("sup", Arg.Any<CancellationToken>()) },
        { ToolbarItemIds.Code, States.Ready, api => api.SetBlockTypeAsync("pre", null, Arg.Any<CancellationToken>()) },
        { ToolbarItemIds.Code, States.Ready with { HasSelection = true }, api => api.ToggleMarkAsync("code", Arg.Any<CancellationToken>()) },
        { ToolbarItemIds.Code, States.Marks("code"), api => api.ToggleMarkAsync("code", Arg.Any<CancellationToken>()) },
        { ToolbarItemIds.Code, States.Block("pre") with { HasSelection = true }, api => api.SetBlockTypeAsync("p", null, Arg.Any<CancellationToken>()) },
        { ToolbarItemIds.Heading1, States.Ready, api => api.SetBlockTypeAsync("h1", null, Arg.Any<CancellationToken>()) },
        { ToolbarItemIds.Heading1, States.Block("h1"), api => api.SetBlockTypeAsync("p", null, Arg.Any<CancellationToken>()) },
        { ToolbarItemIds.Heading3, States.Block("h1"), api => api.SetBlockTypeAsync("h3", null, Arg.Any<CancellationToken>()) },
        { ToolbarItemIds.Heading6, States.Ready, api => api.SetBlockTypeAsync("h6", null, Arg.Any<CancellationToken>()) },
        { ToolbarItemIds.Blockquote, States.Ready, api => api.SetBlockTypeAsync("blockquote", null, Arg.Any<CancellationToken>()) },
        { ToolbarItemIds.Blockquote, States.Block("blockquote"), api => api.SetBlockTypeAsync("p", null, Arg.Any<CancellationToken>()) },
        { ToolbarItemIds.UnorderedList, States.Block("ol"), api => api.SetBlockTypeAsync("ul", null, Arg.Any<CancellationToken>()) },
        { ToolbarItemIds.OrderedList, States.Ready, api => api.SetBlockTypeAsync("ol", null, Arg.Any<CancellationToken>()) },
        { ToolbarItemIds.OrderedList, States.Block("ol"), api => api.SetBlockTypeAsync("p", null, Arg.Any<CancellationToken>()) },
        { ToolbarItemIds.AlignLeft, States.Ready, api => api.SetBlockStyleAsync(TextAlign("left"), Arg.Any<CancellationToken>()) },
        { ToolbarItemIds.AlignCenter, States.Ready, api => api.SetBlockStyleAsync(TextAlign("center"), Arg.Any<CancellationToken>()) },
        { ToolbarItemIds.AlignRight, States.Ready, api => api.SetBlockStyleAsync(TextAlign("right"), Arg.Any<CancellationToken>()) },
        { ToolbarItemIds.Justify, States.Ready, api => api.SetBlockStyleAsync(TextAlign("justify"), Arg.Any<CancellationToken>()) },
        { ToolbarItemIds.ClearFormatting, States.Ready, api => api.ClearFormattingAsync(Arg.Any<CancellationToken>()) },
        { ToolbarItemIds.Undo, States.Ready, api => api.UndoAsync(Arg.Any<CancellationToken>()) },
        { ToolbarItemIds.Redo, States.Ready, api => api.RedoAsync(Arg.Any<CancellationToken>()) },
        { ToolbarItemIds.ViewSource, States.Ready, api => api.ToggleSourceViewAsync(Arg.Any<CancellationToken>()) },
        { ToolbarItemIds.ThemeToggle, States.Ready with { Theme = Theme.Light }, api => api.SetThemeAsync(Theme.Dark, Arg.Any<CancellationToken>()) },
        { ToolbarItemIds.ThemeToggle, States.Ready with { Theme = Theme.Dark }, api => api.SetThemeAsync(Theme.Light, Arg.Any<CancellationToken>()) },
        { ToolbarItemIds.ThemeToggle, States.Ready with { Theme = Theme.Auto }, api => api.SetThemeAsync(Theme.Dark, Arg.Any<CancellationToken>()) },
        { ToolbarItemIds.ThemeToggle, States.Ready with { Theme = Theme.Auto, PrefersDarkScheme = true }, api => api.SetThemeAsync(Theme.Light, Arg.Any<CancellationToken>()) },
        { ToolbarItemIds.Link, States.Ready, api => api.OpenPanelAsync<LinkPanel>(Arg.Any<IReadOnlyDictionary<string, object?>?>(), Arg.Any<CancellationToken>()) },
        { ToolbarItemIds.Table, States.Ready, api => api.OpenPanelAsync<TablePanel>(Arg.Any<IReadOnlyDictionary<string, object?>?>(), Arg.Any<CancellationToken>()) }
    };

    [Theory]
    [MemberData(nameof(Commands))]
    public async Task ExecuteRunsTheMatchingEditorCommand(string id, EditorState state, Action<IEditorApi> expected)
    {
        _api.State.Returns(state);

        var token = TestContext.Current.CancellationToken;

        await BuiltIn.Item(id).ExecuteAsync(_api, token);

        expected(_api.Received(1));
        SubstituteCalls.Commands(_api).ShouldHaveSingleItem().GetArguments().OfType<CancellationToken>().ShouldBe([token]);
    }

    [Fact]
    public async Task UnlinkSelectsTheWholeLinkBeforeUnwrapping()
    {
        await BuiltIn.Item(ToolbarItemIds.Unlink).ExecuteAsync(_api, TestContext.Current.CancellationToken);

        Received.InOrder(() =>
        {
            _api.SelectLinkAtCursorAsync(Arg.Any<CancellationToken>());
            _api.UnwrapSelectionAsync("a", Arg.Any<CancellationToken>());
        });
        SubstituteCalls.Commands(_api).Length.ShouldBe(2);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ImageOpensThePanelWithTheImageAtTheCursor(bool onImage)
    {
        var image = onImage ? new ImageInfo("/a.png", "A", null, 10, 20) : null;
        _api.GetImageAtCursorAsync(Arg.Any<CancellationToken>()).Returns(image);

        await BuiltIn.Item(ToolbarItemIds.Image).ExecuteAsync(_api, TestContext.Current.CancellationToken);

        var call = SubstituteCalls.To(_api, nameof(IEditorApi.OpenPanelAsync)).ShouldHaveSingleItem();
        call.GetMethodInfo().GetGenericArguments().ShouldBe([typeof(ImagePanel)]);
        var parameters = call.GetArguments()[0].ShouldBeAssignableTo<IReadOnlyDictionary<string, object?>>().ShouldNotBeNull();
        parameters[nameof(ImagePanel.Image)].ShouldBe(image);
    }

    [Fact]
    public async Task HeadingsDropdownDoesNothingItself()
    {
        await BuiltIn.Item(ToolbarItemIds.Headings).ExecuteAsync(_api, TestContext.Current.CancellationToken);

        SubstituteCalls.Commands(_api).ShouldBeEmpty();
    }

    private static IReadOnlyDictionary<string, string> TextAlign(string value) =>
        Arg.Is<IReadOnlyDictionary<string, string>>(styles => styles.Count == 1 && styles["text-align"] == value);
}
