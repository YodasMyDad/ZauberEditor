using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Toolbar;

internal sealed class ViewSourceItem : ToolbarItemBase
{
    public override string Id => ToolbarItemIds.ViewSource;

    public override string Label => "View source";

    public override string? IconSvg => Icons.Source;

    public override bool IsToggle => true;

    public override bool IsEnabled(EditorState state) => !state.IsReadOnly;

    public override bool IsActive(EditorState state) => state.IsSourceView;

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken) => api.ToggleSourceViewAsync(cancellationToken);
}
