using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Toolbar;

internal static class BuiltInToolbarItems
{
    public static IReadOnlyList<IToolbarItem> All { get; } =
    [
        new MarkItem(ToolbarItemIds.Bold, "Bold", Icons.Bold, "strong", ["strong", "b"], "Control+b"),
        new MarkItem(ToolbarItemIds.Italic, "Italic", Icons.Italic, "em", ["em", "i"], "Control+i"),
        new MarkItem(ToolbarItemIds.Underline, "Underline", Icons.Underline, "u", ["u"], "Control+u"),
        new MarkItem(ToolbarItemIds.Strike, "Strikethrough", Icons.Strikethrough, "s", ["s", "strike"]),
        new MarkItem(ToolbarItemIds.Subscript, "Subscript", Icons.Subscript, "sub", ["sub"]),
        new MarkItem(ToolbarItemIds.Superscript, "Superscript", Icons.Superscript, "sup", ["sup"]),
        new CodeItem(),
        new ColorDropdownItem(ToolbarItemIds.TextColor, "Text colour", Icons.TextColor, ColorPalettes.Text),
        .. ColorPalettes.Text,
        new ColorDropdownItem(ToolbarItemIds.BackgroundColor, "Highlight colour", Icons.Highlighter, ColorPalettes.Highlight),
        .. ColorPalettes.Highlight,
        new BlockItem(ToolbarItemIds.Paragraph, "Paragraph", tag: "p"),
        new BlockItem(ToolbarItemIds.Heading1, "H1", tooltip: "Heading 1"),
        new BlockItem(ToolbarItemIds.Heading2, "H2", tooltip: "Heading 2"),
        new BlockItem(ToolbarItemIds.Heading3, "H3", tooltip: "Heading 3"),
        new BlockItem(ToolbarItemIds.Heading4, "H4", tooltip: "Heading 4"),
        new BlockItem(ToolbarItemIds.Heading5, "H5", tooltip: "Heading 5"),
        new BlockItem(ToolbarItemIds.Heading6, "H6", tooltip: "Heading 6"),
        new BlockItem(ToolbarItemIds.CodeBlock, "Code block", tag: "pre"),
        new HeadingsDropdownItem(),
        new BlockItem(ToolbarItemIds.Blockquote, "Quote", Icons.Quote),
        new BlockItem(ToolbarItemIds.UnorderedList, "Bulleted list", Icons.BulletList, shortcut: "Control+Shift+8"),
        new BlockItem(ToolbarItemIds.OrderedList, "Numbered list", Icons.NumberedList, shortcut: "Control+Shift+7"),
        new IndentItem(outdent: false),
        new IndentItem(outdent: true),
        new AlignItem(ToolbarItemIds.AlignLeft, "Align left", Icons.AlignLeft, TextAlignment.Left, "left"),
        new AlignItem(ToolbarItemIds.AlignCenter, "Align centre", Icons.AlignCenter, TextAlignment.Center, "center"),
        new AlignItem(ToolbarItemIds.AlignRight, "Align right", Icons.AlignRight, TextAlignment.Right, "right"),
        new AlignItem(ToolbarItemIds.Justify, "Justify", Icons.AlignJustify, TextAlignment.Justify, "justify"),
        new LinkItem(),
        new UnlinkItem(),
        new ImageItem(),
        new TableItem(),
        new HorizontalRuleItem(),
        new SpecialCharactersItem(),
        new ClearFormattingItem(),
        new UndoItem(),
        new RedoItem(),
        new FindReplaceItem(),
        new PastePlainTextItem(),
        new ViewSourceItem(),
        new FullscreenItem(),
        new ThemeToggleItem()
    ];
}
