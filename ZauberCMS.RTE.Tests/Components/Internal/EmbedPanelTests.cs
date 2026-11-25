using Bunit;
using ZauberCMS.RTE.Components.Internal;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Tests.Components.Internal;

public sealed class EmbedPanelTests : BunitContext
{
    private readonly IEditorApi _api = Substitute.For<IEditorApi>();

    public EmbedPanelTests()
    {
        _api.Settings.Returns(new EditorSettings());
        _api.InsertEmbedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
    }

    [Fact]
    public void HelpNamesTheSupportedProviders() =>
        Render<EmbedPanel>(parameters => parameters.AddCascadingValue(_api))
            .Find(".rte-form-help").TextContent.ShouldBe("Paste a link to a YouTube or Vimeo video.");

    [Theory]
    [InlineData("https://example.com/video/1")]
    [InlineData("https://www.youtube.com/channel/UC123")]
    [InlineData("javascript:alert(1)")]
    public async Task UnsupportedLinksShowAnErrorAndInsertNothing(string url)
    {
        var cut = Render<EmbedPanel>(parameters => parameters.AddCascadingValue(_api));

        cut.Find("input[inputmode=url]").Input(url);
        await cut.Find(".rte-panel-actions .rte-btn-primary").ClickAsync(new());

        var error = cut.Find(".rte-form-error");
        error.GetAttribute("role").ShouldBe("alert");
        error.TextContent.ShouldBe("That isn't a YouTube or Vimeo video link.");
        cut.Find("input[inputmode=url]").GetAttribute("aria-invalid").ShouldBe("true");
        await _api.DidNotReceive().InsertEmbedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _api.DidNotReceive().ClosePanelAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SupportedLinksAreEmbeddedAndThePanelCloses()
    {
        var cut = Render<EmbedPanel>(parameters => parameters.AddCascadingValue(_api));

        cut.Find("input[inputmode=url]").Input(" https://youtu.be/dQw4w9WgXcQ?t=5 ");
        await cut.Find(".rte-panel-actions .rte-btn-primary").ClickAsync(new());

        cut.Instance.Title.ShouldBe("Embed video");
        await _api.Received(1).InsertEmbedAsync("https://youtu.be/dQw4w9WgXcQ?t=5", Arg.Any<CancellationToken>());
        await _api.Received(1).ClosePanelAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EditingStartsFromTheEmbedsUrl()
    {
        var embed = new EmbedInfo("vimeo", "76979871", "https://vimeo.com/76979871");

        var cut = Render<EmbedPanel>(parameters => parameters.AddCascadingValue(_api).Add(panel => panel.Embed, embed));

        cut.Instance.Title.ShouldBe("Edit embed");
        cut.Find("input[inputmode=url]").GetAttribute("value").ShouldBe("https://vimeo.com/76979871");
        cut.Find(".rte-panel-actions .rte-btn-primary").TextContent.ShouldBe("Save");
        cut.Find("input[inputmode=url]").Input("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
        await cut.Find(".rte-panel-actions .rte-btn-primary").ClickAsync(new());
        await _api.Received(1).InsertEmbedAsync("https://www.youtube.com/watch?v=dQw4w9WgXcQ", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AnEmbedTheEditorRefusesKeepsThePanelOpen()
    {
        _api.InsertEmbedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        var cut = Render<EmbedPanel>(parameters => parameters.AddCascadingValue(_api));

        cut.Find("input[inputmode=url]").Input("https://vimeo.com/76979871");
        await cut.Find(".rte-panel-actions .rte-btn-primary").ClickAsync(new());

        cut.Find(".rte-form-error").TextContent.ShouldBe("This editor's HTML policy doesn't allow embeds.");
        await _api.DidNotReceive().ClosePanelAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task OnlyTheConfiguredProvidersAreAccepted()
    {
        _api.Settings.Returns(new EditorSettings { EmbedProviders = [EmbedProvider.Vimeo] });
        var cut = Render<EmbedPanel>(parameters => parameters.AddCascadingValue(_api));

        cut.Find(".rte-form-help").TextContent.ShouldBe("Paste a link to a Vimeo video.");
        cut.Find("input[inputmode=url]").Input("https://youtu.be/dQw4w9WgXcQ");
        await cut.Find(".rte-panel-actions .rte-btn-primary").ClickAsync(new());

        cut.Find(".rte-form-error").TextContent.ShouldBe("That isn't a Vimeo video link.");
        await _api.DidNotReceive().InsertEmbedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void NoProvidersMeansEmbedsAreOff()
    {
        _api.Settings.Returns(new EditorSettings { EmbedProviders = [] });

        Render<EmbedPanel>(parameters => parameters.AddCascadingValue(_api))
            .Find(".rte-form-help").TextContent.ShouldBe("Video embeds are turned off for this editor.");
    }
}
