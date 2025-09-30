using Zauber.RTE.Services;
using Zauber.RTE.Components.Panels;

namespace Zauber.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for justified text alignment
/// </summary>
public class JustifiedItem : ToolbarItemBase
{
    public override string Id => "justified";
    public override string Label => "Justified";
    public override string IconCss => "fa-align-justify";
    public override ToolbarPlacement Placement => ToolbarPlacement.Block;

    public override bool IsActive(EditorState state) => state.CurrentAlignment == TextAlignment.Justified;
    public override Task ExecuteAsync(EditorApi api) => api.SetBlockStyleAsync(new() { ["text-align"] = "justify" });
}
