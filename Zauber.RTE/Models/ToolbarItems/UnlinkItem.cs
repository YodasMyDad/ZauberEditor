namespace Zauber.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for removing links from selected text
/// </summary>
public class UnlinkItem : ToolbarItemBase
{
    public override string Id => "unlink";
    public override string Label => "Unlink";
    public override string Tooltip => "Remove Link";
    public override string IconCss => "fa-link-slash";
    public override ToolbarPlacement Placement => ToolbarPlacement.Insert;

    public override bool IsEnabled(EditorState state)
    {
        // Only enabled when not in source view
        return !state.IsSourceView;
    }

    public override async Task ExecuteAsync(EditorApi api)
    {
        // Check if we're in a link
        var linkInfo = await api.GetLinkAtCursorAsync();
        if (linkInfo != null)
        {
            // Select the entire link first (in case cursor is just inside it with no selection)
            await api.SelectLinkAtCursorAsync();
            
            // Unwrap the link
            await api.UnwrapSelectionAsync("a");
        }
    }
}

