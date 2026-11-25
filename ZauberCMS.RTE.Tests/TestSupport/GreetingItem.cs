using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Tests.TestSupport;

internal sealed class GreetingItem(IGreeter greeter) : ToolbarItemBase
{
    public const string ItemId = "greeting";

    public override string Id => ItemId;

    public override string Label => greeter.Greeting;

    public IGreeter Greeter => greeter;

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken) => api.InsertTextAsync(greeter.Greeting, cancellationToken);
}
