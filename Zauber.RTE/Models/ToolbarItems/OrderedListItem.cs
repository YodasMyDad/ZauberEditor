using Zauber.RTE.Services;
using Zauber.RTE.Components.Panels;

namespace Zauber.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for ordered list
/// </summary>
public class OrderedListItem : ToolbarItemBase
{
    public override string Id => "ol";
    public override string Label => "Numbered List";
    public override string Tooltip => "Ordered List (Ctrl+Shift+7)";
    public override string IconClass => "fa-list-ol";
    public override ToolbarPlacement Placement => ToolbarPlacement.Block;
    public override bool IsToggle => true;

    public override bool IsActive(EditorState state) => state.CurrentBlockType == "ol";
    public override Task ExecuteAsync(EditorApi api) => api.SetBlockTypeAsync("ol");
}
