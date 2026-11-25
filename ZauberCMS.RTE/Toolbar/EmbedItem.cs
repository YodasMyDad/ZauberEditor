using ZauberCMS.RTE.Components.Internal;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Toolbar;

internal sealed class EmbedItem : ToolbarItemBase
{
    public override string Id => ToolbarItemIds.Embed;

    public override string Label => "Embed video";

    public override string? IconSvg => Icons.Embed;

    public override bool IsToggle => true;

    public override bool IsActive(EditorState state) => state.InEmbed;

    public override async Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken)
    {
        Dictionary<string, object?> parameters = new()
        {
            [nameof(EmbedPanel.Embed)] = await api.GetEmbedAtCursorAsync(cancellationToken)
        };
        await api.OpenPanelAsync<EmbedPanel>(parameters, cancellationToken);
    }
}
