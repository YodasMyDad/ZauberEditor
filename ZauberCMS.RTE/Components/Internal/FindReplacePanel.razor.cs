using System.ComponentModel;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Components.Internal;

/// <summary>Finds and replaces text. Opened by the built-in find and replace item; not for direct use.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed partial class FindReplacePanel
{
    private string _find = string.Empty;
    private string _replacement = string.Empty;
    private bool _matchCase;
    private string? _status;

    /// <inheritdoc />
    public override string Title => "Find and replace";

    private bool IsEmpty => _find.Length == 0;

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && await Api.GetSelectionAsync() is { IsCollapsed: false, Text: var text } && !text.Contains('\n'))
        {
            _find = text;
            StateHasChanged();
        }
    }

    private void ResetStatus() => _status = null;

    private async Task FindNextAsync()
    {
        if (!IsEmpty)
        {
            Show(await Api.FindAsync(_find, _matchCase));
        }
    }

    private async Task ReplaceAsync() => Show(await Api.ReplaceAsync(_find, _replacement, _matchCase));

    private async Task ReplaceAllAsync()
    {
        var count = await Api.ReplaceAllAsync(_find, _replacement, _matchCase);
        _status = count switch
        {
            0 => "No matches.",
            1 => "Replaced 1 match.",
            _ => $"Replaced {count} matches."
        };
    }

    private void Show(FindResult result) => _status = result switch
    {
        { Count: 0 } => "No matches.",
        { Current: > 0 } => $"Match {result.Current} of {result.Count}.",
        _ => result.Count == 1 ? "1 match." : $"{result.Count} matches."
    };
}
