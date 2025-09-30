using Zauber.RTE.Services;
using Zauber.RTE.Components.Panels;

namespace Zauber.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for paragraph block type
/// </summary>
public class ParagraphItem : ToolbarItemBase
{
    public override string Id => "paragraph";
    public override string Label => "Paragraph";
    public override string IconCss => "fa-paragraph";
    public override ToolbarPlacement Placement => ToolbarPlacement.Block;
    public override bool IsToggle => true;

    public override bool IsActive(EditorState state) => state.CurrentBlockType == "paragraph";
    public override Task ExecuteAsync(EditorApi api) => api.SetBlockTypeAsync("paragraph");
}
