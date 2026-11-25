using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Tests.TestSupport;

internal sealed class ShoutingBoldItem : ToolbarItemBase
{
    public override string Id => "BOLD";

    public override string Label => "Custom bold";

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken) => api.ToggleMarkAsync("b", cancellationToken);
}
