using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Toolbar;

internal sealed class HeadingsDropdownItem : ToolbarItemBase
{
    public override string Id => ToolbarItemIds.Headings;

    public override string Label => "Headings";

    public override string? Tooltip => "Paragraph, headings and code block";

    public override string? IconSvg => Icons.Heading;

    public override bool IsToggle => true;

    public override ToolbarItemKind Kind => ToolbarItemKind.Dropdown;

    public override IReadOnlyList<string> ChildItemIds { get; } =
    [
        ToolbarItemIds.Paragraph,
        ToolbarItemIds.Heading1, ToolbarItemIds.Heading2, ToolbarItemIds.Heading3,
        ToolbarItemIds.Heading4, ToolbarItemIds.Heading5, ToolbarItemIds.Heading6,
        ToolbarItemIds.CodeBlock
    ];

    public override bool IsActive(EditorState state) => state.HeadingLevel > 0 || state.BlockType == "pre";

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken) => Task.CompletedTask;
}
