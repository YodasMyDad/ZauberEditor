using Bunit;
using ZauberCMS.RTE.Components.Internal;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Tests.Components.Internal;

public sealed class ImagePanelTests : BunitContext
{
    private readonly IEditorApi _api = Substitute.For<IEditorApi>();

    public ImagePanelTests() => _api.Settings.Returns(new EditorSettings());

    [Fact]
    public async Task UnsafeSourceIsRejected()
    {
        var cut = Render<ImagePanel>(parameters => parameters.AddCascadingValue(_api));

        cut.Find("input[inputmode=url]").Input("javascript:alert(1)");
        await cut.Find(".rte-panel-actions .rte-btn-primary").ClickAsync(new());

        cut.Find(".rte-form-error").GetAttribute("role").ShouldBe("alert");
        await _api.DidNotReceive().InsertHtmlAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InsertEncodesTheAlternativeText()
    {
        var cut = Render<ImagePanel>(parameters => parameters.AddCascadingValue(_api));

        cut.Find("input[inputmode=url]").Input("https://example.com/a.png");
        cut.Find("input[placeholder='Describe the image']").Input("\" onerror=\"alert(1)");
        await cut.Find(".rte-panel-actions .rte-btn-primary").ClickAsync(new());

        await _api.Received(1).InsertHtmlAsync(
            "<img src=\"https://example.com/a.png\" alt=\"&quot; onerror=&quot;alert(1)\">",
            Arg.Any<CancellationToken>());
        await _api.Received(1).ClosePanelAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EditingKeepsTheImageSize()
    {
        var image = new ImageInfo("https://example.com/a.png", "Old", null, 320, 200);

        var cut = Render<ImagePanel>(parameters => parameters.AddCascadingValue(_api).Add(panel => panel.Image, image));
        await cut.Find(".rte-panel-actions .rte-btn-primary").ClickAsync(new());

        cut.Instance.Title.ShouldBe("Edit image");
        await _api.Received(1).SelectImageAtCursorAsync(Arg.Any<CancellationToken>());
        await _api.Received(1).InsertHtmlAsync(
            "<img src=\"https://example.com/a.png\" alt=\"Old\" width=\"320\" height=\"200\">",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void FileUploadIsOfferedOnlyWhenAllowed()
    {
        Render<ImagePanel>(parameters => parameters.AddCascadingValue(_api)).FindAll(".rte-button-row").ShouldBeEmpty();

        _api.Settings.Returns(new EditorSettings { ImageConstraints = new() { AllowBase64ImageUpload = true } });

        Render<ImagePanel>(parameters => parameters.AddCascadingValue(_api)).FindAll(".rte-button-row button").Count.ShouldBe(2);
    }
}
