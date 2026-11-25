using ZauberCMS.RTE.Models;

namespace ZauberRTE.Web.Toolbar;

public sealed class CalloutItem : ToolbarItemBase
{
    public const string ItemId = "callout";

    public override string Id => ItemId;

    public override string Label => "Insert callout";

    public override string? IconSvg => DemoIcons.Callout;

    public override async Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(api);
        await api.SaveSelectionAsync(cancellationToken);
        await api.OpenPanelAsync<CalloutPanel>(cancellationToken: cancellationToken);
    }
}
