using Zauber.RTE.Services;
using Zauber.RTE.Components.Panels;

namespace Zauber.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for bold formatting
/// </summary>
public class BoldItem : ToolbarItemBase
{
    public override string Id => "bold";
    public override string Label => "Bold";
    public override string Tooltip => "Bold (Ctrl+B)";
    public override string IconClass => "fa-bold";
    public override ToolbarPlacement Placement => ToolbarPlacement.Inline;
    public override bool IsToggle => true;

    public override bool IsActive(EditorState state) => state.ActiveMarks.Contains("strong");
    public override Task ExecuteAsync(EditorApi api) => api.ToggleMarkAsync("strong");
}
