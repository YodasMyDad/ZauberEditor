using Zauber.RTE.Services;
using Zauber.RTE.Components.Panels;

namespace Zauber.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for inserting images
/// </summary>
public class ImageItem : ToolbarItemBase
{
    public override string Id => "image";
    public override string Label => "Image";
    public override string IconClass => "fa-image";
    public override ToolbarPlacement Placement => ToolbarPlacement.Media;

    public override Task ExecuteAsync(EditorApi api) => api.OpenPanelAsync(typeof(Zauber.RTE.Components.Panels.ImagePanel));
}
