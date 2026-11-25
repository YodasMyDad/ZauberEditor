using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Toolbar;

internal sealed class IndentItem(bool outdent) : ToolbarItemBase
{
    public override string Id => outdent ? ToolbarItemIds.Outdent : ToolbarItemIds.Indent;

    public override string Label => outdent ? "Decrease indent" : "Increase indent";

    public override string? Tooltip => outdent ? "Decrease indent (Shift+Tab in a list)" : "Increase indent (Tab in a list)";

    public override string? IconSvg => outdent ? Icons.Outdent : Icons.Indent;

    public override bool IsEnabled(EditorState state) => base.IsEnabled(state) && (!outdent || state.CanOutdent);

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken) =>
        outdent ? api.OutdentAsync(cancellationToken) : api.IndentAsync(cancellationToken);
}
