using Zauber.RTE.Services;
using Zauber.RTE.Components.Panels;

namespace Zauber.RTE.Models.ToolbarItems;

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

    public override bool IsActive(EditorState state) => state.ActiveMarks.Contains("sup");
    public override Task ExecuteAsync(EditorApi api) => api.ToggleMarkAsync("sup");
}
