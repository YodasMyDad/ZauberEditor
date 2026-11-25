namespace ZauberRTE.Web.Components.Layout;

public sealed partial class NavMenu
{
    private static readonly (string Href, string Title)[] Links =
    [
        ("", "Full toolbar"),
        ("custom-items", "Custom items & panel"),
        ("toolbar-catalog", "Toolbar catalogue"),
        ("multiple-editors", "Multiple editors"),
        ("read-only", "Read-only"),
        ("themes", "Themes"),
        ("large-content", "Large content"),
        ("webassembly", "WebAssembly"),
        ("auto", "Auto render mode")
    ];
}
