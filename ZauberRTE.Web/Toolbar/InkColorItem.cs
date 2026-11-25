using ZauberCMS.RTE.Models;

namespace ZauberRTE.Web.Toolbar;

public sealed class InkColorItem(string color, string label) : ToolbarItemBase
{
    private const string Tag = "span";

    public static IReadOnlyList<InkColorItem> Palette { get; } =
    [
        new("red", "Red"),
        new("green", "Green"),
        new("blue", "Blue"),
        new("purple", "Purple")
    ];

    public override string Id => $"{InkColorDropdownItem.ItemId}-{color}";

    public override string Label => label;

    public override string? IconSvg => DemoIcons.Swatch(CssClass);

    private string CssClass => $"ink-{color}";

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(api);
        return api.WrapSelectionAsync(Tag, new Dictionary<string, string> { ["class"] = CssClass }, cancellationToken);
    }
}
