namespace ZauberCMS.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for viewing source
/// </summary>
public class ViewSourceItem : ToolbarItemBase
{
    public override string Id => "viewSource";
    public override string Label => "View Source";
    public override string IconCss => "fa-file-code";
    public override ToolbarPlacement Placement => ToolbarPlacement.Inline;
    public override bool IsToggle => true;

    public override bool IsActive(EditorState state) => state.IsSourceView;
    public override Task ExecuteAsync(IEditorApi api) => api.ToggleSourceViewAsync();
}
