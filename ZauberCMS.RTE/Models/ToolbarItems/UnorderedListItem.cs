namespace ZauberCMS.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for unordered list
/// </summary>
public class UnorderedListItem : ToolbarItemBase
{
    public override string Id => "ul";
    public override string Label => "Bullet List";
    public override string Tooltip => "Unordered List (Ctrl+Shift+8)";
    public override string IconCss => "fa-list-ul";
    public override string Shortcut => "Control+Shift+8";
    public override ToolbarPlacement Placement => ToolbarPlacement.Block;
    public override bool IsToggle => true;

    public override bool IsActive(EditorState state) => state.CurrentBlockType == "ul";
    public override Task ExecuteAsync(IEditorApi api) => api.SetBlockTypeAsync("ul");
}
