using ZauberCMS.RTE.Models;

namespace ZauberRTE.Web.Toolbar;

public sealed class TextColorDropdownItem : ToolbarItemBase
{
    public const string ItemId = "textColor";

    public override string Id => ItemId;

    public override string Label => "Text colour";

    public override string? IconSvg => DemoIcons.Palette;

    public override ToolbarItemKind Kind => ToolbarItemKind.Dropdown;

    public override IReadOnlyList<string> ChildItemIds { get; } =
        [.. TextColorItem.Palette.Select(item => item.Id), RemoveTextColorItem.ItemId];

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken) => Task.CompletedTask;
}
