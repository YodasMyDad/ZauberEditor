namespace ZauberCMS.RTE.Models.ToolbarItems;

/// <summary>
/// Label item for headings dropdown that resets to paragraph
/// </summary>
public class HeadingsLabelItem : ToolbarItemBase
{
    public override string Id => "headings-label";
    public override string Label => "Normal Text";
    public override string? Tooltip => "Reset to paragraph";
    public override string IconCss => "fa-paragraph";
    public override ToolbarPlacement Placement => ToolbarPlacement.Block;
    public override bool IsDropdownLabel => true;

    // Active when block is a paragraph (not a heading)
    public override bool IsActive(EditorState state) => 
        state.CurrentBlockType == "paragraph" || 
        (state.CurrentBlockType != "heading" && state.CurrentHeadingLevel == 0);
    
    public override async Task ExecuteAsync(IEditorApi api)
    {
        // Reset to paragraph
        await api.SetBlockTypeAsync("p", null);
    }
}

