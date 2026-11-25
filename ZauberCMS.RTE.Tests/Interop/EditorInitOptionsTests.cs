using ZauberCMS.RTE.Interop;
using ZauberCMS.RTE.Models;
using ZauberCMS.RTE.Services;
using ZauberCMS.RTE.Tests.TestSupport;

namespace ZauberCMS.RTE.Tests.Interop;

public sealed class EditorInitOptionsTests
{
    private static readonly IReadOnlyList<IToolbarItem> FullToolbar =
        ToolbarArrangement.Create(ToolbarLayout.Full, Registries.Items()).Items;

    [Fact]
    public void PassesContentAndModeThrough()
    {
        var options = EditorInitOptions.Create("<p>x</p>", readOnly: true, "Type here", new EditorSettings(), FullToolbar, []);

        options.Html.ShouldBe("<p>x</p>");
        options.ReadOnly.ShouldBeTrue();
        options.Placeholder.ShouldBe("Type here");
    }

    [Fact]
    public void TrackedTagsAreLowerCaseDistinctAndSorted()
    {
        var options = EditorInitOptions.Create(null, false, null, new EditorSettings(), FullToolbar, []);

        options.TrackedTags.ShouldBe(["b", "code", "em", "i", "s", "strike", "strong", "sub", "sup", "u"]);
    }

    [Fact]
    public void AliasesMapToThePrimaryTag()
    {
        var options = EditorInitOptions.Create(null, false, null, new EditorSettings(), FullToolbar, []);

        options.TagAliases.ShouldBe(
            new Dictionary<string, string> { ["b"] = "strong", ["i"] = "em", ["strike"] = "s" },
            ignoreOrder: true);
    }

    [Fact]
    public void ShortcutsComeFromTheToolbarItems()
    {
        var options = EditorInitOptions.Create(null, false, null, new EditorSettings(), FullToolbar, []);

        options.Shortcuts.ShouldBe(
            [
                new ShortcutBinding("bold", "b", true, false, false),
                new ShortcutBinding("italic", "i", true, false, false),
                new ShortcutBinding("underline", "u", true, false, false),
                new ShortcutBinding("ul", "8", true, true, false),
                new ShortcutBinding("ol", "7", true, true, false),
                new ShortcutBinding("link", "k", true, false, false),
                new ShortcutBinding("undo", "z", true, false, false),
                new ShortcutBinding("redo", "y", true, false, false),
                new ShortcutBinding("findReplace", "h", true, false, false)
            ],
            ignoreOrder: true);
    }

    [Fact]
    public void OnlyItemsOnTheToolbarContribute()
    {
        var simple = ToolbarArrangement.Create(ToolbarLayout.Simple, Registries.Items()).Items;

        var options = EditorInitOptions.Create(null, false, null, new EditorSettings(), simple, []);

        options.TrackedTags.ShouldBe(["b", "em", "i", "strong", "u"]);
        options.Shortcuts.Select(shortcut => shortcut.ItemId).ShouldBe(["bold", "italic", "underline"], ignoreOrder: true);
    }

    [Fact]
    public void SettingsAreTranslatedForTheScript()
    {
        var settings = new EditorSettings
        {
            HistoryLimit = 25,
            ChangeDelay = TimeSpan.FromSeconds(1.5),
            HtmlPolicy = new()
            {
                AllowedTags = new HashSet<string> { "P", "strong", "p" },
                AllowedAttributes = new HashSet<string> { "HREF", "class" },
                AllowDataUrls = false,
                AllowExternalImages = false
            },
            ImageConstraints = new()
            {
                MaintainAspectRatio = false,
                MaxWidth = 800,
                MaxHeight = 600,
                AllowBase64ImageUpload = true,
                MaxFileSizeBytes = 1024,
                AllowedImageTypes = new HashSet<string> { ".PNG", ".gif", ".png" }
            }
        };

        var options = EditorInitOptions.Create(null, false, null, settings, [], []);

        options.HistoryLimit.ShouldBe(25);
        options.ChangeDelay.ShouldBe(1500);
        options.InlineLimit.ShouldBe(EditorInitOptions.DefaultInlineLimit);
        options.MaxContentBytes.ShouldBe(settings.MaxContentBytes);
        options.Policy.AllowedTags.ShouldBe(["p", "strong"]);
        options.Policy.AllowedAttributes.ShouldBe(["class", "href"]);
        options.Policy.AllowDataUrls.ShouldBeFalse();
        options.Policy.AllowExternalImages.ShouldBeFalse();
        options.Policy.TagAttributes.ShouldBeEmpty();
        options.Image.MaintainAspectRatio.ShouldBeFalse();
        options.Image.MaxWidth.ShouldBe(800);
        options.Image.MaxHeight.ShouldBe(600);
        options.Image.AllowBase64Upload.ShouldBeTrue();
        options.Image.MaxFileSizeBytes.ShouldBe(1024);
        options.Image.AllowedTypes.ShouldBe([".gif", ".png"]);
        options.TrackedTags.ShouldBeEmpty();
        options.TagAliases.ShouldBeEmpty();
        options.Shortcuts.ShouldBeEmpty();
    }

    [Fact]
    public void TagAttributesAreLowerCasedAndSorted()
    {
        var settings = new EditorSettings
        {
            HtmlPolicy = new()
            {
                TagAttributes = new Dictionary<string, IReadOnlySet<string>>
                {
                    ["A"] = new HashSet<string> { "Target", "download" },
                    ["img"] = new HashSet<string> { "loading" }
                }
            }
        };

        var options = EditorInitOptions.Create(null, false, null, settings, [], []);

        options.Policy.TagAttributes.Keys.ShouldBe(["a", "img"], ignoreOrder: true);
        options.Policy.TagAttributes["a"].ShouldBe(["download", "target"]);
        options.Policy.TagAttributes["img"].ShouldBe(["loading"]);
    }

    [Fact]
    public void EditorShortcutsFollowTheItemShortcuts()
    {
        ShortcutBinding save = new("Ctrl+S", "s", true, false, false, Custom: true);

        var options = EditorInitOptions.Create(null, false, null, new EditorSettings(), FullToolbar, [save]);

        options.Shortcuts[^1].ShouldBe(save);
        options.Shortcuts.Count.ShouldBe(10);
    }

    [Fact]
    public void ImageDefaultsBlockEmbeddingFiles()
    {
        var options = EditorInitOptions.Create(null, false, null, new EditorSettings(), [], []);

        options.Image.AllowBase64Upload.ShouldBeFalse();
        options.Image.MaxFileSizeBytes.ShouldBe(5 * 1024 * 1024);
        options.Image.AllowedTypes.ShouldBe([".gif", ".jpeg", ".jpg", ".png", ".webp"]);
    }

    [Fact]
    public void NullArgumentsAreRejected()
    {
        Should.Throw<ArgumentNullException>(() => EditorInitOptions.Create(null, false, null, null!, [], []));
        Should.Throw<ArgumentNullException>(() => EditorInitOptions.Create(null, false, null, new EditorSettings(), null!, []));
        Should.Throw<ArgumentNullException>(() => EditorInitOptions.Create(null, false, null, new EditorSettings(), [], null!));
    }
}
