using System.Globalization;
using ZauberCMS.RTE.Models;

namespace ZauberRTE.Web.Toolbar;

public sealed class InsertTimeItem(TimeProvider timeProvider) : ToolbarItemBase
{
    public const string ItemId = "insertTime";

    public override string Id => ItemId;

    public override string Label => "Insert current time";

    public override string? IconSvg => DemoIcons.Clock;

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(api);
        return api.InsertTextAsync(timeProvider.GetLocalNow().ToString("HH:mm:ss", CultureInfo.InvariantCulture), cancellationToken);
    }
}
