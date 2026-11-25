using ZauberCMS.RTE.Models;

namespace ZauberRTE.Web.Toolbar;

public sealed class InkColorDropdownItem : ToolbarItemBase
{
    public const string ItemId = "inkColor";

    public override string Id => ItemId;

    public override string Label => "Ink colour";

    public override string? IconSvg => DemoIcons.Palette;

    public override ToolbarItemKind Kind => ToolbarItemKind.Dropdown;

    public override IReadOnlyList<string> ChildItemIds { get; } =
        [.. InkColorItem.Palette.Select(item => item.Id), RemoveInkColorItem.ItemId];

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken) => Task.CompletedTask;
}
