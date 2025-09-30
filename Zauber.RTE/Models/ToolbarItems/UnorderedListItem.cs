using Zauber.RTE.Services;
using Zauber.RTE.Components.Panels;

namespace Zauber.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for unordered list
/// </summary>
public class UnorderedListItem : ToolbarItemBase
{
    public override string Id => "ul";
    public override string Label => "Bullet List";
    public override string Tooltip => "Unordered List (Ctrl+Shift+8)";
    public override string IconCss => "fa-list-ul";
    public override ToolbarPlacement Placement => ToolbarPlacement.Block;
    public override bool IsToggle => true;

    public override bool IsActive(EditorState state) => state.CurrentBlockType == "ul";
    public override Task ExecuteAsync(EditorApi api) => api.SetBlockTypeAsync("ul");
}
