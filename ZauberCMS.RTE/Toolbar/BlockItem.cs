using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Toolbar;

internal sealed class BlockItem(
    string id,
    string label,
    string? iconSvg = null,
    string? tooltip = null,
    string? shortcut = null,
    string? tag = null) : ToolbarItemBase
{
    private readonly string _tag = tag ?? id;

    public override string Id => id;

    public override string Label => label;

    public override string? Tooltip => tooltip;

    public override string? IconSvg => iconSvg;

    public override string? Shortcut => shortcut;

    public override bool IsToggle => true;

    public override bool IsActive(EditorState state) => state.BlockType == _tag;

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken) =>
        api.SetBlockTypeAsync(IsActive(api.State) ? "p" : _tag, cancellationToken: cancellationToken);
}
