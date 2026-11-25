using ZauberCMS.RTE.Components.Internal;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Toolbar;

internal sealed class LinkItem : ToolbarItemBase
{
    public override string Id => ToolbarItemIds.Link;

    public override string Label => "Link";

    public override string? IconSvg => Icons.Link;

    public override string? Shortcut => "Control+k";

    public override bool IsToggle => true;

    public override bool IsActive(EditorState state) => state.InLink;

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken) =>
        api.OpenPanelAsync<LinkPanel>(cancellationToken: cancellationToken);
}
