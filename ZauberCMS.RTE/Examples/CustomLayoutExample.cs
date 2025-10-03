using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Examples;

/// <summary>
/// Examples demonstrating the new flexible toolbar layout system
/// </summary>
public static class CustomLayoutExample
{
    /// <summary>
    /// Example 1: Simple layout with blocks and separators
    /// </summary>
    public static ToolbarLayout SimpleCustomLayout => ToolbarLayout.FromItems(
        new ToolbarBlock("bold", "italic", "underline"),
        new ToolbarSeparator(),
        new ToolbarBlock("headings"), // Headings dropdown
        new ToolbarSeparator(),
        new ToolbarBlock("link", "image")
    );

    /// <summary>
    /// Example 2: Advanced layout with custom CSS classes
    /// </summary>
    public static ToolbarLayout AdvancedCustomLayout => ToolbarLayout.FromItems(
        new ToolbarBlock("my-custom-class", "undo", "redo"),
        new ToolbarSeparator("my-separator-style"),
        new ToolbarBlock("headings", "bold", "italic"),
        new ToolbarSeparator(),
        new ToolbarItemReference("link", "my-link-style"),
        new ToolbarItemReference("image")
    );

    /// <summary>
    /// Example 3: Minimal layout for comments
    /// </summary>
    public static ToolbarLayout CommentLayout => ToolbarLayout.FromItems(
        new ToolbarBlock("bold", "italic", "link")
    );

    /// <summary>
    /// Example 4: Blog editor layout with all heading levels
    /// </summary>
    public static ToolbarLayout BlogEditorLayout => ToolbarLayout.FromItems(
        new ToolbarBlock("undo", "redo", "viewSource"),
        new ToolbarSeparator(),
        new ToolbarBlock("headings", "blockquote"),
        new ToolbarSeparator(),
        new ToolbarBlock("bold", "italic", "underline", "strike", "code"),
        new ToolbarSeparator(),
        new ToolbarBlock("ul", "ol"),
        new ToolbarSeparator(),
        new ToolbarBlock("link", "unlink", "image"),
        new ToolbarSeparator(),
        new ToolbarBlock("alignLeft", "alignCenter", "alignRight")
    );

    /// <summary>
    /// Example 5: Using individual heading buttons instead of dropdown
    /// </summary>
    public static ToolbarLayout IndividualHeadingsLayout => ToolbarLayout.FromItems(
        new ToolbarBlock("bold", "italic"),
        new ToolbarSeparator(),
        new ToolbarBlock("h1", "h2", "h3"), // Individual heading buttons
        new ToolbarSeparator(),
        new ToolbarBlock("link", "image")
    );

    /// <summary>
    /// Example 6: Mixed dropdowns and individual items
    /// </summary>
    public static ToolbarLayout MixedLayout => ToolbarLayout.FromItems(
        new ToolbarBlock("headings", "bold", "italic"), // Dropdown + buttons
        new ToolbarSeparator(),
        new ToolbarBlock("h1"), // Individual H1 button
        new ToolbarSeparator(),
        new ToolbarBlock("link")
    );

    /// <summary>
    /// Example 7: Using ToolbarLayoutExtensions helper methods
    /// </summary>
    public static ToolbarLayout UsingExtensionsLayout()
    {
        return ToolbarLayout.FromItems(
            ToolbarLayoutExtensions.Block("bold", "italic", "underline"),
            ToolbarLayoutExtensions.Separator(),
            ToolbarLayoutExtensions.Block("headings"),
            ToolbarLayoutExtensions.Separator(),
            ToolbarLayoutExtensions.Item("link"),
            ToolbarLayoutExtensions.Item("image")
        );
    }

    /// <summary>
    /// Example 8: Creating a layout with custom toolbar items
    /// Assumes you've created custom toolbar items like "emoji", "mention", "hashtag"
    /// </summary>
    public static ToolbarLayout SocialMediaLayout => ToolbarLayout.FromItems(
        new ToolbarBlock("bold", "italic"),
        new ToolbarSeparator(),
        new ToolbarBlock("emoji", "mention", "hashtag"), // Custom items
        new ToolbarSeparator(),
        new ToolbarBlock("link", "image")
    );
}

/// <summary>
/// Example of how to use custom layouts in your Blazor component
/// </summary>
public class UsageExample
{
    /*
     * In your Blazor component (.razor file):
     * 
     * @page "/editor"
     * @using ZauberCMS.RTE.Models
     * @using ZauberCMS.RTE.Examples
     * 
     * <ZauberRichTextEditor 
     *     @bind-Value="_content"
     *     Settings="@_editorSettings" />
     * 
     * @code {
     *     private string? _content;
     *     private EditorSettings _editorSettings = new()
     *     {
     *         ToolbarLayout = CustomLayoutExample.BlogEditorLayout
     *     };
     * }
     * 
     * Or dynamically create a layout:
     * 
     * @code {
     *     private EditorSettings _editorSettings = new()
     *     {
     *         ToolbarLayout = ToolbarLayout.FromItems(
     *             new ToolbarBlock("bold", "italic"),
     *             new ToolbarSeparator(),
     *             new ToolbarBlock("headings", "link")
     *         )
     *     };
     * }
     */
}

