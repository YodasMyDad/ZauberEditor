using Bunit;
using ZauberCMS.RTE.Components.Internal;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Tests.Components.Internal;

public sealed class SpecialCharactersPanelTests : BunitContext
{
    private readonly IEditorApi _api = Substitute.For<IEditorApi>();

    [Fact]
    public void EveryCharacterIsANamedButton()
    {
        var cut = Render<SpecialCharactersPanel>(parameters => parameters.AddCascadingValue(_api));

        var buttons = cut.FindAll("[role=group] button");
        buttons.Count.ShouldBe(29);
        buttons.ShouldAllBe(button => button.GetAttribute("type") == "button" && !string.IsNullOrWhiteSpace(button.GetAttribute("aria-label")));
    }

    [Fact]
    public async Task ChoosingACharacterInsertsItAndCloses()
    {
        var cut = Render<SpecialCharactersPanel>(parameters => parameters.AddCascadingValue(_api));

        await cut.Find("button[aria-label='Copyright sign']").ClickAsync(new());

        await _api.Received(1).InsertTextAsync("©", Arg.Any<CancellationToken>());
        await _api.Received(1).ClosePanelAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task NonBreakingSpaceIsLabelledAndInsertedAsTheCharacter()
    {
        var cut = Render<SpecialCharactersPanel>(parameters => parameters.AddCascadingValue(_api));
        var cell = cut.Find("button[aria-label='Non-breaking space']");

        cell.TextContent.Trim().ShouldBe("nbsp");
        await cell.ClickAsync(new());

        await _api.Received(1).InsertTextAsync(" ", Arg.Any<CancellationToken>());
    }
}
