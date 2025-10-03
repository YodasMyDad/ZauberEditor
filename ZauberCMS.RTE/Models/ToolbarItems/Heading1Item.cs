namespace ZauberCMS.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for heading 1 block type
/// </summary>
public class Heading1Item : ToolbarItemBase
{
    public override string Id => "h1";
    public override string Label => "H1";
    public override string? Tooltip => "Heading 1";
    public override string IconCss => ""; // Empty = show label as text in button
    public override ToolbarPlacement Placement => ToolbarPlacement.Block;
    public override bool IsToggle => true;

    public override bool IsActive(EditorState state) => state is { CurrentBlockType: "heading", CurrentHeadingLevel: 1 };
    public override async Task ExecuteAsync(IEditorApi api)
    {
        // Get current block type directly to check if we're toggling off
        var currentBlockType = await api.GetCurrentBlockTypeAsync();
        var currentHeadingLevel = await api.GetCurrentHeadingLevelAsync();
        
        // Toggle: if already H1, convert to paragraph
        if (currentBlockType == "heading" && currentHeadingLevel == 1)
        {
            await api.SetBlockTypeAsync("p", null);
        }
        else
        {
            await api.SetBlockTypeAsync("h1", null);
        }
    }
}
