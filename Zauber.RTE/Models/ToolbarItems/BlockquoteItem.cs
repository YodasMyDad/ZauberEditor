using Zauber.RTE.Services;
using Zauber.RTE.Components.Panels;

namespace Zauber.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for blockquote
/// </summary>
public class BlockquoteItem : ToolbarItemBase
{
    public override string Id => "blockquote";
    public override string Label => "Quote";
    public override string IconCss => "fa-quote-left";
    public override ToolbarPlacement Placement => ToolbarPlacement.Block;
    public override bool IsToggle => true;

    public override bool IsActive(EditorState state) => state.CurrentBlockType == "blockquote";
    public override Task ExecuteAsync(EditorApi api) => api.SetBlockTypeAsync("blockquote");
}
