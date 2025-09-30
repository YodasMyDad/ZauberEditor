using Zauber.RTE.Services;
using Zauber.RTE.Components.Panels;

namespace Zauber.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for code block
/// </summary>
public class CodeBlockItem : ToolbarItemBase
{
    public override string Id => "codeBlock";
    public override string Label => "Code Block";
    public override string IconClass => "fa-square-code";
    public override ToolbarPlacement Placement => ToolbarPlacement.Block;
    public override bool IsToggle => true;

    public override bool IsActive(EditorState state) => state.CurrentBlockType == "codeblock";
    public override Task ExecuteAsync(EditorApi api) => api.SetBlockTypeAsync("codeblock");
}
