using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Toolbar;

internal sealed class CodeItem : ToolbarItemBase
{
    private const string Tag = "code";
    private const string BlockTag = "pre";

    public override string Id => ToolbarItemIds.Code;

    public override string Label => "Code";

    public override string? Tooltip => "Code (inline, or a block when nothing is selected)";

    public override string? IconSvg => Icons.Code;

    public override bool IsToggle => true;

    public override IReadOnlyList<string> TrackedTags => [Tag];

    public override string? PrimaryTag => Tag;

    public override bool IsActive(EditorState state) => state.BlockType == BlockTag || base.IsActive(state);

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken) => api.State switch
    {
        { BlockType: BlockTag } => api.SetBlockTypeAsync("p", cancellationToken: cancellationToken),
        { HasSelection: true } => api.ToggleMarkAsync(Tag, cancellationToken),
        var state when state.ActiveMarks.Contains(Tag) => api.ToggleMarkAsync(Tag, cancellationToken),
        _ => api.SetBlockTypeAsync(BlockTag, cancellationToken: cancellationToken)
    };
}
