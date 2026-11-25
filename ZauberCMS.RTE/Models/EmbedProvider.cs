using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace ZauberCMS.RTE.Models;

/// <summary>A video site whose links the editor turns into embed placeholders, and how hosts turn them into players.</summary>
public sealed partial record EmbedProvider
{
    private const string IdToken = "{id}";
    private const int MaxUrlLength = 2048;
    private const string IframeAllow = "accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share";
    private static readonly TimeSpan MatchTimeout = TimeSpan.FromMilliseconds(250);

    private Regex[] _patterns = [];

    /// <summary>YouTube: watch, short, embed, shorts and live links; players use youtube-nocookie.com.</summary>
    public static EmbedProvider YouTube { get; } = new()
    {
        Name = "youtube",
        DisplayName = "YouTube",
        UrlPatterns =
        [
            @"^https?://(?:www\.|m\.|music\.)?youtube\.com/watch/?\?(?:[^#]*&)?v=(?<id>[A-Za-z0-9_-]{11})(?=[&#]|$)",
            @"^https?://(?:www\.|m\.)?youtube(?:-nocookie)?\.com/(?:embed|shorts|live|v)/(?<id>[A-Za-z0-9_-]{11})(?=[/?#]|$)",
            @"^https?://youtu\.be/(?<id>[A-Za-z0-9_-]{11})(?=[/?#]|$)"
        ],
        EmbedUrlTemplate = "https://www.youtube-nocookie.com/embed/{id}",
        ThumbnailUrlTemplate = "https://i.ytimg.com/vi/{id}/hqdefault.jpg"
    };

    /// <summary>Vimeo: video page and player links.</summary>
    public static EmbedProvider Vimeo { get; } = new()
    {
        Name = "vimeo",
        DisplayName = "Vimeo",
        UrlPatterns =
        [
            @"^https?://(?:www\.)?vimeo\.com/(?:channels/[A-Za-z0-9_-]+/|groups/[A-Za-z0-9_-]+/videos/)?(?<id>\d+)(?=[/?#]|$)",
            @"^https?://player\.vimeo\.com/video/(?<id>\d+)(?=[/?#]|$)"
        ],
        EmbedUrlTemplate = "https://player.vimeo.com/video/{id}"
    };

    /// <summary>The built-in providers: YouTube and Vimeo.</summary>
    public static IReadOnlyList<EmbedProvider> Defaults { get; } = [YouTube, Vimeo];

    /// <summary>Stored in <c>data-embed</c>: lower-case letters, digits and hyphens, up to 32 characters.</summary>
    public required string Name { get; init => field = ValidName(value); }

    /// <summary>The name shown to users, such as <c>YouTube</c>.</summary>
    public required string DisplayName
    {
        get;
        init
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            field = value.Trim();
        }
    }

    /// <summary>JavaScript-compatible regular expressions, matched case-insensitively against the URL, each with an <c>id</c> group.</summary>
    public required IReadOnlyList<string> UrlPatterns
    {
        get;
        init
        {
            _patterns = Compile(value);
            field = [.. value];
        }
    }

    /// <summary>The absolute https player URL, with <c>{id}</c> where the video id goes.</summary>
    public required string EmbedUrlTemplate { get; init => field = ValidTemplate(value); }

    /// <summary>An absolute https preview image URL with <c>{id}</c>, or null when the site has none.</summary>
    public string? ThumbnailUrlTemplate { get; init => field = value is null ? null : ValidTemplate(value); }

    /// <summary>Whether <paramref name="id"/> is a valid video id: 1 to 64 letters, digits, hyphens or underscores.</summary>
    /// <param name="id">The id to check.</param>
    public static bool IsValidId([NotNullWhen(true)] string? id) => id is not null && IdPattern().IsMatch(id);

    /// <summary>Finds the first provider that matches <paramref name="url"/>.</summary>
    /// <param name="providers">The providers to try, in order.</param>
    /// <param name="url">A video link; <c>https://</c> is assumed when it has no scheme.</param>
    /// <param name="provider">The matching provider.</param>
    /// <param name="id">The video id.</param>
    public static bool TryMatch(
        IEnumerable<EmbedProvider> providers,
        string? url,
        [NotNullWhen(true)] out EmbedProvider? provider,
        [NotNullWhen(true)] out string? id)
    {
        ArgumentNullException.ThrowIfNull(providers);
        foreach (var candidate in providers)
        {
            if (candidate.TryMatch(url, out id))
            {
                provider = candidate;
                return true;
            }
        }

        (provider, id) = (null, null);
        return false;
    }

    /// <summary>Finds the provider stored in a placeholder's <c>data-embed</c> attribute, or null.</summary>
    /// <param name="providers">The providers to search.</param>
    /// <param name="name">The provider name.</param>
    public static EmbedProvider? Find(IEnumerable<EmbedProvider> providers, string? name)
    {
        ArgumentNullException.ThrowIfNull(providers);
        return providers.FirstOrDefault(provider => string.Equals(provider.Name, name?.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Extracts the video id when <paramref name="url"/> is a link to this provider.</summary>
    /// <param name="url">A video link; <c>https://</c> is assumed when it has no scheme.</param>
    /// <param name="id">The video id.</param>
    public bool TryMatch(string? url, [NotNullWhen(true)] out string? id)
    {
        id = null;
        if (Normalise(url) is not { } absolute)
        {
            return false;
        }

        try
        {
            id = _patterns.Select(pattern => pattern.Match(absolute).Groups["id"].Value).FirstOrDefault(IsValidId);
        }
        catch (RegexMatchTimeoutException)
        {
            id = null;
        }

        return id is not null;
    }

    /// <summary>The canonical player URL for a video, such as <c>https://www.youtube-nocookie.com/embed/{id}</c>.</summary>
    /// <param name="id">The video id.</param>
    /// <exception cref="ArgumentException"><paramref name="id"/> is not a valid id.</exception>
    public string BuildEmbedUrl(string id) => Fill(EmbedUrlTemplate, id);

    /// <summary>The preview image URL for a video, or null when the provider has none.</summary>
    /// <param name="id">The video id.</param>
    /// <exception cref="ArgumentException"><paramref name="id"/> is not a valid id.</exception>
    public string? BuildThumbnailUrl(string id) => ThumbnailUrlTemplate is { } template ? Fill(template, id) : null;

    /// <summary>Safe, encoded iframe markup that plays the video, for hosts that expand placeholders when rendering.</summary>
    /// <param name="id">The video id from <c>data-embed-id</c>.</param>
    /// <param name="title">The iframe's accessible title; defaults to "<see cref="DisplayName"/> video".</param>
    /// <exception cref="ArgumentException"><paramref name="id"/> is not a valid id.</exception>
    public string BuildIframeHtml(string id, string? title = null) =>
        HtmlBuilder.Element("iframe")
            .Attr("src", BuildEmbedUrl(id))
            .Attr("title", string.IsNullOrWhiteSpace(title) ? $"{DisplayName} video" : title.Trim())
            .Attr("loading", "lazy")
            .Attr("allow", IframeAllow)
            .Attr("allowfullscreen", string.Empty)
            .Attr("referrerpolicy", "strict-origin-when-cross-origin")
            .Build();

    /// <summary>Whether both have the same name, display name, patterns and templates.</summary>
    /// <param name="other">The provider to compare with.</param>
    public bool Equals(EmbedProvider? other) =>
        other is not null
        && Name == other.Name
        && DisplayName == other.DisplayName
        && EmbedUrlTemplate == other.EmbedUrlTemplate
        && ThumbnailUrlTemplate == other.ThumbnailUrlTemplate
        && ValueEquality.Sequence(UrlPatterns, other.UrlPatterns);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Name, EmbedUrlTemplate);

    private static string? Normalise(string? url)
    {
        var trimmed = url?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > MaxUrlLength)
        {
            return null;
        }

        var absolute = trimmed.StartsWith("//", StringComparison.Ordinal) ? $"https:{trimmed}"
            : SchemePattern().IsMatch(trimmed) ? trimmed
            : $"https://{trimmed}";
        return Uri.TryCreate(absolute, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https" ? absolute : null;
    }

    private static string Fill(string template, string id) =>
        IsValidId(id) ? template.Replace(IdToken, id, StringComparison.Ordinal) : throw new ArgumentException($"'{id}' is not a valid embed id.", nameof(id));

    private static string ValidName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return NamePattern().IsMatch(name)
            ? name
            : throw new ArgumentException($"'{name}' is not a valid provider name: use lower-case letters, digits and hyphens.", nameof(name));
    }

    private static string ValidTemplate(string template)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(template);
        var valid = template.Contains(IdToken, StringComparison.Ordinal)
            && Uri.TryCreate(template.Replace(IdToken, "id", StringComparison.Ordinal), UriKind.Absolute, out var uri)
            && uri.Scheme == Uri.UriSchemeHttps;
        return valid ? template : throw new ArgumentException($"'{template}' must be an absolute https URL containing {IdToken}.", nameof(template));
    }

    private static Regex[] Compile(IReadOnlyList<string> patterns)
    {
        ArgumentNullException.ThrowIfNull(patterns);
        if (patterns.Count == 0)
        {
            throw new ArgumentException("An embed provider needs at least one URL pattern.", nameof(patterns));
        }

        return [.. patterns.Select(CompilePattern)];
    }

    private static Regex CompilePattern(string pattern)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pattern);
        Regex regex;
        try
        {
            regex = new(pattern, RegexOptions.ECMAScript | RegexOptions.IgnoreCase, MatchTimeout);
        }
        catch (ArgumentException exception)
        {
            throw new ArgumentException($"'{pattern}' is not a valid JavaScript-compatible regular expression.", nameof(pattern), exception);
        }

        return regex.GetGroupNames().Contains("id")
            ? regex
            : throw new ArgumentException($"'{pattern}' has no (?<id>...) group.", nameof(pattern));
    }

    [GeneratedRegex("^[a-z0-9][a-z0-9-]{0,31}$")]
    private static partial Regex NamePattern();

    [GeneratedRegex("^[A-Za-z0-9_-]{1,64}$")]
    private static partial Regex IdPattern();

    [GeneratedRegex("^[A-Za-z][A-Za-z0-9+.-]*:")]
    private static partial Regex SchemePattern();
}
