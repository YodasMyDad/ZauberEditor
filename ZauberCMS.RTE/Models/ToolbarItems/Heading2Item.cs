namespace ZauberCMS.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for heading 2 block type
/// </summary>
public class Heading2Item : ToolbarItemBase
{
    public override string Id => "h2";
    public override string Label => "Heading 2";
    public override string IconCss => "fa-heading";
    public override ToolbarPlacement Placement => ToolbarPlacement.Block;
    public override bool IsToggle => true;

    public override bool IsActive(EditorState state) => state is { CurrentBlockType: "heading", CurrentHeadingLevel: 2 };
    public override async Task ExecuteAsync(IEditorApi api)
    {
        // Get current block type directly to check if we're toggling off
        var currentBlockType = await api.GetCurrentBlockTypeAsync();
        var currentHeadingLevel = await api.GetCurrentHeadingLevelAsync();
        
        // Toggle: if already H2, convert to paragraph
        if (currentBlockType == "heading" && currentHeadingLevel == 2)
        {
            await api.SetBlockTypeAsync("p", null);
        }
        else
        {
            await api.SetBlockTypeAsync("h2", null);
        }
    }
}
