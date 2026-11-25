using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Toolbar;

internal sealed class UnlinkItem : ToolbarItemBase
{
    public override string Id => ToolbarItemIds.Unlink;

    public override string Label => "Remove link";

    public override string? IconSvg => Icons.Unlink;

    public override bool IsEnabled(EditorState state) => base.IsEnabled(state) && state.InLink;

    public override async Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken)
    {
        await api.SelectLinkAtCursorAsync(cancellationToken);
        await api.UnwrapSelectionAsync("a", cancellationToken);
    }
}
