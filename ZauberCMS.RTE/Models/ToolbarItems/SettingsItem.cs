namespace ZauberCMS.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for settings
/// </summary>
public class SettingsItem : ToolbarItemBase
{
    public override string Id => "settings";
    public override string Label => "Settings";
    public override string IconCss => "fa-cog";
    public override ToolbarPlacement Placement => ToolbarPlacement.Custom;
    public override Type? PanelComponent => typeof(Components.ZauberRteSettings);

    public override Task ExecuteAsync(IEditorApi api) => api.OpenPanelAsync(typeof(Components.ZauberRteSettings));
}
