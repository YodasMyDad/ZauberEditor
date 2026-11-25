using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Toolbar;

internal sealed class ThemeToggleItem : ToolbarItemBase
{
    public override string Id => ToolbarItemIds.ThemeToggle;

    public override string Label => "Dark theme";

    public override string? Tooltip => "Switch between light and dark theme";

    public override string? IconSvg => Icons.Contrast;

    public override bool IsToggle => true;

    public override bool IsEnabled(EditorState state) => true;

    public override bool IsActive(EditorState state) => state.IsDark;

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken) =>
        api.SetThemeAsync(api.State.IsDark ? Theme.Light : Theme.Dark, cancellationToken);
}
