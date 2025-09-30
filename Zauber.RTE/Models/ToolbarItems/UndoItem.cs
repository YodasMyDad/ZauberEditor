using Zauber.RTE.Services;
using Zauber.RTE.Components.Panels;

namespace Zauber.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for undo
/// </summary>
public class UndoItem : ToolbarItemBase
{
    public override string Id => "undo";
    public override string Label => "Undo";
    public override string IconClass => "fa-undo";
    public override ToolbarPlacement Placement => ToolbarPlacement.Inline;

    public override bool IsEnabled(EditorState state) => state.CanUndo;
    public override Task ExecuteAsync(EditorApi api) => api.UndoAsync();
}
