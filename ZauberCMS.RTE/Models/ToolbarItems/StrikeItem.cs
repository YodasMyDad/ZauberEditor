namespace ZauberCMS.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for strikethrough formatting
/// </summary>
public class StrikeItem : ToolbarItemBase
{
    public override string Id => "strike";
    public override string Label => "Strikethrough";
    public override string Tooltip => "Strikethrough";
    public override string IconCss => "fa-strikethrough";
    public override ToolbarPlacement Placement => ToolbarPlacement.Inline;
    public override bool IsToggle => true;

    public override bool IsActive(EditorState state) => state.ActiveMarks.Contains("s");
    public override Task ExecuteAsync(IEditorApi api) => api.ToggleMarkAsync("s");
}
