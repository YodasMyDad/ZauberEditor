using Zauber.RTE.Services;
using Zauber.RTE.Components.Panels;

namespace Zauber.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for heading 2 block type
/// </summary>
public class Heading2Item : ToolbarItemBase
{
    public override string Id => "h2";
    public override string Label => "Heading 2";
    public override string IconClass => "fa-heading";
    public override ToolbarPlacement Placement => ToolbarPlacement.Block;
    public override bool IsToggle => true;

    public override bool IsActive(EditorState state) => state.CurrentBlockType == "heading" && state.CurrentHeadingLevel == 2;
    public override Task ExecuteAsync(EditorApi api) => api.SetBlockTypeAsync("heading", new() { ["level"] = "2" });
}
