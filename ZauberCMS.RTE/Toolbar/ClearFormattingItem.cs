using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Toolbar;

internal sealed class ClearFormattingItem : ToolbarItemBase
{
    public override string Id => ToolbarItemIds.ClearFormatting;

    public override string Label => "Clear formatting";

    public override string? IconSvg => Icons.Eraser;

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken) => api.ClearFormattingAsync(cancellationToken);
}
