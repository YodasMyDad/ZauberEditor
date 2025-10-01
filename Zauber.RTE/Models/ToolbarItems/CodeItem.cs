using Zauber.RTE.Services;
using Zauber.RTE.Components.Panels;

namespace Zauber.RTE.Models.ToolbarItems;

/// <summary>
/// Smart toolbar item for code formatting that adapts based on selection context.
/// Applies inline code for text selections, code block for full block selections.
/// </summary>
public class CodeItem : ToolbarItemBase
{
    public override string Id => "code";
    public override string Label => "Code";
    public override string Tooltip => "Code (inline or block)";
    public override string IconCss => "fa-code";
    public override ToolbarPlacement Placement => ToolbarPlacement.Inline;
    public override bool IsToggle => true;

    public override bool IsActive(EditorState state) => 
        state.ActiveMarks.Contains("code") || 
        state.CurrentBlockType == "pre" || 
        state.CurrentBlockType == "codeblock";

    public override async Task ExecuteAsync(EditorApi api)
    {
        // Get fresh state from DOM
        var currentBlockType = await api.GetCurrentBlockTypeAsync();
        var state = api.GetState();

        // If currently in a code block, toggle it off to paragraph
        if (currentBlockType == "pre" || currentBlockType == "codeblock")
        {
            await api.SetBlockTypeAsync("p", null);
        }
        // If currently has inline code mark, toggle it off
        else if (state.ActiveMarks.Contains("code"))
        {
            await api.ToggleMarkAsync("code");
        }
        // If no selection (just cursor), convert block to code block
        else if (!state.HasSelection)
        {
            await api.SetBlockTypeAsync("codeblock", null);
        }
        // Has selection - apply inline code
        else
        {
            await api.ToggleMarkAsync("code");
        }
    }
}
