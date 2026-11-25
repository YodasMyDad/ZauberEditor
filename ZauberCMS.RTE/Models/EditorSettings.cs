namespace ZauberCMS.RTE.Models;

/// <summary>Configuration for an editor instance. Pass a new instance to apply changes.</summary>
public sealed record EditorSettings
{
    /// <summary>The toolbar layout used when the component's <c>Toolbar</c> parameter is not set.</summary>
    public ToolbarLayout ToolbarLayout { get; init; } = ToolbarLayout.Full;

    /// <summary>Editor size.</summary>
    public EditorDimensions Dimensions { get; init; } = new();

    /// <summary>Image insertion and resizing rules.</summary>
    public ImageConstraints ImageConstraints { get; init; } = new();

    /// <summary>The HTML allowed in the content.</summary>
    public HtmlPolicy HtmlPolicy { get; init; } = new();

    /// <summary>Maximum number of undo steps.</summary>
    public int HistoryLimit { get; init; } = 100;

    /// <summary>How long typing must pause before the bound value updates.</summary>
    public TimeSpan ChangeDelay { get; init; } = TimeSpan.FromMilliseconds(250);

    /// <summary>Largest content, in UTF-8 bytes, the editor will transfer to .NET.</summary>
    public long MaxContentBytes { get; init; } = 10 * 1024 * 1024;

    /// <summary>The video sites that can be embedded; iframes from other sites are removed. Defaults to <see cref="EmbedProvider.Defaults"/>.</summary>
    public IReadOnlyList<EmbedProvider> EmbedProviders { get; init => field = UniqueProviders(value); } = EmbedProvider.Defaults;

    /// <summary>Whether both have the same values, comparing collections by content.</summary>
    /// <param name="other">The settings to compare with.</param>
    public bool Equals(EditorSettings? other) =>
        other is not null
        && ToolbarLayout == other.ToolbarLayout
        && Dimensions == other.Dimensions
        && ImageConstraints == other.ImageConstraints
        && HtmlPolicy == other.HtmlPolicy
        && HistoryLimit == other.HistoryLimit
        && ChangeDelay == other.ChangeDelay
        && MaxContentBytes == other.MaxContentBytes
        && ValueEquality.Sequence(EmbedProviders, other.EmbedProviders);

    /// <inheritdoc />
    public override int GetHashCode() =>
        HashCode.Combine(ToolbarLayout, Dimensions, ImageConstraints, HtmlPolicy, HistoryLimit, ChangeDelay, MaxContentBytes);

    private static EmbedProvider[] UniqueProviders(IReadOnlyList<EmbedProvider> providers)
    {
        var list = LayoutGuard.NoNulls(providers, nameof(providers));
        return list.DistinctBy(provider => provider.Name).Count() == list.Count
            ? [.. list]
            : throw new ArgumentException("Embed provider names must be unique.", nameof(providers));
    }
}
