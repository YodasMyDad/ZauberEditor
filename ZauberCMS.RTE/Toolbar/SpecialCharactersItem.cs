using ZauberCMS.RTE.Components.Internal;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Toolbar;

internal sealed class SpecialCharactersItem : ToolbarItemBase
{
    public override string Id => ToolbarItemIds.SpecialCharacters;

    public override string Label => "Special character";

    public override string? IconSvg => Icons.Omega;

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken) =>
        api.OpenPanelAsync<SpecialCharactersPanel>(cancellationToken: cancellationToken);
}
