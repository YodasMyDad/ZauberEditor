using Zauber.RTE.Services;
using Zauber.RTE.Components.Panels;

namespace Zauber.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for settings
/// </summary>
public class SettingsItem : ToolbarItemBase
{
    public override string Id => "settings";
    public override string Label => "Settings";
    public override string IconCss => "fa-cog";
    public override ToolbarPlacement Placement => ToolbarPlacement.Custom;
    public override Type? PanelComponent => typeof(Zauber.RTE.Components.ZauberRteSettings);

    public override Task ExecuteAsync(EditorApi api) => api.OpenPanelAsync(typeof(Zauber.RTE.Components.ZauberRteSettings));
}
