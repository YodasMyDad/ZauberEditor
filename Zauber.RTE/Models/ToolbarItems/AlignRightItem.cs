namespace Zauber.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for right text alignment
/// </summary>
public class AlignRightItem : ToolbarItemBase
{
    public override string Id => "alignRight";
    public override string Label => "Align Right";
    public override string IconCss => "fa-align-right";
    public override ToolbarPlacement Placement => ToolbarPlacement.Block;

    public override bool IsActive(EditorState state) => state.CurrentAlignment == TextAlignment.Right;
    public override Task ExecuteAsync(EditorApi api) => api.SetBlockStyleAsync(new() { ["text-align"] = "right" });
}
