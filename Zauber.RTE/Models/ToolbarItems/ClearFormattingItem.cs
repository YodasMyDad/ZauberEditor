using Zauber.RTE.Services;
using Zauber.RTE.Components.Panels;

namespace Zauber.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for clearing formatting
/// </summary>
public class ClearFormattingItem : ToolbarItemBase
{
    public override string Id => "clear";
    public override string Label => "Clear Formatting";
    public override string IconCss => "fa-eraser";
    public override ToolbarPlacement Placement => ToolbarPlacement.Inline;

    public override Task ExecuteAsync(EditorApi api) => api.ClearFormattingAsync();
}
