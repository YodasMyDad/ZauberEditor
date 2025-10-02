namespace ZauberCMS.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for superscript
/// </summary>
public class SuperscriptItem : ToolbarItemBase
{
    public override string Id => "superscript";
    public override string Label => "Superscript";
    public override string IconCss => "fa-superscript";
    public override ToolbarPlacement Placement => ToolbarPlacement.Inline;
    public override bool IsToggle => true;
    public override string[] TrackedTags => ["sup"];
    public override string PrimaryTag => "sup";

    public override bool IsActive(EditorState state) => state.ActiveMarks.Contains("sup");
    public override Task ExecuteAsync(IEditorApi api) => api.ToggleMarkAsync("sup");
}
