using Zauber.RTE.Services;
using Zauber.RTE.Components.Panels;

namespace Zauber.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for heading 1 block type
/// </summary>
public class Heading1Item : ToolbarItemBase
{
    public override string Id => "h1";
    public override string Label => "Heading 1";
    public override string IconClass => "fa-heading";
    public override ToolbarPlacement Placement => ToolbarPlacement.Block;
    public override bool IsToggle => true;

    public override bool IsActive(EditorState state) => state.CurrentBlockType == "heading" && state.CurrentHeadingLevel == 1;
    public override Task ExecuteAsync(EditorApi api) => api.SetBlockTypeAsync("heading", new() { ["level"] = "1" });
}
