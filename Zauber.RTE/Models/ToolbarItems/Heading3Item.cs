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
    public override string IconClass => "fa-heading";
    public override ToolbarPlacement Placement => ToolbarPlacement.Block;
    public override bool IsToggle => true;

    public override bool IsActive(EditorState state) => state.CurrentBlockType == "heading" && state.CurrentHeadingLevel == 3;
    public override Task ExecuteAsync(EditorApi api) => api.SetBlockTypeAsync("heading", new() { ["level"] = "3" });
}
