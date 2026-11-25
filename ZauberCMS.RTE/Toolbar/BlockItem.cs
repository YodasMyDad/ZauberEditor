using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Toolbar;

internal sealed class BlockItem(
    string tag,
    string label,
    string? iconSvg = null,
    string? tooltip = null,
    string? shortcut = null) : ToolbarItemBase
{
    public override string Id => tag;

    public override string Label => label;

    public override string? Tooltip => tooltip;

    public override string? IconSvg => iconSvg;

    public override string? Shortcut => shortcut;

    public override bool IsToggle => true;

    public override bool IsActive(EditorState state) => state.BlockType == tag;

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken) =>
        api.SetBlockTypeAsync(IsActive(api.State) ? "p" : tag, cancellationToken: cancellationToken);
}
