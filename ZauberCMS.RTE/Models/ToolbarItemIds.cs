namespace ZauberCMS.RTE.Models;

/// <summary>Ids of the built-in toolbar items.</summary>
public static class ToolbarItemIds
{
    /// <summary>Bold.</summary>
    public const string Bold = "bold";

    /// <summary>Italic.</summary>
    public const string Italic = "italic";

    /// <summary>Underline.</summary>
    public const string Underline = "underline";

    /// <summary>Strikethrough.</summary>
    public const string Strike = "strike";

    /// <summary>Subscript.</summary>
    public const string Subscript = "subscript";

    /// <summary>Superscript.</summary>
    public const string Superscript = "superscript";

    /// <summary>Inline code, or a code block when nothing is selected.</summary>
    public const string Code = "code";

    /// <summary>Paragraph, listed in the headings dropdown.</summary>
    public const string Paragraph = "paragraph";

    /// <summary>Heading level 1.</summary>
    public const string Heading1 = "h1";

    /// <summary>Heading level 2.</summary>
    public const string Heading2 = "h2";

    /// <summary>Heading level 3.</summary>
    public const string Heading3 = "h3";

    /// <summary>Heading level 4.</summary>
    public const string Heading4 = "h4";

    /// <summary>Heading level 5.</summary>
    public const string Heading5 = "h5";

    /// <summary>Heading level 6.</summary>
    public const string Heading6 = "h6";

    /// <summary>Code block, listed in the headings dropdown.</summary>
    public const string CodeBlock = "codeBlock";

    /// <summary>Dropdown of the paragraph, heading levels and code block.</summary>
    public const string Headings = "headings";

    /// <summary>Block quote.</summary>
    public const string Blockquote = "blockquote";

    /// <summary>Bulleted list.</summary>
    public const string UnorderedList = "ul";

    /// <summary>Numbered list.</summary>
    public const string OrderedList = "ol";

    /// <summary>Indent the block, or nest the list item under the one before it.</summary>
    public const string Indent = "indent";

    /// <summary>Outdent the block, or lift the list item out of its list.</summary>
    public const string Outdent = "outdent";

    /// <summary>Align left.</summary>
    public const string AlignLeft = "alignLeft";

    /// <summary>Align centre.</summary>
    public const string AlignCenter = "alignCenter";

    /// <summary>Align right.</summary>
    public const string AlignRight = "alignRight";

    /// <summary>Justify.</summary>
    public const string Justify = "justified";

    /// <summary>Insert or edit a link.</summary>
    public const string Link = "link";

    /// <summary>Remove a link.</summary>
    public const string Unlink = "unlink";

    /// <summary>Insert or edit an image.</summary>
    public const string Image = "image";

    /// <summary>Insert a table.</summary>
    public const string Table = "table";

    /// <summary>Insert a horizontal line.</summary>
    public const string HorizontalRule = "horizontalRule";

    /// <summary>Insert a special character.</summary>
    public const string SpecialCharacters = "specialCharacters";

    /// <summary>Dropdown of text colours; its swatches have ids such as <c>textColor-red</c>.</summary>
    public const string TextColor = "textColor";

    /// <summary>Dropdown of highlight colours; its swatches have ids such as <c>backgroundColor-yellow</c>.</summary>
    public const string BackgroundColor = "backgroundColor";

    /// <summary>Remove formatting from the selection.</summary>
    public const string ClearFormatting = "clean-html";

    /// <summary>Undo.</summary>
    public const string Undo = "undo";

    /// <summary>Redo.</summary>
    public const string Redo = "redo";

    /// <summary>Find and replace text.</summary>
    public const string FindReplace = "findReplace";

    /// <summary>Toggle pasting as plain text.</summary>
    public const string PastePlainText = "pastePlainText";

    /// <summary>Toggle the HTML source view.</summary>
    public const string ViewSource = "viewSource";

    /// <summary>Toggle full screen.</summary>
    public const string Fullscreen = "fullscreen";

    /// <summary>Switch between light and dark themes.</summary>
    public const string ThemeToggle = "themeToggle";
}
