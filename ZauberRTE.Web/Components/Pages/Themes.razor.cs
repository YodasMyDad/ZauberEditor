using ZauberCMS.RTE.Models;

namespace ZauberRTE.Web.Components.Pages;

public sealed partial class Themes
{
    private const string SampleHtml = "<p>The quick <strong>brown</strong> fox jumps over the <em>lazy</em> dog.</p>";

    private static readonly Theme[] FixedThemes = [Theme.Light, Theme.Dark, Theme.Auto];

    private static readonly Theme[] AllThemes = Enum.GetValues<Theme>();

    private static readonly ToolbarLayout ToggleLayout = new(
        new ToolbarGroup(ToolbarItemIds.Bold, ToolbarItemIds.Italic, ToolbarItemIds.Underline),
        new ToolbarSeparator(),
        new ToolbarGroup(ToolbarItemIds.ThemeToggle));

    private Theme _theme = Theme.Light;
}
