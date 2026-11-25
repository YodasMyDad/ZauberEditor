using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Toolbar;

internal sealed class ColorItem(string parentId, string property, string name, string label, string? value) : ToolbarItemBase
{
    public override string Id => $"{parentId}-{name}";

    public override string Label => label;

    public override string? IconSvg { get; } = value is null ? Icons.NoColor : Icons.Swatch(value);

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken) =>
        api.SetTextStyleAsync(property, value, cancellationToken);
}
