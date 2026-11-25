using ZauberCMS.RTE.Components.Internal;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Toolbar;

internal sealed class FindReplaceItem : ToolbarItemBase
{
    public override string Id => ToolbarItemIds.FindReplace;

    public override string Label => "Find and replace";

    public override string? IconSvg => Icons.Search;

    public override string? Shortcut => "Control+h";

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken) =>
        api.OpenPanelAsync<FindReplacePanel>(cancellationToken: cancellationToken);
}
