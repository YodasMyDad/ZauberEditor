namespace ZauberCMS.RTE.Models.ToolbarItems;

/// <summary>
/// Dropdown toolbar item containing all heading options (H1-H6)
/// </summary>
public class HeadingsDropdownItem : ToolbarItemBase
{
    private static readonly List<IToolbarItem> _childItems =
    [
        new Heading1Item(),
        new Heading2Item(),
        new Heading3Item(),
        new Heading4Item(),
        new Heading5Item(),
        new Heading6Item()
    ];

    public override string Id => "headings";
    public override string Label => "Headings";
    public override string IconCss => "fa-heading";
    public override ToolbarPlacement Placement => ToolbarPlacement.Block;
    public override ToolbarItemType ItemType => ToolbarItemType.Dropdown;
    public override bool IsToggle => true;

    public override List<IToolbarItem> ChildItems => _childItems;

    public override bool IsActive(EditorState state) => 
        state.CurrentBlockType == "heading" && state.CurrentHeadingLevel > 0;

    public override Task ExecuteAsync(IEditorApi api)
    {
        // Dropdown itself doesn't execute - child items do
        return Task.CompletedTask;
    }
}

