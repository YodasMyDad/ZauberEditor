using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Tests.Models;

public sealed class EmbedProviderTests
{
    private const string VideoId = "dQw4w9WgXcQ";

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("http://youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("https://m.youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("https://music.youtube.com/watch?v=dQw4w9WgXcQ&list=PL123")]
    [InlineData("https://www.youtube.com/watch?feature=share&v=dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ&t=42s")]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ#t=1m2s")]
    [InlineData("https://youtu.be/dQw4w9WgXcQ")]
    [InlineData("https://youtu.be/dQw4w9WgXcQ?t=10")]
    [InlineData("https://youtu.be/dQw4w9WgXcQ?si=abc123")]
    [InlineData("https://www.youtube.com/embed/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/embed/dQw4w9WgXcQ?rel=0&start=30")]
    [InlineData("https://www.youtube-nocookie.com/embed/dQw4w9WgXcQ")]
    [InlineData("https://youtube.com/shorts/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/shorts/dQw4w9WgXcQ?feature=share")]
    [InlineData("https://www.youtube.com/live/dQw4w9WgXcQ")]
    [InlineData("HTTPS://WWW.YOUTUBE.COM/watch?v=dQw4w9WgXcQ")]
    [InlineData("  https://youtu.be/dQw4w9WgXcQ  ")]
    [InlineData("youtu.be/dQw4w9WgXcQ")]
    [InlineData("www.youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("//www.youtube.com/embed/dQw4w9WgXcQ")]
    public void YouTubeLinksYieldTheVideoId(string url)
    {
        EmbedProvider.YouTube.TryMatch(url, out var id).ShouldBeTrue();
        id.ShouldBe(VideoId);
    }

    [Theory]
    [InlineData("https://vimeo.com/76979871", "76979871")]
    [InlineData("https://www.vimeo.com/76979871?share=copy", "76979871")]
    [InlineData("https://vimeo.com/76979871#t=30s", "76979871")]
    [InlineData("https://vimeo.com/channels/staffpicks/123456", "123456")]
    [InlineData("https://vimeo.com/groups/shortfilms/videos/98765", "98765")]
    [InlineData("https://player.vimeo.com/video/76979871", "76979871")]
    [InlineData("https://player.vimeo.com/video/76979871?h=abc&badge=0", "76979871")]
    [InlineData("vimeo.com/76979871", "76979871")]
    public void VimeoLinksYieldTheVideoId(string url, string expected)
    {
        EmbedProvider.Vimeo.TryMatch(url, out var id).ShouldBeTrue();
        id.ShouldBe(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not a url")]
    [InlineData("https://www.youtube.com/watch?v=short")]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQextra")]
    [InlineData("https://www.youtube.com/watch?list=PL123")]
    [InlineData("https://www.youtube.com/channel/UC123")]
    [InlineData("https://youtube.com.evil.example/watch?v=dQw4w9WgXcQ")]
    [InlineData("https://evil.example/?next=https://youtu.be/dQw4w9WgXcQ")]
    [InlineData("https://evil.example/youtu.be/dQw4w9WgXcQ")]
    [InlineData("https://notyoutube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("javascript:alert('youtu.be/dQw4w9WgXcQ')")]
    [InlineData("ftp://youtu.be/dQw4w9WgXcQ")]
    [InlineData("data:text/html,youtu.be/dQw4w9WgXcQ")]
    public void OtherLinksDoNotMatchYouTube(string? url) =>
        EmbedProvider.YouTube.TryMatch(url, out _).ShouldBeFalse();

    [Theory]
    [InlineData("https://vimeo.com/")]
    [InlineData("https://vimeo.com/about")]
    [InlineData("https://vimeo.com/76979871abc")]
    [InlineData("https://player.vimeo.com/video/")]
    [InlineData("https://vimeo.com.evil.example/76979871")]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ")]
    public void OtherLinksDoNotMatchVimeo(string url) =>
        EmbedProvider.Vimeo.TryMatch(url, out _).ShouldBeFalse();

    [Fact]
    public void OverlongUrlsAreRejected() =>
        EmbedProvider.YouTube.TryMatch($"https://youtu.be/{VideoId}?x={new string('a', 2100)}", out _).ShouldBeFalse();

    [Fact]
    public void TryMatchFindsTheFirstMatchingProvider()
    {
        EmbedProvider.TryMatch(EmbedProvider.Defaults, "https://vimeo.com/76979871", out var provider, out var id).ShouldBeTrue();
        provider.ShouldBe(EmbedProvider.Vimeo);
        id.ShouldBe("76979871");

        EmbedProvider.TryMatch(EmbedProvider.Defaults, "https://example.com/video/1", out provider, out id).ShouldBeFalse();
        (provider, id).ShouldBe((null, null));
    }

    [Fact]
    public void FindLooksUpThePlaceholderProviderName()
    {
        EmbedProvider.Find(EmbedProvider.Defaults, "youtube").ShouldBeSameAs(EmbedProvider.YouTube);
        EmbedProvider.Find(EmbedProvider.Defaults, " Vimeo ").ShouldBeSameAs(EmbedProvider.Vimeo);
        EmbedProvider.Find(EmbedProvider.Defaults, "dailymotion").ShouldBeNull();
        EmbedProvider.Find(EmbedProvider.Defaults, null).ShouldBeNull();
    }

    [Theory]
    [InlineData("dQw4w9WgXcQ", true)]
    [InlineData("76979871", true)]
    [InlineData("a_b-c", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("../etc", false)]
    [InlineData("a/b", false)]
    [InlineData("a\"b", false)]
    [InlineData("a b", false)]
    public void IdsAreLimitedToUrlSafeCharacters(string? id, bool valid) => EmbedProvider.IsValidId(id).ShouldBe(valid);

    [Fact]
    public void IdsLongerThan64CharactersAreInvalid() => EmbedProvider.IsValidId(new string('a', 65)).ShouldBeFalse();

    [Fact]
    public void BuiltInProvidersBuildCanonicalPlayerUrls()
    {
        EmbedProvider.YouTube.BuildEmbedUrl(VideoId).ShouldBe("https://www.youtube-nocookie.com/embed/dQw4w9WgXcQ");
        EmbedProvider.Vimeo.BuildEmbedUrl("76979871").ShouldBe("https://player.vimeo.com/video/76979871");
    }

    [Fact]
    public void ThumbnailsExistOnlyWhereTheProviderHasThem()
    {
        EmbedProvider.YouTube.BuildThumbnailUrl(VideoId).ShouldBe("https://i.ytimg.com/vi/dQw4w9WgXcQ/hqdefault.jpg");
        EmbedProvider.Vimeo.BuildThumbnailUrl("76979871").ShouldBeNull();
    }

    [Theory]
    [InlineData("../admin")]
    [InlineData("x\" onload=\"alert(1)")]
    [InlineData("")]
    public void BuildingWithAnInvalidIdThrows(string id)
    {
        Should.Throw<ArgumentException>(() => EmbedProvider.YouTube.BuildEmbedUrl(id));
        Should.Throw<ArgumentException>(() => EmbedProvider.YouTube.BuildIframeHtml(id));
    }

    [Fact]
    public void IframeMarkupIsLazySandboxedFromTheReferrerAndTitled() =>
        EmbedProvider.YouTube.BuildIframeHtml(VideoId).ShouldBe(
            "<iframe src=\"https://www.youtube-nocookie.com/embed/dQw4w9WgXcQ\" title=\"YouTube video\" loading=\"lazy\" "
            + "allow=\"accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share\" "
            + "allowfullscreen=\"\" referrerpolicy=\"strict-origin-when-cross-origin\"></iframe>");

    [Fact]
    public void IframeTitlesAreEncoded() =>
        EmbedProvider.Vimeo.BuildIframeHtml("1", "\"><script>alert(1)</script>")
            .ShouldContain("title=\"&quot;&gt;&lt;script&gt;alert(1)&lt;/script&gt;\"");

    [Fact]
    public void CustomProvidersMatchWithTheirOwnPatterns()
    {
        var provider = Custom() with { UrlPatterns = [@"^https://video\.example\.com/v/(?<id>[a-z0-9]+)$"] };

        provider.TryMatch("https://video.example.com/v/abc123", out var id).ShouldBeTrue();
        id.ShouldBe("abc123");
        provider.TryMatch("https://video.example.com/v/abc123/extra", out _).ShouldBeFalse();
        provider.BuildIframeHtml("abc123").ShouldStartWith("<iframe src=\"https://video.example.com/embed/abc123\" title=\"Example video\"");
    }

    [Fact]
    public void ExtractedIdsMustStillBeUrlSafe() =>
        (Custom() with { UrlPatterns = ["^https://video\\.example\\.com/v/(?<id>[^/]+)$"] })
            .TryMatch("https://video.example.com/v/a..b%22", out _).ShouldBeFalse();

    [Theory]
    [InlineData("YouTube")]
    [InlineData("you tube")]
    [InlineData("-youtube")]
    [InlineData("")]
    public void NamesMustBeLowerCaseTokens(string name) =>
        Should.Throw<ArgumentException>(() => Custom() with { Name = name });

    [Theory]
    [InlineData("^https://x\\.example/(\\d+)$")]
    [InlineData("^https://x\\.example/(?<id>\\d+")]
    [InlineData("a++(?<id>b)")]
    [InlineData(" ")]
    public void PatternsMustBeValidAndCaptureAnId(string pattern) =>
        Should.Throw<ArgumentException>(() => Custom() with { UrlPatterns = [pattern] });

    [Fact]
    public void AtLeastOnePatternIsRequired() =>
        Should.Throw<ArgumentException>(() => Custom() with { UrlPatterns = [] });

    [Theory]
    [InlineData("http://video.example.com/embed/{id}")]
    [InlineData("https://video.example.com/embed/")]
    [InlineData("/embed/{id}")]
    [InlineData("javascript:alert('{id}')")]
    public void TemplatesMustBeAbsoluteHttpsUrlsWithAnId(string template)
    {
        Should.Throw<ArgumentException>(() => Custom() with { EmbedUrlTemplate = template });
        Should.Throw<ArgumentException>(() => Custom() with { ThumbnailUrlTemplate = template });
    }

    [Fact]
    public void ProvidersWithTheSameValuesAreEqual()
    {
        var copy = Custom() with { UrlPatterns = [.. Custom().UrlPatterns] };

        copy.ShouldBe(Custom());
        copy.GetHashCode().ShouldBe(Custom().GetHashCode());
        (copy with { DisplayName = "Other" }).ShouldNotBe(Custom());
        (copy with { UrlPatterns = ["^https://other\\.example/(?<id>\\d+)$"] }).ShouldNotBe(Custom());
    }

    [Fact]
    public void DefaultsAreYouTubeThenVimeo() =>
        EmbedProvider.Defaults.Select(provider => provider.Name).ShouldBe(["youtube", "vimeo"]);

    private static EmbedProvider Custom() => new()
    {
        Name = "example",
        DisplayName = "Example",
        UrlPatterns = [@"^https://video\.example\.com/v/(?<id>\d+)$"],
        EmbedUrlTemplate = "https://video.example.com/embed/{id}"
    };
}
