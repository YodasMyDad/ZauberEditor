using System.Reflection;
using ZauberCMS.RTE.Models;
using ZauberCMS.RTE.Toolbar;

namespace ZauberCMS.RTE.Tests.Toolbar;

public sealed class BuiltInToolbarItemsTests
{
    private static readonly string[] PublishedIds =
    [
        .. typeof(ToolbarItemIds)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral)
            .Select(field => (string)field.GetRawConstantValue()!)
    ];

    [Fact]
    public void EveryPublishedIdHasExactlyOneBuiltInItem() =>
        BuiltInToolbarItems.All.Select(item => item.Id).ShouldBe(PublishedIds, ignoreOrder: true);

    [Fact]
    public void IdsAreUniqueIgnoringCase() =>
        BuiltInToolbarItems.All.Select(item => item.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count().ShouldBe(BuiltInToolbarItems.All.Count);

    [Fact]
    public void EveryItemHasALabel() =>
        BuiltInToolbarItems.All.ShouldAllBe(item => !string.IsNullOrWhiteSpace(item.Label));

    [Fact]
    public void ButtonIconsAreSvgMarkup() =>
        BuiltInToolbarItems.All
            .Where(item => item.IconSvg is not null)
            .ShouldAllBe(item => item.IconSvg!.StartsWith("<svg", StringComparison.Ordinal) && item.IconSvg.EndsWith("</svg>", StringComparison.Ordinal));

    [Fact]
    public void EveryShortcutParses() =>
        BuiltInToolbarItems.All
            .Where(item => item.Shortcut is not null && !KeyboardShortcut.TryParse(item.Shortcut, out _))
            .ShouldBeEmpty();

    [Fact]
    public void HeadingsDropdownListsTheSixHeadings()
    {
        var headings = BuiltInToolbarItems.All.Single(item => item.Id == ToolbarItemIds.Headings);

        headings.Kind.ShouldBe(ToolbarItemKind.Dropdown);
        headings.ChildItemIds.ShouldBe(["h1", "h2", "h3", "h4", "h5", "h6"]);
    }

    [Theory]
    [InlineData(ToolbarItemIds.Bold, "strong", new[] { "strong", "b" })]
    [InlineData(ToolbarItemIds.Italic, "em", new[] { "em", "i" })]
    [InlineData(ToolbarItemIds.Underline, "u", new[] { "u" })]
    [InlineData(ToolbarItemIds.Strike, "s", new[] { "s", "strike" })]
    [InlineData(ToolbarItemIds.Subscript, "sub", new[] { "sub" })]
    [InlineData(ToolbarItemIds.Superscript, "sup", new[] { "sup" })]
    [InlineData(ToolbarItemIds.Code, "code", new[] { "code" })]
    public void MarkItemsTrackTheirTagsAndEmitTheCanonicalOne(string id, string primaryTag, string[] trackedTags)
    {
        var item = BuiltInToolbarItems.All.Single(candidate => candidate.Id == id);

        item.PrimaryTag.ShouldBe(primaryTag);
        item.TrackedTags.ShouldBe(trackedTags);
        item.IsToggle.ShouldBeTrue();
    }
}
