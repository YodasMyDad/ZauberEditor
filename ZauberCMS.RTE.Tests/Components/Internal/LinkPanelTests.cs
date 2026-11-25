using Bunit;
using ZauberCMS.RTE.Components.Internal;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Tests.Components.Internal;

public sealed class LinkPanelTests : BunitContext
{
    private readonly IEditorApi _api = Substitute.For<IEditorApi>();

    [Fact]
    public async Task ExistingLinkIsLoadedAndSelected()
    {
        _api.GetLinkAtCursorAsync(Arg.Any<CancellationToken>()).Returns(new LinkInfo("https://example.com", "Example", "Tip", "_blank"));

        var cut = Render<LinkPanel>(parameters => parameters.AddCascadingValue(_api));

        cut.Find("input[inputmode=url]").GetAttribute("value").ShouldBe("https://example.com");
        cut.Find("input[placeholder='Link text']").GetAttribute("value").ShouldBe("Example");
        cut.Find("select").GetAttribute("value").ShouldBe("_blank");
        await _api.Received(1).SelectLinkAtCursorAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public void SelectedTextBecomesTheLinkText()
    {
        _api.GetSelectionAsync(Arg.Any<CancellationToken>()).Returns(new SelectionInfo("chosen words", IsCollapsed: false));

        var cut = Render<LinkPanel>(parameters => parameters.AddCascadingValue(_api));

        cut.Find("input[placeholder='Link text']").GetAttribute("value").ShouldBe("chosen words");
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:text/html,<script>alert(1)</script>")]
    public async Task UnsafeUrlIsRejected(string url)
    {
        var cut = Render<LinkPanel>(parameters => parameters.AddCascadingValue(_api));

        cut.Find("input[inputmode=url]").Input(url);
        await cut.Find(".rte-panel-actions .rte-btn-primary").ClickAsync(new());

        cut.Find(".rte-form-error").GetAttribute("role").ShouldBe("alert");
        cut.Find("input[inputmode=url]").GetAttribute("aria-invalid").ShouldBe("true");
        await _api.DidNotReceive().InsertHtmlAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _api.DidNotReceive().ClosePanelAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveInsertsAnEncodedLinkAndCloses()
    {
        var cut = Render<LinkPanel>(parameters => parameters.AddCascadingValue(_api));

        cut.Find("input[inputmode=url]").Input(" https://example.com/?a=1&b=\"x\" ");
        cut.Find("input[placeholder='Link text']").Input("<b>hi</b>");
        cut.Find("select").Change("_blank");
        await cut.Find(".rte-panel-actions .rte-btn-primary").ClickAsync(new());

        await _api.Received(1).InsertHtmlAsync(
            "<a href=\"https://example.com/?a=1&amp;b=&quot;x&quot;\" target=\"_blank\" rel=\"noopener noreferrer\">&lt;b&gt;hi&lt;/b&gt;</a>",
            Arg.Any<CancellationToken>());
        await _api.Received(1).ClosePanelAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UnchangedSelectedTextIsWrappedSoItKeepsItsFormatting()
    {
        _api.GetSelectionAsync(Arg.Any<CancellationToken>()).Returns(new SelectionInfo("Hello world", IsCollapsed: false));
        var cut = Render<LinkPanel>(parameters => parameters.AddCascadingValue(_api));

        cut.Find("input[inputmode=url]").Input("https://example.com");
        cut.Find("input[placeholder='Optional']").Input("Tip");
        await cut.Find("form").SubmitAsync();

        await _api.Received(1).WrapSelectionAsync(
            "a",
            Arg.Is<IReadOnlyDictionary<string, string>>(attributes =>
                attributes.Count == 2 && attributes["href"] == "https://example.com" && attributes["title"] == "Tip"),
            Arg.Any<CancellationToken>());
        await _api.DidNotReceive().InsertHtmlAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _api.Received(1).ClosePanelAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EditingAnExistingLinkUpdatesItInPlace()
    {
        _api.GetLinkAtCursorAsync(Arg.Any<CancellationToken>()).Returns(new LinkInfo("https://old.example", "Example", null, "_blank"));
        var cut = Render<LinkPanel>(parameters => parameters.AddCascadingValue(_api));

        cut.Find("input[inputmode=url]").Input("https://new.example");
        await cut.Find("form").SubmitAsync();

        await _api.Received(1).WrapSelectionAsync(
            "a",
            Arg.Is<IReadOnlyDictionary<string, string>>(attributes =>
                attributes["href"] == "https://new.example" && attributes["target"] == "_blank" && attributes["rel"] == "noopener noreferrer"),
            Arg.Any<CancellationToken>());
        await _api.DidNotReceive().InsertHtmlAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ChangedTextReplacesTheSelection()
    {
        _api.GetSelectionAsync(Arg.Any<CancellationToken>()).Returns(new SelectionInfo("Hello world", IsCollapsed: false));
        var cut = Render<LinkPanel>(parameters => parameters.AddCascadingValue(_api));

        cut.Find("input[inputmode=url]").Input("/about");
        cut.Find("input[placeholder='Link text']").Input("About us");
        await cut.Find("form").SubmitAsync();

        await _api.Received(1).InsertHtmlAsync("<a href=\"/about\">About us</a>", Arg.Any<CancellationToken>());
        await _api.DidNotReceive().WrapSelectionAsync(Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, string>?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SubmittingWithoutAUrlDoesNothing()
    {
        var cut = Render<LinkPanel>(parameters => parameters.AddCascadingValue(_api));

        await cut.Find("form").SubmitAsync();

        await _api.DidNotReceive().InsertHtmlAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _api.DidNotReceive().ClosePanelAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public void SaveIsTheFormSubmitButtonAndCancelIsNot()
    {
        var cut = Render<LinkPanel>(parameters => parameters.AddCascadingValue(_api));

        cut.Find("form.rte-panel-content .rte-btn-primary").GetAttribute("type").ShouldBe("submit");
        cut.Find("form.rte-panel-content .rte-btn-secondary").GetAttribute("type").ShouldBe("button");
    }

    [Fact]
    public async Task EmptyTextFallsBackToTheUrl()
    {
        var cut = Render<LinkPanel>(parameters => parameters.AddCascadingValue(_api));

        cut.Find("input[inputmode=url]").Input("/about");
        await cut.Find(".rte-panel-actions .rte-btn-primary").ClickAsync(new());

        await _api.Received(1).InsertHtmlAsync("<a href=\"/about\">/about</a>", Arg.Any<CancellationToken>());
    }
}
