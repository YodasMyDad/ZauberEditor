using Zauber.RTE.Services;
using Zauber.RTE.Components.Panels;

namespace Zauber.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for inserting tables
/// </summary>
public class TableItem : ToolbarItemBase
{
    public override string Id => "table";
    public override string Label => "Table";
    public override string IconClass => "fa-table";
    public override ToolbarPlacement Placement => ToolbarPlacement.Insert;

    public override Task ExecuteAsync(EditorApi api) => api.OpenPanelAsync(typeof(Zauber.RTE.Components.Panels.TablePanel));
}
