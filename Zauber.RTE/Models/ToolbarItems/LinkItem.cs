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

    public override Task ExecuteAsync(EditorApi api) => api.OpenPanelAsync(typeof(LinkPanel));
}
