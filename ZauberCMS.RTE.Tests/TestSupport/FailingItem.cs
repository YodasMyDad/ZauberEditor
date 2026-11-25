using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Tests.TestSupport;

internal sealed class FailingItem : ToolbarItemBase
{
    public const string ItemId = "failing";

    public override string Id => ItemId;

    public override string Label => "Fail";

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken) => throw new InvalidOperationException("The item failed.");
}
