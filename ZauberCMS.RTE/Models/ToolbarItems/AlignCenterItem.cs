namespace ZauberCMS.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for center text alignment
/// </summary>
public class AlignCenterItem : ToolbarItemBase
{
    public override string Id => "alignCenter";
    public override string Label => "Align Center";
    public override string IconCss => "fa-align-center";
    public override ToolbarPlacement Placement => ToolbarPlacement.Block;

    public override bool IsActive(EditorState state) => state.CurrentAlignment == TextAlignment.Center;
    public override Task ExecuteAsync(IEditorApi api) => api.SetBlockStyleAsync(new Dictionary<string, string> { ["text-align"] = "center" });
}

