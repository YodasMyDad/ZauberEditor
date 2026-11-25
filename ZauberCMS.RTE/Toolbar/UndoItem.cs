using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Toolbar;

internal sealed class UndoItem : ToolbarItemBase
{
    public override string Id => ToolbarItemIds.Undo;

    public override string Label => "Undo";

    public override string? IconSvg => Icons.Undo;

    public override string? Shortcut => "Control+z";

    public override bool IsEnabled(EditorState state) => base.IsEnabled(state) && state.CanUndo;

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken) => api.UndoAsync(cancellationToken);
}
