namespace ZauberCMS.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for heading 3 block type
/// </summary>
public class Heading3Item : ToolbarItemBase
{
    public override string Id => "h3";
    public override string Label => "Heading 3";
    public override string IconCss => "fa-heading";
    public override ToolbarPlacement Placement => ToolbarPlacement.Block;
    public override bool IsToggle => true;

    public override bool IsActive(EditorState state) => state is { CurrentBlockType: "heading", CurrentHeadingLevel: 3 };
    public override async Task ExecuteAsync(IEditorApi api)
    {
        // Get current block type directly to check if we're toggling off
        var currentBlockType = await api.GetCurrentBlockTypeAsync();
        var currentHeadingLevel = await api.GetCurrentHeadingLevelAsync();
        
        // Toggle: if already H3, convert to paragraph
        if (currentBlockType == "heading" && currentHeadingLevel == 3)
        {
            await api.SetBlockTypeAsync("p", null);
        }
        else
        {
            await api.SetBlockTypeAsync("h3", null);
        }
    }
}
