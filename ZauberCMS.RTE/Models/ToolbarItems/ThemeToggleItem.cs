namespace ZauberCMS.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for toggling light/dark theme
/// </summary>
public class ThemeToggleItem : ToolbarItemBase
{
    public override string Id => "themeToggle";
    public override string Label => "Toggle Theme";
    public override string Tooltip => "Switch between light and dark theme";
    public override string IconCss => "fa-adjust";
    public override ToolbarPlacement Placement => ToolbarPlacement.Custom;

    public override async Task ExecuteAsync(IEditorApi api)
    {
        // This will be handled by the component to toggle theme
        await api.ShowToastAsync("Theme toggled", ToastType.Info);
    }
}
