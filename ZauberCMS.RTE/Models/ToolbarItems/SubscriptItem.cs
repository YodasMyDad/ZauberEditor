namespace ZauberCMS.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for subscript
/// </summary>
public class SubscriptItem : ToolbarItemBase
{
    public override string Id => "subscript";
    public override string Label => "Subscript";
    public override string IconCss => "fa-subscript";
    public override ToolbarPlacement Placement => ToolbarPlacement.Inline;
    public override bool IsToggle => true;
    public override string[] TrackedTags => ["sub"];
    public override string PrimaryTag => "sub";

    public override bool IsActive(EditorState state) => state.ActiveMarks.Contains("sub");
    public override Task ExecuteAsync(IEditorApi api) => api.ToggleMarkAsync("sub");
}
