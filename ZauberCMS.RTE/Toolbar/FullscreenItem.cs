using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Toolbar;

internal sealed class FullscreenItem : ToolbarItemBase
{
    public override string Id => ToolbarItemIds.Fullscreen;

    public override string Label => "Full screen";

    public override string? Tooltip => "Full screen (Esc to exit)";

    public override string? IconSvg => Icons.Fullscreen;

    public override bool IsToggle => true;

    public override bool IsEnabled(EditorState state) => true;

    public override bool IsActive(EditorState state) => state.IsFullscreen;

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken) =>
        api.SetFullscreenAsync(!api.State.IsFullscreen, cancellationToken);
}
