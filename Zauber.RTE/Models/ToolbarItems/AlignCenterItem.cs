using Zauber.RTE.Services;
using Zauber.RTE.Components.Panels;

namespace Zauber.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for center text alignment
/// </summary>
public class AlignCenterItem : ToolbarItemBase
{
    public override string Id => "alignCenter";
    public override string Label => "Align Center";
    public override string IconClass => "fa-align-center";
    public override ToolbarPlacement Placement => ToolbarPlacement.Block;

    public override bool IsActive(EditorState state) => state.CurrentAlignment == TextAlignment.Center;
    public override Task ExecuteAsync(EditorApi api) => api.SetBlockStyleAsync(new() { ["text-align"] = "center" });
}

