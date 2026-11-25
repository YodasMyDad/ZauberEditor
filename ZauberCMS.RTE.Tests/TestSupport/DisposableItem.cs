using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Tests.TestSupport;

internal sealed class DisposableItem : ToolbarItemBase, IDisposable
{
    public const string ItemId = "disposable";

    public override string Id => ItemId;

    public override string Label => "Disposable";

    public bool IsDisposed { get; private set; }

    public void Dispose() => IsDisposed = true;

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken) => Task.CompletedTask;
}
