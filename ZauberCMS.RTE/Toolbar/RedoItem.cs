using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Toolbar;

internal sealed class RedoItem : ToolbarItemBase
{
    public override string Id => ToolbarItemIds.Redo;

    public override string Label => "Redo";

    public override string? IconSvg => Icons.Redo;

    public override string? Shortcut => "Control+y";

    public override bool IsEnabled(EditorState state) => base.IsEnabled(state) && state.CanRedo;

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken) => api.RedoAsync(cancellationToken);
}
