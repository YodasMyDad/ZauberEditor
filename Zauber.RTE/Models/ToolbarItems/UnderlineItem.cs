using Zauber.RTE.Services;
using Zauber.RTE.Components.Panels;

namespace Zauber.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for underline formatting
/// </summary>
public class UnderlineItem : ToolbarItemBase
{
    public override string Id => "underline";
    public override string Label => "Underline";
    public override string Tooltip => "Underline (Ctrl+U)";
    public override string IconCss => "fa-underline";
    public override string Shortcut => "Control+u";
    public override ToolbarPlacement Placement => ToolbarPlacement.Inline;
    public override bool IsToggle => true;

    public override bool IsActive(EditorState state) => state.ActiveMarks.Contains("u");
    public override Task ExecuteAsync(EditorApi api) => api.ToggleMarkAsync("u");
}
