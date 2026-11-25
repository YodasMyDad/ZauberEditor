using System.ComponentModel;
using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Components.Internal;

/// <summary>Inserts or edits an image. Opened by the built-in image item; not for direct use.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed partial class ImagePanel
{
    private string _url = string.Empty;
    private string _dataUrl = string.Empty;
    private string _alt = string.Empty;
    private string? _error;
    private bool _fromFile;

    /// <summary>The image being edited, or null to insert a new one.</summary>
    [Parameter]
    public ImageInfo? Image { get; set; }

    /// <inheritdoc />
    public override string Title => Image is null ? "Insert image" : "Edit image";

    private ImageConstraints Constraints => Api.Settings.ImageConstraints;

    private bool AllowUpload => Constraints.AllowBase64ImageUpload && Api.Settings.HtmlPolicy.AllowDataUrls;

    private string Accept => string.Join(',', Constraints.AllowedImageTypes);

    private string Source => (_fromFile ? _dataUrl : _url).Trim();

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        if (Image is null)
        {
            return;
        }

        _alt = Image.Alt ?? string.Empty;
        _fromFile = Image.Src.StartsWith("data:", StringComparison.OrdinalIgnoreCase);
        (_fromFile ? ref _dataUrl : ref _url) = Image.Src;
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && Image is not null)
        {
            await Api.SelectImageAtCursorAsync();
        }
    }

    private async Task OnFileSelectedAsync(InputFileChangeEventArgs args)
    {
        _error = null;
        var file = args.File;
        if (!Constraints.AllowedImageTypes.Contains(Path.GetExtension(file.Name))
            || !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            _error = $"Choose a {string.Join(", ", Constraints.AllowedImageTypes)} file.";
            return;
        }

        if (file.Size > Constraints.MaxFileSizeBytes)
        {
            _error = string.Create(
                CultureInfo.InvariantCulture,
                $"The file is larger than the {Constraints.MaxFileSizeBytes / 1024d / 1024d:0.#} MB limit.");
            return;
        }

        await using var stream = file.OpenReadStream(Constraints.MaxFileSizeBytes);
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer);
        _dataUrl = $"data:{file.ContentType};base64,{Convert.ToBase64String(buffer.GetBuffer(), 0, (int)buffer.Length)}";
    }

    private async Task SaveAsync()
    {
        var source = Source;
        if (source.Length == 0)
        {
            return;
        }

        if (!_fromFile && !UrlRules.HasAllowedScheme(source, UrlRules.ImageSchemes))
        {
            _error = "Use an http or https address, or a relative path.";
            return;
        }

        var image = HtmlBuilder.Image(source, _alt.Trim())
            .Attr("width", Image?.Width?.ToString(CultureInfo.InvariantCulture))
            .Attr("height", Image?.Height?.ToString(CultureInfo.InvariantCulture));

        await Api.InsertHtmlAsync(image.Build());
        await CloseAsync();
    }
}
