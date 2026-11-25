using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Toolbar;

internal sealed class MarkItem(
    string id,
    string label,
    string iconSvg,
    string primaryTag,
    IReadOnlyList<string> trackedTags,
    string? shortcut = null) : ToolbarItemBase
{
    public override string Id => id;

    public override string Label => label;

    public override string? IconSvg => iconSvg;

    public override string? Shortcut => shortcut;

    public override bool IsToggle => true;

    public override IReadOnlyList<string> TrackedTags => trackedTags;

    public override string? PrimaryTag => primaryTag;

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken) => api.ToggleMarkAsync(primaryTag, cancellationToken);
}
