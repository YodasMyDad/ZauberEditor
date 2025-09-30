using Zauber.RTE.Services;
using Zauber.RTE.Components.Panels;

namespace Zauber.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for heading 3 block type
/// </summary>
public class Heading3Item : ToolbarItemBase
{
    public override string Id => "h3";
    public override string Label => "Heading 3";
    public override string IconCss => "fa-heading";
    public override ToolbarPlacement Placement => ToolbarPlacement.Block;
    public override bool IsToggle => true;

    public override bool IsActive(EditorState state) => state.CurrentBlockType == "heading" && state.CurrentHeadingLevel == 3;
    public override async Task ExecuteAsync(EditorApi api)
    {
        // Toggle: if already H3, convert to paragraph
        if (IsActive(api.GetState()))
        {
            await api.SetBlockTypeAsync("p", null);
        }
        else
        {
            await api.SetBlockTypeAsync("h3", null);
        }
    }
}
