using Zauber.RTE.Services;
using Zauber.RTE.Components.Panels;

namespace Zauber.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for viewing source
/// </summary>
public class ViewSourceItem : ToolbarItemBase
{
    public override string Id => "viewSource";
    public override string Label => "View Source";
    public override string IconClass => "fa-code";
    public override ToolbarPlacement Placement => ToolbarPlacement.Inline;
    public override bool IsToggle => true;

    public override bool IsActive(EditorState state) => state.IsSourceView;
    public override Task ExecuteAsync(EditorApi api) => api.ToggleSourceViewAsync();
}
