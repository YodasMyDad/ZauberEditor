using ZauberCMS.RTE.Models;

namespace ZauberRTE.Web.Toolbar;

public sealed class RemoveTextColorItem : ToolbarItemBase
{
    public const string ItemId = $"{TextColorDropdownItem.ItemId}-none";

    public override string Id => ItemId;

    public override string Label => "Default colour";

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(api);
        return api.UnwrapSelectionAsync("span", cancellationToken);
    }
}
