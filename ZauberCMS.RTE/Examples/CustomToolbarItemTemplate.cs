using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Examples;

/// <summary>
/// Example template for creating a custom toolbar item.
/// Copy this file to your project and customize as needed.
/// </summary>
public class CustomToolbarItemTemplate : ToolbarItemBase
{
    /// <summary>
    /// Unique identifier for this toolbar item (used in toolbar layout configuration)
    /// </summary>
    public override string Id => "my-custom-item";

    /// <summary>
    /// Display label for accessibility and tooltips
    /// </summary>
    public override string Label => "My Custom Item";

    /// <summary>
    /// Optional tooltip text (shows on hover)
    /// </summary>
    public override string? Tooltip => "Click to do something custom";

    /// <summary>
    /// FontAwesome icon CSS class (e.g., "fa-star", "fa-check", "fa-magic")
    /// See: https://fontawesome.com/icons for available icons
    /// </summary>
    public override string IconCss => "fa-star";

    /// <summary>
    /// Where this item appears in the toolbar
    /// Options: Inline, Block, Insert, Media, Custom
    /// </summary>
    public override ToolbarPlacement Placement => ToolbarPlacement.Insert;

    /// <summary>
    /// Set to true if this is a toggle button (like Bold or Italic)
    /// Default: false
    /// </summary>
    public override bool IsToggle => false;

    /// <summary>
    /// Determines if this item should be enabled given the current editor state
    /// Example: Only enable when text is selected
    /// Default: Always enabled
    /// </summary>
    public override bool IsEnabled(EditorState state)
    {
        // Example: Only enable when text is selected
        // return state.HasSelection;
        
        return true; // Always enabled
    }

    /// <summary>
    /// Determines if this toggle item is currently active
    /// Only relevant if IsToggle is true
    /// Default: Never active
    /// </summary>
    public override bool IsActive(EditorState state)
    {
        // Example: Check if a specific mark/format is active
        // return state.ActiveMarks.Contains("mark");
        
        return false;
    }

    /// <summary>
    /// Executes the action for this toolbar item
    /// This is called when the user clicks the button
    /// </summary>
    public override async Task ExecuteAsync(IEditorApi api)
    {
        // Option 1: Insert HTML directly
        await api.InsertHtmlAsync("<span>Custom content</span>");

        // Option 2: Use HtmlBuilder for cleaner code
        // var html = HtmlBuilder.Element("div")
        //     .Class("custom-class")
        //     .Text("Hello from custom item!")
        //     .Build();
        // await api.InsertHtmlAsync(html);

        // Option 3: Toggle formatting
        // await api.ToggleMarkAsync("mark");

        // Option 4: Change block type
        // await api.SetBlockTypeAsync("h2", null);

        // Option 5: Open a custom panel for user input
        // await api.OpenPanelAsync(typeof(MyCustomPanel));

        // Option 6: Work with selection
        // var selection = await api.GetSelectionAsync();
        // if (selection?.SelectedText != null)
        // {
        //     var html = HtmlBuilder.Element("span")
        //         .Class("highlight")
        //         .Text(selection.SelectedText)
        //         .Build();
        //     await api.ReplaceSelectionAsync(html);
        // }

        // Option 7: Show a notification
        // await api.ShowToastAsync("Action completed!", ToastType.Success);
    }
}

