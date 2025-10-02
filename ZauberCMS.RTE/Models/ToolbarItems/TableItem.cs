namespace ZauberCMS.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for inserting tables
/// </summary>
public class TableItem : ToolbarItemBase
{
    public override string Id => "table";
    public override string Label => "Table";
    public override string IconCss => "fa-table";
    public override ToolbarPlacement Placement => ToolbarPlacement.Insert;

    public override Task ExecuteAsync(IEditorApi api) => api.OpenPanelAsync(typeof(Components.Panels.TablePanel));
}
