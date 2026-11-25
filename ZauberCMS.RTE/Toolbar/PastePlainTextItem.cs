using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Toolbar;

internal sealed class PastePlainTextItem : ToolbarItemBase
{
    public override string Id => ToolbarItemIds.PastePlainText;

    public override string Label => "Paste as plain text";

    public override string? Tooltip => "Paste as plain text (Ctrl+Shift+V pastes plain text once)";

    public override string? IconSvg => Icons.PastePlainText;

    public override bool IsToggle => true;

    public override bool IsActive(EditorState state) => state.IsPlainTextPaste;

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken) =>
        api.SetPlainTextPasteAsync(!api.State.IsPlainTextPaste, cancellationToken);
}
