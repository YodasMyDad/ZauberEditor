using ZauberCMS.RTE.Models;

namespace ZauberRTE.Web.Toolbar;

public sealed class HighlightItem : ToolbarItemBase
{
    public const string ItemId = "highlight";
    private const string Tag = "mark";

    public override string Id => ItemId;

    public override string Label => "Highlight";

    public override string? IconSvg => DemoIcons.Highlighter;

    public override string? Shortcut => "Control+Shift+H";

    public override bool IsToggle => true;

    public override IReadOnlyList<string> TrackedTags { get; } = [Tag];

    public override string? PrimaryTag => Tag;

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(api);
        return api.ToggleMarkAsync(Tag, cancellationToken);
    }
}
