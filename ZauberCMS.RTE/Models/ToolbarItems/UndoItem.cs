namespace ZauberCMS.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for undo
/// </summary>
public class UndoItem : ToolbarItemBase
{
    public override string Id => "undo";
    public override string Label => "Undo";
    public override string IconCss => "fa-undo";
    public override string Shortcut => "Control+z";
    public override ToolbarPlacement Placement => ToolbarPlacement.Inline;

    public override bool IsEnabled(EditorState state) => state.CanUndo;
    public override Task ExecuteAsync(IEditorApi api) => api.UndoAsync();
}
