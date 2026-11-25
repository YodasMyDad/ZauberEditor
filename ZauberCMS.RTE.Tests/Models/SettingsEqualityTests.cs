using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Tests.Models;

public sealed class SettingsEqualityTests
{
    [Fact]
    public void SettingsBuiltFromEqualPartsAreEqual()
    {
        var first = Build(["p", "strong"], [".png"]);
        var second = Build(["strong", "p"], [".png"]);

        first.ShouldNotBeSameAs(second);
        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }

    [Fact]
    public void ChangedPolicyTagsMakeSettingsDifferent() =>
        Build(["p", "strong"], [".png"]).ShouldNotBe(Build(["p", "em"], [".png"]));

    [Fact]
    public void ChangedImageTypesMakeSettingsDifferent() =>
        Build(["p"], [".png"]).ShouldNotBe(Build(["p"], [".png", ".gif"]));

    [Fact]
    public void ChangedLayoutMakesSettingsDifferent() =>
        new EditorSettings { ToolbarLayout = ToolbarLayout.Simple }.ShouldNotBe(new EditorSettings());

    [Fact]
    public void PolicySetsCompareCaseInsensitivelyLikeTheirComparer() =>
        new HtmlPolicy { AllowedTags = new HashSet<string>(["P"], StringComparer.OrdinalIgnoreCase) }
            .ShouldBe(new HtmlPolicy { AllowedTags = new HashSet<string>(["p"], StringComparer.OrdinalIgnoreCase) });

    [Fact]
    public void EmbedProvidersCompareByContent()
    {
        var custom = new EmbedProvider
        {
            Name = "example",
            DisplayName = "Example",
            UrlPatterns = [@"^https://video.example.com/v/(?<id>d+)$"],
            EmbedUrlTemplate = "https://video.example.com/embed/{id}"
        };

        new EditorSettings { EmbedProviders = [EmbedProvider.YouTube, EmbedProvider.Vimeo] }.ShouldBe(new EditorSettings());
        new EditorSettings { EmbedProviders = [custom] }.ShouldBe(new EditorSettings { EmbedProviders = [custom with { }] });
        new EditorSettings { EmbedProviders = [EmbedProvider.Vimeo] }.ShouldNotBe(new EditorSettings());
    }

    [Fact]
    public void EmbedProviderNamesMustBeUnique() =>
        Should.Throw<ArgumentException>(() => new EditorSettings { EmbedProviders = [EmbedProvider.YouTube, EmbedProvider.YouTube with { DisplayName = "Tube" }] });

    [Fact]
    public void EmbedProvidersAreCopiedAndMustNotContainNulls()
    {
        List<EmbedProvider> providers = [EmbedProvider.Vimeo];
        var settings = new EditorSettings { EmbedProviders = providers };
        providers.Add(EmbedProvider.YouTube);

        settings.EmbedProviders.ShouldBe([EmbedProvider.Vimeo]);
        Should.Throw<ArgumentException>(() => new EditorSettings { EmbedProviders = [null!] });
    }

    private static EditorSettings Build(string[] tags, string[] imageTypes) => new()
    {
        ToolbarLayout = new ToolbarLayout(new ToolbarGroup("bold", "italic"), new ToolbarSeparator()),
        Dimensions = new() { Width = "100%" },
        HtmlPolicy = new() { AllowedTags = new HashSet<string>(tags), AllowDataUrls = false },
        ImageConstraints = new() { AllowedImageTypes = new HashSet<string>(imageTypes), MaxWidth = 800 },
    };
}
