using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Toolbar;

internal sealed class ColorDropdownItem(string id, string label, string iconSvg, IReadOnlyList<ColorItem> colors) : ToolbarItemBase
{
    public override string Id => id;

    public override string Label => label;

    public override string? IconSvg => iconSvg;

    public override ToolbarItemKind Kind => ToolbarItemKind.Dropdown;

    public override IReadOnlyList<string> ChildItemIds { get; } = [.. colors.Select(color => color.Id)];

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken) => Task.CompletedTask;
}
