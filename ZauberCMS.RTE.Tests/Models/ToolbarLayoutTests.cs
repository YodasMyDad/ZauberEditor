using System.Text.Json;
using ZauberCMS.RTE.Models;
using ZauberCMS.RTE.Services;
using ZauberCMS.RTE.Tests.TestSupport;

namespace ZauberCMS.RTE.Tests.Models;

public sealed class ToolbarLayoutTests
{
    [Fact]
    public void FullLayoutIncludesEveryBuiltInExceptTheDropdownChildren()
    {
        var arrangement = ToolbarArrangement.Create(ToolbarLayout.Full, Registries.Items());

        arrangement.MissingIds.ShouldBeEmpty();
        arrangement.Slots.Select(slot => slot.Item.Id).ShouldBe(
        [
            "bold", "italic", "underline", "strike", "code", "subscript", "superscript",
            "headings", "blockquote",
            "ul", "ol", "link", "unlink", "image", "table",
            "alignLeft", "alignCenter", "alignRight", "justified",
            "clean-html", "undo", "redo", "viewSource", "themeToggle"
        ]);
        arrangement.Items.Select(item => item.Id).ShouldBe(Registries.Items().Keys, ignoreOrder: true);
    }

    [Fact]
    public void SimpleLayoutUsesOnlyRegisteredItems()
    {
        var arrangement = ToolbarArrangement.Create(ToolbarLayout.Simple, Registries.Items());

        arrangement.MissingIds.ShouldBeEmpty();
        arrangement.Slots.Select(slot => slot.Item.Id).ShouldBe(["bold", "italic", "underline", "clean-html", "alignLeft", "alignCenter", "alignRight"]);
        arrangement.Segments.Count(segment => segment.IsSeparator).ShouldBe(2);
    }

    [Fact]
    public void JsonRoundTripKeepsGroupsSeparatorsAndItemReferences()
    {
        var layout = new ToolbarLayout(
            new ToolbarGroup("bold", "italic") { CssClass = "marks" },
            new ToolbarSeparator(),
            new ToolbarItemRef("link"));

        var copy = ToolbarLayout.FromJson(layout.ToJson());

        copy.Entries.Count.ShouldBe(3);
        var group = copy.Entries[0].ShouldBeOfType<ToolbarGroup>();
        group.ItemIds.ShouldBe(["bold", "italic"]);
        group.CssClass.ShouldBe("marks");
        copy.Entries[1].ShouldBeOfType<ToolbarSeparator>().CssClass.ShouldBeNull();
        copy.Entries[2].ShouldBeOfType<ToolbarItemRef>().ItemId.ShouldBe("link");
        copy.ToJson().ShouldBe(layout.ToJson());
    }

    [Fact]
    public void PresetsSurviveARoundTrip()
    {
        ToolbarLayout.FromJson(ToolbarLayout.Full.ToJson()).ToJson().ShouldBe(ToolbarLayout.Full.ToJson());
        ToolbarLayout.FromJson(ToolbarLayout.Simple.ToJson()).ToJson().ShouldBe(ToolbarLayout.Simple.ToJson());
    }

    [Fact]
    public void JsonUsesCamelCaseAndTypeDiscriminators()
    {
        var json = new ToolbarLayout(new ToolbarGroup("bold"), new ToolbarSeparator(), new ToolbarItemRef("link")).ToJson();

        using var document = JsonDocument.Parse(json);
        var entries = document.RootElement.GetProperty("entries");
        entries[0].GetProperty("$type").GetString().ShouldBe("group");
        entries[0].GetProperty("itemIds")[0].GetString().ShouldBe("bold");
        entries[1].GetProperty("$type").GetString().ShouldBe("separator");
        entries[2].GetProperty("$type").GetString().ShouldBe("item");
        entries[2].GetProperty("itemId").GetString().ShouldBe("link");
    }

    [Fact]
    public void HandWrittenJsonIsRead()
    {
        const string Json = """
            {
              "entries": [
                { "$type": "group", "itemIds": ["bold", "headings"], "cssClass": "first" },
                { "$type": "separator" },
                { "$type": "item", "itemId": "undo" }
              ]
            }
            """;

        var arrangement = ToolbarArrangement.Create(ToolbarLayout.FromJson(Json), Registries.Items());

        arrangement.Slots.Select(slot => slot.Item.Id).ShouldBe(["bold", "headings", "undo"]);
        arrangement.Segments[0].CssClass.ShouldBe("rte-toolbar-block first");
    }

    [Theory]
    [InlineData("null")]
    [InlineData("{\"entries\":[{\"$type\":\"unknown\"}]}")]
    [InlineData("not json")]
    [InlineData("{}")]
    [InlineData("{\"entries\":null}")]
    [InlineData("{\"entries\":[null]}")]
    [InlineData("{\"entries\":[{\"$type\":\"group\"}]}")]
    [InlineData("{\"entries\":[{\"$type\":\"group\",\"itemIds\":[\"bold\",null]}]}")]
    [InlineData("{\"entries\":[{\"$type\":\"item\"}]}")]
    public void InvalidJsonIsRejected(string json) =>
        Should.Throw<JsonException>(() => ToolbarLayout.FromJson(json));

    [Fact]
    public void LayoutsCannotBeBuiltWithMissingParts()
    {
        Should.Throw<ArgumentNullException>(() => new ToolbarLayout(null!));
        Should.Throw<ArgumentException>(() => new ToolbarLayout(new ToolbarGroup("bold"), null!));
        Should.Throw<ArgumentNullException>(() => new ToolbarGroup(null!));
        Should.Throw<ArgumentException>(() => new ToolbarGroup("bold", null!));
        Should.Throw<ArgumentNullException>(() => new ToolbarItemRef(null!));
        Should.Throw<ArgumentNullException>(() => new ToolbarGroup("bold") with { ItemIds = null! });
        Should.Throw<ArgumentNullException>(() => ToolbarLayout.Simple with { Entries = null! });
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void EmptyJsonIsRejected(string json) =>
        Should.Throw<ArgumentException>(() => ToolbarLayout.FromJson(json));

    [Fact]
    public void LayoutsWithTheSameEntriesAreEqual()
    {
        var first = new ToolbarLayout(new ToolbarGroup("bold", "italic") { CssClass = "x" }, new ToolbarSeparator(), new ToolbarItemRef("link"));
        var second = new ToolbarLayout([new ToolbarGroup(["bold", "italic"]) { CssClass = "x" }, new ToolbarSeparator(), new ToolbarItemRef("link")]);

        first.ShouldBe(second);
        (first == second).ShouldBeTrue();
        first.GetHashCode().ShouldBe(second.GetHashCode());
        ToolbarLayout.FromJson(ToolbarLayout.Full.ToJson()).ShouldBe(ToolbarLayout.Full);
    }

    [Theory]
    [MemberData(nameof(ChangedLayouts))]
    public void LayoutsWithDifferentEntriesAreNotEqual(ToolbarLayout changed) =>
        changed.ShouldNotBe(new ToolbarLayout(new ToolbarGroup("bold", "italic"), new ToolbarSeparator()));

    public static TheoryData<ToolbarLayout> ChangedLayouts() =>
    [
        new ToolbarLayout(new ToolbarGroup("italic", "bold"), new ToolbarSeparator()),
        new ToolbarLayout(new ToolbarGroup("bold", "italic") { CssClass = "x" }, new ToolbarSeparator()),
        new ToolbarLayout(new ToolbarGroup("bold", "italic")),
        new ToolbarLayout(new ToolbarGroup("bold"), new ToolbarItemRef("italic"), new ToolbarSeparator()),
        new ToolbarLayout(new ToolbarGroup("bold", "italic"), new ToolbarItemRef("link")),
    ];
}
