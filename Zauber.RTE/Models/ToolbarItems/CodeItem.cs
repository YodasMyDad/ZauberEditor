using Zauber.RTE.Services;
using Zauber.RTE.Components.Panels;

namespace Zauber.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for code formatting
/// </summary>
public class CodeItem : ToolbarItemBase
{
    public override string Id => "code";
    public override string Label => "Code";
    public override string Tooltip => "Code";
    public override string IconCss => "fa-code";
    public override ToolbarPlacement Placement => ToolbarPlacement.Inline;
    public override bool IsToggle => true;

    public override bool IsActive(EditorState state) => state.ActiveMarks.Contains("code");
    public override Task ExecuteAsync(EditorApi api) => api.ToggleMarkAsync("code");
}
