namespace ZauberRTE.Web.Toolbar;

public static class DemoIcons
{
    private const string Open = """<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false">""";
    private const string Close = "</svg>";

    public const string Highlighter = Open + """<path d="M9 11l-5 5v3h3l5-5M9 11l6-6 4 4-6 6M9 11l4 4M14 20h6"/>""" + Close;
    public const string Clock = Open + """<circle cx="12" cy="12" r="9"/><path d="M12 7v5l3 2"/>""" + Close;
    public const string Callout = Open + """<rect x="3" y="4" width="18" height="16" rx="2"/><path d="M7 4v16M11 9h6M11 13h4"/>""" + Close;
    public const string Palette = Open + """<path d="M12 3a9 9 0 1 0 0 18c1.1 0 1.5-.8 1.5-1.5 0-1.2-1-1.5-1-2.5s.8-1.5 2-1.5H17a4 4 0 0 0 4-4c0-4.7-4-8.5-9-8.5z"/><circle cx="7.5" cy="11" r="1"/><circle cx="10" cy="7" r="1"/><circle cx="15" cy="7.5" r="1"/>""" + Close;

    public static string Swatch(string cssClass) =>
        $"""<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" class="{cssClass}" aria-hidden="true" focusable="false"><rect x="4" y="4" width="16" height="16" rx="4" fill="currentColor"/></svg>""";
}
