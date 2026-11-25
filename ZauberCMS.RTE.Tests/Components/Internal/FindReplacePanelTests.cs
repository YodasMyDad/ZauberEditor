using Bunit;
using ZauberCMS.RTE.Components.Internal;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Tests.Components.Internal;

public sealed class FindReplacePanelTests : BunitContext
{
    private readonly IEditorApi _api = Substitute.For<IEditorApi>();

    [Fact]
    public void SelectedTextPrefillsTheSearch()
    {
        _api.GetSelectionAsync(Arg.Any<CancellationToken>()).Returns(new SelectionInfo("fox", false));

        var cut = Render<FindReplacePanel>(parameters => parameters.AddCascadingValue(_api));

        cut.WaitForAssertion(() => cut.FindAll("input[type=text]")[0].GetAttribute("value").ShouldBe("fox"));
    }

    [Fact]
    public void ActionsNeedSomethingToFind()
    {
        var cut = Render<FindReplacePanel>(parameters => parameters.AddCascadingValue(_api));

        cut.FindAll(".rte-panel-actions button").ShouldAllBe(button => button.HasAttribute("disabled"));
    }

    [Fact]
    public async Task FindNextShowsThePositionOfTheMatch()
    {
        _api.FindAsync("the", true, Arg.Any<CancellationToken>()).Returns(new FindResult(4, 2));
        var cut = Render<FindReplacePanel>(parameters => parameters.AddCascadingValue(_api));

        cut.FindAll("input[type=text]")[0].Input("the");
        cut.Find("input[type=checkbox]").Change(true);
        await cut.Find("form").SubmitAsync();

        cut.Find("[role=status]").TextContent.ShouldBe("Match 2 of 4.");
    }

    [Fact]
    public async Task ReplaceUsesTheReplacementText()
    {
        _api.ReplaceAsync("the", "a", false, Arg.Any<CancellationToken>()).Returns(FindResult.None);
        var cut = Render<FindReplacePanel>(parameters => parameters.AddCascadingValue(_api));

        cut.FindAll("input[type=text]")[0].Input("the");
        cut.FindAll("input[type=text]")[1].Input("a");
        await cut.FindAll(".rte-panel-actions button")[1].ClickAsync(new());

        cut.Find("[role=status]").TextContent.ShouldBe("No matches.");
    }

    [Fact]
    public async Task ReplaceAllReportsTheCount()
    {
        _api.ReplaceAllAsync("the", string.Empty, false, Arg.Any<CancellationToken>()).Returns(3);
        var cut = Render<FindReplacePanel>(parameters => parameters.AddCascadingValue(_api));

        cut.FindAll("input[type=text]")[0].Input("the");
        await cut.FindAll(".rte-panel-actions button")[0].ClickAsync(new());

        cut.Find("[role=status]").TextContent.ShouldBe("Replaced 3 matches.");
    }

    [Fact]
    public async Task ChangingTheSearchClearsTheStatus()
    {
        _api.FindAsync("the", false, Arg.Any<CancellationToken>()).Returns(new FindResult(1, 1));
        var cut = Render<FindReplacePanel>(parameters => parameters.AddCascadingValue(_api));
        cut.FindAll("input[type=text]")[0].Input("the");
        await cut.Find("form").SubmitAsync();

        cut.FindAll("input[type=text]")[0].Input("them");

        cut.Find("[role=status]").TextContent.ShouldBeEmpty();
    }
}
