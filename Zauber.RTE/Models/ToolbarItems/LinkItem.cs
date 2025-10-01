using Zauber.RTE.Components.Panels;

namespace Zauber.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for inserting links
/// </summary>
public class LinkItem : ToolbarItemBase
{
    public override string Id => "link";
    public override string Label => "Link";
    public override string Tooltip => "Insert Link (Ctrl+K)";
    public override string IconCss => "fa-link";
    public override string Shortcut => "Control+k";
    public override ToolbarPlacement Placement => ToolbarPlacement.Insert;

    public override async Task ExecuteAsync(EditorApi api)
    {
        // Only open panel if there's selected text or cursor is on a link
        var selection = await api.GetSelectionAsync();
        var existingLink = await api.GetLinkAtCursorAsync();
        
        if ((selection != null && !string.IsNullOrWhiteSpace(selection.SelectedText)) || existingLink != null)
        {
            await api.OpenPanelAsync(typeof(LinkPanel));
        }
    }
}
