namespace ZauberCMS.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for left text alignment
/// </summary>
public class AlignLeftItem : ToolbarItemBase
{
    public override string Id => "alignLeft";
    public override string Label => "Align Left";
    public override string IconCss => "fa-align-left";
    public override ToolbarPlacement Placement => ToolbarPlacement.Block;

    public override bool IsActive(EditorState state) => state.CurrentAlignment == TextAlignment.Left;
    public override Task ExecuteAsync(IEditorApi api) => api.SetBlockStyleAsync(new Dictionary<string, string> { ["text-align"] = "left" });
}
