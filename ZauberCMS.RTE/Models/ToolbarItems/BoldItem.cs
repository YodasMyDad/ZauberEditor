namespace ZauberCMS.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for bold formatting
/// </summary>
public class BoldItem : ToolbarItemBase
{
    public override string Id => "bold";
    public override string Label => "Bold";
    public override string Tooltip => "Bold (Ctrl+B)";
    public override string IconCss => "fa-bold";
    public override string Shortcut => "Control+b";
    public override ToolbarPlacement Placement => ToolbarPlacement.Inline;
    public override bool IsToggle => true;
    public override string[] TrackedTags => ["strong", "b"];
    public override string PrimaryTag => "strong";
    public override bool IsActive(EditorState state) => state.ActiveMarks.Contains("strong");
    public override Task ExecuteAsync(IEditorApi api) => api.ToggleMarkAsync("strong");
}
