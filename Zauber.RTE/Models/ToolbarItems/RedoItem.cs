using Zauber.RTE.Services;
using Zauber.RTE.Components.Panels;

namespace Zauber.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for redo
/// </summary>
public class RedoItem : ToolbarItemBase
{
    public override string Id => "redo";
    public override string Label => "Redo";
    public override string IconCss => "fa-redo";
    public override string Shortcut => "Control+y";
    public override ToolbarPlacement Placement => ToolbarPlacement.Inline;

    public override bool IsEnabled(EditorState state) => state.CanRedo;
    public override Task ExecuteAsync(EditorApi api) => api.RedoAsync();
}
