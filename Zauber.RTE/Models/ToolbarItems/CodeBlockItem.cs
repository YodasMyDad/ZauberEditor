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
    public override string IconCss => "fa-code";
    public override ToolbarPlacement Placement => ToolbarPlacement.Block;
    public override bool IsToggle => true;

    public override bool IsActive(EditorState state) => state.CurrentBlockType == "pre" || state.CurrentBlockType == "codeblock";
    public override async Task ExecuteAsync(EditorApi api)
    {
        // Toggle: if already codeblock, convert to paragraph
        if (IsActive(api.GetState()))
        {
            await api.SetBlockTypeAsync("p", null);
        }
        else
        {
            await api.SetBlockTypeAsync("codeblock", null);
        }
    }
}
