using Zauber.RTE.Services;
using Zauber.RTE.Components.Panels;

namespace Zauber.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for subscript
/// </summary>
public class SubscriptItem : ToolbarItemBase
{
    public override string Id => "subscript";
    public override string Label => "Subscript";
    public override string IconClass => "fa-subscript";
    public override ToolbarPlacement Placement => ToolbarPlacement.Inline;
    public override bool IsToggle => true;

    public override bool IsActive(EditorState state) => state.ActiveMarks.Contains("sub");
    public override Task ExecuteAsync(EditorApi api) => api.ToggleMarkAsync("sub");
}
