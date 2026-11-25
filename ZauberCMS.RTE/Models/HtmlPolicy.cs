using System.Collections.Frozen;

namespace ZauberCMS.RTE.Models;

/// <summary>The HTML kept when content is loaded, pasted, inserted or edited as source.</summary>
public sealed record HtmlPolicy
{
    private static readonly FrozenSet<string> DefaultTags = FrozenSet.Create(
        StringComparer.OrdinalIgnoreCase,
        "p", "br", "div", "span", "strong", "em", "b", "i", "u", "s", "strike", "sub", "sup", "code", "pre",
        "h1", "h2", "h3", "h4", "h5", "h6", "blockquote", "ul", "ol", "li",
        "table", "caption", "colgroup", "col", "thead", "tbody", "tfoot", "tr", "th", "td",
        "a", "img", "figure", "figcaption", "hr");

    private static readonly FrozenSet<string> DefaultAttributes = FrozenSet.Create(
        StringComparer.OrdinalIgnoreCase,
        "class", "id", "title", "lang", "dir", "role", "style", "data-*", "aria-*",
        "href", "target", "rel", "src", "alt", "width", "height", "colspan", "rowspan", "scope");

    /// <summary>Allowed element names (case-insensitive).</summary>
    public IReadOnlySet<string> AllowedTags { get; init; } = DefaultTags;

    /// <summary>Allowed attribute names; <c>data-*</c> and <c>aria-*</c> allow those families.</summary>
    public IReadOnlySet<string> AllowedAttributes { get; init; } = DefaultAttributes;

    /// <summary>Attributes allowed only on the given tag, in addition to <see cref="AllowedAttributes"/> (case-insensitive).</summary>
    public IReadOnlyDictionary<string, IReadOnlySet<string>> TagAttributes { get; init => field = Normalise(value); } =
        FrozenDictionary<string, IReadOnlySet<string>>.Empty;

    /// <summary>Whether images may use <c>data:</c> URLs.</summary>
    public bool AllowDataUrls { get; init; } = true;

    /// <summary>Whether images may load from other origins.</summary>
    public bool AllowExternalImages { get; init; } = true;

    /// <summary>Whether both policies allow the same tags, attributes and image sources.</summary>
    /// <param name="other">The policy to compare with.</param>
    public bool Equals(HtmlPolicy? other) =>
        other is not null
        && AllowDataUrls == other.AllowDataUrls
        && AllowExternalImages == other.AllowExternalImages
        && ValueEquality.Set(AllowedTags, other.AllowedTags)
        && ValueEquality.Set(AllowedAttributes, other.AllowedAttributes)
        && ValueEquality.SetMap(TagAttributes, other.TagAttributes);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(AllowDataUrls, AllowExternalImages);

    private static FrozenDictionary<string, IReadOnlySet<string>> Normalise(IReadOnlyDictionary<string, IReadOnlySet<string>> tagAttributes)
    {
        ArgumentNullException.ThrowIfNull(tagAttributes);
        return tagAttributes
            .GroupBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .ToFrozenDictionary(
                group => group.Key,
                IReadOnlySet<string> (group) => group.SelectMany(pair => pair.Value).ToFrozenSet(StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase);
    }
}
