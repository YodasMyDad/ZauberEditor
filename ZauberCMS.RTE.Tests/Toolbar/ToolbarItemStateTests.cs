using ZauberCMS.RTE.Models;
using ZauberCMS.RTE.Tests.TestSupport;
using ZauberCMS.RTE.Toolbar;

namespace ZauberCMS.RTE.Tests.Toolbar;

public sealed class ToolbarItemStateTests
{
    public static TheoryData<string, EditorState, bool> EnabledCases => new()
    {
        { ToolbarItemIds.Bold, States.Ready, true },
        { ToolbarItemIds.Bold, States.Ready with { IsReadOnly = true }, false },
        { ToolbarItemIds.Bold, States.Ready with { IsSourceView = true }, false },
        { ToolbarItemIds.Code, States.Ready with { IsReadOnly = true }, false },
        { ToolbarItemIds.Heading2, States.Ready with { IsSourceView = true }, false },
        { ToolbarItemIds.Headings, States.Ready with { IsReadOnly = true }, false },
        { ToolbarItemIds.AlignCenter, States.Ready with { IsSourceView = true }, false },
        { ToolbarItemIds.Link, States.Ready, true },
        { ToolbarItemIds.Link, States.Ready with { IsReadOnly = true }, false },
        { ToolbarItemIds.Image, States.Ready with { IsSourceView = true }, false },
        { ToolbarItemIds.Table, States.Ready, true },
        { ToolbarItemIds.Table, States.Ready with { IsReadOnly = true }, false },
        { ToolbarItemIds.ClearFormatting, States.Ready with { IsSourceView = true }, false },
        { ToolbarItemIds.Undo, States.Ready, false },
        { ToolbarItemIds.Undo, States.Ready with { CanUndo = true }, true },
        { ToolbarItemIds.Undo, States.Ready with { CanUndo = true, IsReadOnly = true }, false },
        { ToolbarItemIds.Undo, States.Ready with { CanUndo = true, IsSourceView = true }, false },
        { ToolbarItemIds.Redo, States.Ready, false },
        { ToolbarItemIds.Redo, States.Ready with { CanRedo = true }, true },
        { ToolbarItemIds.Redo, States.Ready with { CanRedo = true, IsReadOnly = true }, false },
        { ToolbarItemIds.Unlink, States.Ready, false },
        { ToolbarItemIds.Unlink, States.Ready with { InLink = true }, true },
        { ToolbarItemIds.Unlink, States.Ready with { InLink = true, IsSourceView = true }, false },
        { ToolbarItemIds.ViewSource, States.Ready, true },
        { ToolbarItemIds.ViewSource, States.Ready with { IsSourceView = true }, true },
        { ToolbarItemIds.ViewSource, States.Ready with { IsReadOnly = true }, false },
        { ToolbarItemIds.ThemeToggle, States.Ready with { IsReadOnly = true, IsSourceView = true }, true }
    };

    public static TheoryData<string, EditorState, bool> ActiveCases => new()
    {
        { ToolbarItemIds.Bold, States.Marks("strong"), true },
        { ToolbarItemIds.Bold, States.Marks("STRONG"), true },
        { ToolbarItemIds.Bold, States.Marks("em"), false },
        { ToolbarItemIds.Bold, States.Ready, false },
        { ToolbarItemIds.Italic, States.Marks("strong", "em"), true },
        { ToolbarItemIds.Underline, States.Marks("u"), true },
        { ToolbarItemIds.Strike, States.Marks("s"), true },
        { ToolbarItemIds.Subscript, States.Marks("sup"), false },
        { ToolbarItemIds.Superscript, States.Marks("sup"), true },
        { ToolbarItemIds.Code, States.Marks("code"), true },
        { ToolbarItemIds.Code, States.Block("pre"), true },
        { ToolbarItemIds.Code, States.Ready, false },
        { ToolbarItemIds.Heading1, States.Block("h1"), true },
        { ToolbarItemIds.Heading1, States.Block("h2"), false },
        { ToolbarItemIds.Heading6, States.Block("h6"), true },
        { ToolbarItemIds.Headings, States.Block("h4"), true },
        { ToolbarItemIds.Headings, States.Block("p"), false },
        { ToolbarItemIds.Headings, States.Block("blockquote"), false },
        { ToolbarItemIds.Blockquote, States.Block("blockquote"), true },
        { ToolbarItemIds.Blockquote, States.Ready, false },
        { ToolbarItemIds.UnorderedList, States.Block("ul"), true },
        { ToolbarItemIds.UnorderedList, States.Block("ol"), false },
        { ToolbarItemIds.OrderedList, States.Block("ol"), true },
        { ToolbarItemIds.AlignLeft, States.Ready, true },
        { ToolbarItemIds.AlignLeft, States.Ready with { Alignment = TextAlignment.Center }, false },
        { ToolbarItemIds.AlignCenter, States.Ready with { Alignment = TextAlignment.Center }, true },
        { ToolbarItemIds.AlignRight, States.Ready with { Alignment = TextAlignment.Right }, true },
        { ToolbarItemIds.Justify, States.Ready with { Alignment = TextAlignment.Justify }, true },
        { ToolbarItemIds.Justify, States.Ready, false },
        { ToolbarItemIds.Link, States.Ready with { InLink = true }, true },
        { ToolbarItemIds.Link, States.Ready, false },
        { ToolbarItemIds.Image, States.Ready with { InImage = true }, true },
        { ToolbarItemIds.Image, States.Ready, false },
        { ToolbarItemIds.ViewSource, States.Ready with { IsSourceView = true }, true },
        { ToolbarItemIds.ViewSource, States.Ready, false },
        { ToolbarItemIds.ThemeToggle, States.Ready with { Theme = Theme.Dark }, true },
        { ToolbarItemIds.ThemeToggle, States.Ready with { Theme = Theme.Light, PrefersDarkScheme = true }, false },
        { ToolbarItemIds.ThemeToggle, States.Ready with { Theme = Theme.Auto, PrefersDarkScheme = true }, true },
        { ToolbarItemIds.ThemeToggle, States.Ready with { Theme = Theme.Auto }, false },
        { ToolbarItemIds.Undo, States.Ready with { CanUndo = true }, false },
        { ToolbarItemIds.Unlink, States.Ready with { InLink = true }, false },
        { ToolbarItemIds.Table, States.Ready with { InTable = true }, false },
        { ToolbarItemIds.ClearFormatting, States.Marks("strong"), false }
    };

    [Theory]
    [MemberData(nameof(EnabledCases))]
    public void IsEnabledFollowsTheEditorState(string id, EditorState state, bool expected) =>
        BuiltIn.Item(id).IsEnabled(state).ShouldBe(expected);

    [Theory]
    [MemberData(nameof(ActiveCases))]
    public void IsActiveFollowsTheEditorState(string id, EditorState state, bool expected) =>
        BuiltIn.Item(id).IsActive(state).ShouldBe(expected);

    [Fact]
    public void ItemsThatCanBeActiveAreToggles()
    {
        EditorState[] states =
        [
            States.Marks("strong", "em", "u", "s", "sub", "sup", "code"),
            States.Block("h1"), States.Block("h2"), States.Block("h3"), States.Block("h4"), States.Block("h5"), States.Block("h6"),
            States.Block("blockquote"), States.Block("ul"), States.Block("ol"), States.Block("pre"),
            States.Ready with { Alignment = TextAlignment.Center }, States.Ready with { Alignment = TextAlignment.Right },
            States.Ready with { Alignment = TextAlignment.Justify },
            States.Ready with { InLink = true, InImage = true, IsSourceView = true, Theme = Theme.Dark }
        ];

        BuiltInToolbarItems.All.Where(item => states.Any(item.IsActive)).ShouldAllBe(item => item.IsToggle);
    }
}
