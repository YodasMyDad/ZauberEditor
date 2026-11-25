using ZauberCMS.RTE.Models;

namespace ZauberRTE.Web.Toolbar;

public sealed class RemoveInkColorItem : ToolbarItemBase
{
    public const string ItemId = $"{InkColorDropdownItem.ItemId}-none";

    public override string Id => ItemId;

    public override string Label => "Default ink";

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(api);
        return api.UnwrapSelectionAsync("span", cancellationToken);
    }
}
