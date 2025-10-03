namespace ZauberCMS.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for heading 6 block type
/// </summary>
public class Heading6Item : ToolbarItemBase
{
    public override string Id => "h6";
    public override string Label => "H6";
    public override string? Tooltip => "Heading 6";
    public override string IconCss => ""; // Empty = show label as text in button
    public override ToolbarPlacement Placement => ToolbarPlacement.Block;
    public override bool IsToggle => true;

    public override bool IsActive(EditorState state) => state is { CurrentBlockType: "heading", CurrentHeadingLevel: 6 };
    
    public override async Task ExecuteAsync(IEditorApi api)
    {
        var currentBlockType = await api.GetCurrentBlockTypeAsync();
        var currentHeadingLevel = await api.GetCurrentHeadingLevelAsync();
        
        if (currentBlockType == "heading" && currentHeadingLevel == 6)
        {
            await api.SetBlockTypeAsync("p", null);
        }
        else
        {
            await api.SetBlockTypeAsync("h6", null);
        }
    }
}

