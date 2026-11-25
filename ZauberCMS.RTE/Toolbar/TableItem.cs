using ZauberCMS.RTE.Components.Internal;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Toolbar;

internal sealed class TableItem : ToolbarItemBase
{
    public override string Id => ToolbarItemIds.Table;

    public override string Label => "Table";

    public override string? IconSvg => Icons.Table;

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken) =>
        api.OpenPanelAsync<TablePanel>(cancellationToken: cancellationToken);
}
