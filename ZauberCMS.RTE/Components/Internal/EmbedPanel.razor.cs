using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Components.Internal;

/// <summary>Embeds a video or changes the selected embed. Opened by the built-in embed item; not for direct use.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed partial class EmbedPanel
{
    private string _url = string.Empty;
    private string? _error;

    /// <summary>The embed being edited, or null to insert a new one.</summary>
    [Parameter]
    public EmbedInfo? Embed { get; set; }

    /// <inheritdoc />
    public override string Title => Embed is null ? "Embed video" : "Edit embed";

    private IReadOnlyList<EmbedProvider> Providers => Api.Settings.EmbedProviders;

    private string Help => Providers.Count == 0
        ? "Video embeds are turned off for this editor."
        : $"Paste a link to a {ProviderNames} video.";

    private string ProviderNames => Providers.Count == 1
        ? Providers[0].DisplayName
        : $"{string.Join(", ", Providers.SkipLast(1).Select(provider => provider.DisplayName))} or {Providers[^1].DisplayName}";

    /// <inheritdoc />
    protected override void OnInitialized() => _url = Embed?.Url ?? string.Empty;

    private async Task SaveAsync()
    {
        var url = _url.Trim();
        if (url.Length == 0)
        {
            return;
        }

        if (!EmbedProvider.TryMatch(Providers, url, out _, out _))
        {
            _error = Providers.Count == 0 ? Help : $"That isn't a {ProviderNames} video link.";
            return;
        }

        if (!await Api.InsertEmbedAsync(url))
        {
            _error = "This editor's HTML policy doesn't allow embeds.";
            return;
        }

        await CloseAsync();
    }
}
