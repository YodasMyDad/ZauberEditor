using System.Collections.Frozen;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Toolbar;

internal sealed class AlignItem(string id, string label, string iconSvg, TextAlignment alignment, string cssValue)
    : ToolbarItemBase
{
    private readonly FrozenDictionary<string, string> _styles =
        FrozenDictionary.ToFrozenDictionary<string, string>([KeyValuePair.Create("text-align", cssValue)]);

    public override string Id => id;

    public override string Label => label;

    public override string? IconSvg => iconSvg;

    public override bool IsToggle => true;

    public override bool IsActive(EditorState state) => state.Alignment == alignment;

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken) => api.SetBlockStyleAsync(_styles, cancellationToken);
}
