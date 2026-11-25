using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Toolbar;

internal sealed class HorizontalRuleItem : ToolbarItemBase
{
    private const string Html = "<hr><p><br></p>";

    public override string Id => ToolbarItemIds.HorizontalRule;

    public override string Label => "Horizontal line";

    public override string? IconSvg => Icons.HorizontalRule;

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken) => api.InsertHtmlAsync(Html, cancellationToken);
}
