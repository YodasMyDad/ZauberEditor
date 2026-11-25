using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Tests.TestSupport;

internal sealed class TrackedItem : ToolbarItemBase, IDisposable
{
    public const string ItemId = "tracked";

    public TrackedItem(ItemTracker tracker) => tracker.Created.Add(this);

    public override string Id => ItemId;

    public override string Label => "Tracked";

    public bool IsDisposed { get; private set; }

    public void Dispose() => IsDisposed = true;

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken) => Task.CompletedTask;
}
