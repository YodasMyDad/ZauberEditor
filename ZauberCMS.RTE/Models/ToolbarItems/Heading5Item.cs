namespace ZauberCMS.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for heading 5 block type
/// </summary>
public class Heading5Item : ToolbarItemBase
{
    public override string Id => "h5";
    public override string Label => "H5";
    public override string? Tooltip => "Heading 5";
    public override string IconCss => ""; // Empty = show label as text in button
    public override ToolbarPlacement Placement => ToolbarPlacement.Block;
    public override bool IsToggle => true;

    public override bool IsActive(EditorState state) => state is { CurrentBlockType: "heading", CurrentHeadingLevel: 5 };
    
    public override async Task ExecuteAsync(IEditorApi api)
    {
        var currentBlockType = await api.GetCurrentBlockTypeAsync();
        var currentHeadingLevel = await api.GetCurrentHeadingLevelAsync();
        
        if (currentBlockType == "heading" && currentHeadingLevel == 5)
        {
            await api.SetBlockTypeAsync("p", null);
        }
        else
        {
            await api.SetBlockTypeAsync("h5", null);
        }
    }
}

