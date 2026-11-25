using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Tests.TestSupport;

internal sealed class RecordingItem : ToolbarItemBase
{
    public const string ItemId = "recording";

    private readonly List<IEditorApi> _executions = [];
    private readonly List<CancellationToken> _tokens = [];

    public override string Id => ItemId;

    public override string Label => "Record";

    public IReadOnlyList<IEditorApi> Executions => _executions;

    public IReadOnlyList<CancellationToken> Tokens => _tokens;

    public override Task ExecuteAsync(IEditorApi api, CancellationToken cancellationToken)
    {
        _executions.Add(api);
        _tokens.Add(cancellationToken);
        return Task.CompletedTask;
    }
}
