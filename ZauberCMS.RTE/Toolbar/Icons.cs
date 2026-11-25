namespace ZauberCMS.RTE.Toolbar;

internal static class Icons
{
    private const string Open = """<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false">""";
    private const string Close = "</svg>";
    private const string Frame = """<rect x="3" y="4" width="18" height="16" rx="2"/>""";

    public const string Bold = Open + """<path d="M7 5h6a3.5 3.5 0 0 1 0 7H7zM7 12h7a3.5 3.5 0 0 1 0 7H7z"/>""" + Close;
    public const string Italic = Open + """<path d="M10 5h8M6 19h8M14 5l-4 14"/>""" + Close;
    public const string Underline = Open + """<path d="M7 4v7a5 5 0 0 0 10 0V4M5 20h14"/>""" + Close;
    public const string Strikethrough = Open + """<path d="M5 12h14M16.5 7.5C16 5.8 14.3 5 12 5c-2.8 0-4.5 1.3-4.5 3.3 0 1.4.9 2.3 2.5 2.9M8 16.5c.6 1.6 2.2 2.5 4.3 2.5 2.8 0 4.7-1.3 4.7-3.4 0-.9-.3-1.6-1-2.1"/>""" + Close;
    public const string Code = Open + """<path d="M9 7l-5 5 5 5M15 7l5 5-5 5"/>""" + Close;
    public const string Subscript = Open + """<path d="M4 6l8 9M12 6l-8 9M16 15.5a2 2 0 0 1 4 0c0 1.5-4 2.5-4 4.5h4"/>""" + Close;
    public const string Superscript = Open + """<path d="M4 9l8 9M12 9l-8 9M16 4.5a2 2 0 0 1 4 0c0 1.5-4 2.5-4 4.5h4"/>""" + Close;
    public const string Heading = Open + """<path d="M6 4v16M18 4v16M6 12h12"/>""" + Close;
    public const string Quote = Open + """<path d="M4 17v-5c0-3 1.5-5 4.5-6M4 12h4.5v5H4M13 17v-5c0-3 1.5-5 4.5-6M13 12h4.5v5H13"/>""" + Close;
    public const string BulletList = Open + """<path d="M9 6h11M9 12h11M9 18h11M4.5 6h.01M4.5 12h.01M4.5 18h.01"/>""" + Close;
    public const string NumberedList = Open + """<path d="M10 6h10M10 12h10M10 18h10M4 5l1.5-1v5M3.5 14.5a1.5 1.5 0 0 1 3 0c0 1-3 2-3 3.5h3"/>""" + Close;
    public const string Link = Open + """<path d="M10 14a4 4 0 0 0 5.66 0l3-3a4 4 0 0 0-5.66-5.66l-1 1M14 10a4 4 0 0 0-5.66 0l-3 3a4 4 0 0 0 5.66 5.66l1-1"/>""" + Close;
    public const string Unlink = Open + """<path d="M15.5 13.5l3-3a4 4 0 0 0-5.66-5.66l-1.5 1.5M8.5 10.5l-3 3a4 4 0 0 0 5.66 5.66l1.5-1.5M4 4l16 16"/>""" + Close;
    public const string Image = Open + Frame + """<circle cx="8.5" cy="9.5" r="1.5"/><path d="M21 16l-5-5-9 9"/>""" + Close;
    public const string Table = Open + Frame + """<path d="M3 10h18M3 15h18M9 10v10M15 10v10"/>""" + Close;
    public const string AlignLeft = Open + """<path d="M4 6h16M4 10h10M4 14h16M4 18h10"/>""" + Close;
    public const string AlignCenter = Open + """<path d="M4 6h16M7 10h10M4 14h16M7 18h10"/>""" + Close;
    public const string AlignRight = Open + """<path d="M4 6h16M10 10h10M4 14h16M10 18h10"/>""" + Close;
    public const string AlignJustify = Open + """<path d="M4 6h16M4 10h16M4 14h16M4 18h16"/>""" + Close;
    public const string Eraser = Open + """<path d="M7 20h13M14.5 4.5l5 5L11 18H7l-3-3zM9.5 9.5l5 5"/>""" + Close;
    public const string Undo = Open + """<path d="M9 14L4 9l5-5M4 9h10a6 6 0 0 1 0 12h-3"/>""" + Close;
    public const string Redo = Open + """<path d="M15 14l5-5-5-5M20 9H10a6 6 0 0 0 0 12h3"/>""" + Close;
    public const string Source = Open + """<path d="M8 8l-4 4 4 4M16 8l4 4-4 4M13.5 5l-3 14"/>""" + Close;
    public const string Contrast = Open + """<circle cx="12" cy="12" r="9"/><path d="M12 3a9 9 0 0 1 0 18z" fill="currentColor"/>""" + Close;
    public const string Indent = Open + """<path d="M11 6h9M11 10h9M11 14h9M4 18h16M4 8l3 2-3 2"/>""" + Close;
    public const string Outdent = Open + """<path d="M11 6h9M11 10h9M11 14h9M4 18h16M7 8l-3 2 3 2"/>""" + Close;
    public const string Embed = Open + Frame + """<path d="M10 9.5v5l4.5-2.5z"/>""" + Close;
    public const string HorizontalRule = Open + """<path d="M3 12h18M7 7h10M7 17h10"/>""" + Close;
    public const string TextColor = Open + """<path d="M7 16l5-12 5 12M8.8 11.5h6.4M4 20h16"/>""" + Close;
    public const string Highlighter = Open + """<path d="M14.5 4.5l5 5-7 7h-5v-5zM7.5 11.5l5 5M4 20h16"/>""" + Close;
    public const string Fullscreen = Open + """<path d="M4 9V4h5M20 9V4h-5M4 15v5h5M20 15v5h-5"/>""" + Close;
    public const string Search = Open + """<circle cx="10.5" cy="10.5" r="6"/><path d="M15 15l5 5"/>""" + Close;
    public const string Omega = Open + """<path d="M4 20h5v-2.5a7 7 0 1 1 6 0V20h5"/>""" + Close;
    public const string PastePlainText = Open + """<path d="M9 4h6v3H9zM9 5.5H6a1 1 0 0 0-1 1V20a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1V6.5a1 1 0 0 0-1-1h-3M9 11h6M12 11v6"/>""" + Close;
    public const string NoColor = Open + """<rect x="4" y="4" width="16" height="16" rx="3"/><path d="M5 19L19 5"/>""" + Close;
    public const string ChevronDown =Open + """<path d="M6 9l6 6 6-6"/>""" + Close;
    public const string Check = Open + """<path d="M5 12.5l4.5 4.5L19 7.5"/>""" + Close;
    public const string Dismiss = Open + """<path d="M6 6l12 12M18 6L6 18"/>""" + Close;

    public static string Swatch(string color) => $"""{Open}<rect x="4" y="4" width="16" height="16" rx="3" fill="{color}"/>{Close}""";
}
