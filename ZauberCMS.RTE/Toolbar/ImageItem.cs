using ZauberCMS.RTE.Components.Internal;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Toolbar;

internal sealed class ImageItem : ToolbarItemBase
{
    public override string Id => ToolbarItemIds.Image;

    public override string Label => "Image";

    public override string? IconSvg => Icons.Image;

    public override bool IsToggle => true;

    public override bool IsActive(EditorState state) => state.InImage;

    public override async Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken)
    {
        Dictionary<string, object?> parameters = new()
        {
            [nameof(ImagePanel.Image)] = await api.GetImageAtCursorAsync(cancellationToken)
        };
        await api.OpenPanelAsync<ImagePanel>(parameters, cancellationToken);
    }
}
