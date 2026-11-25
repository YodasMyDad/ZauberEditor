using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Tests.TestSupport;

internal sealed class UnconstructableItem(string label) : ToolbarItemBase
{
    public const string ItemId = "unconstructable";

    public override string Id => ItemId;

    public override string Label => label;

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken) => Task.CompletedTask;
}
