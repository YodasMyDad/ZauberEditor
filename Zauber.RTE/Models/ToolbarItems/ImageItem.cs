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
    public override string IconCss => "fa-image";
    public override ToolbarPlacement Placement => ToolbarPlacement.Media;

    public override async Task ExecuteAsync(EditorApi api)
    {
        // Check if cursor is on an existing image before opening panel
        var existingImage = await api.GetImageAtCursorAsync();
        
        // Pass the image info as a parameter to the panel
        var parameters = existingImage != null 
            ? new Dictionary<string, object> { ["ExistingImage"] = existingImage }
            : null;
            
        await api.OpenPanelAsync(typeof(Zauber.RTE.Components.Panels.ImagePanel), parameters);
    }
}
