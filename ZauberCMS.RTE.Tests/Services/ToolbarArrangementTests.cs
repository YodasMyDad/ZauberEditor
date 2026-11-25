using ZauberCMS.RTE.Models;
using ZauberCMS.RTE.Services;
using ZauberCMS.RTE.Tests.TestSupport;

namespace ZauberCMS.RTE.Tests.Services;

public sealed class ToolbarArrangementTests
{
    private readonly IReadOnlyDictionary<string, IToolbarItem> _items = Registries.Items();

    [Fact]
    public void EntriesBecomeSegmentsWithTheirClasses()
    {
        var layout = new ToolbarLayout(
            new ToolbarGroup("bold", "italic"),
            new ToolbarSeparator { CssClass = "wide" },
            new ToolbarItemRef("undo") { CssClass = "history" });

        var arrangement = ToolbarArrangement.Create(layout, _items);

        arrangement.Segments.Select(segment => (segment.Key, segment.CssClass, segment.IsSeparator)).ShouldBe(
        [
            (0, "rte-toolbar-block", false),
            (1, "rte-toolbar-separator wide", true),
            (2, "rte-toolbar-single-item history", false)
        ]);
        arrangement.Segments[0].Slots.Select(slot => slot.Item.Id).ShouldBe(["bold", "italic"]);
        arrangement.Segments[1].Slots.ShouldBeEmpty();
        arrangement.Slots.Select(slot => slot.Index).ShouldBe([0, 1, 2]);
    }

    [Fact]
    public void FindLooksUpArrangedItemsAndDropdownChildrenIgnoringCase()
    {
        var arrangement = ToolbarArrangement.Create(new ToolbarLayout(new ToolbarGroup("bold", "headings")), Registries.Items());

        arrangement.Find("BOLD").ShouldNotBeNull().Id.ShouldBe("bold");
        arrangement.Find("h2").ShouldNotBeNull().Id.ShouldBe("h2");
        arrangement.Find("italic").ShouldBeNull();
    }

    [Fact]
    public void UnknownIdsAreReportedAndSkipped()
    {
        var layout = new ToolbarLayout(new ToolbarGroup("bold", "missing", "italic"), new ToolbarGroup("gone"), new ToolbarItemRef("nothing"));

        var arrangement = ToolbarArrangement.Create(layout, _items);

        arrangement.MissingIds.ShouldBe(["missing", "gone", "nothing"]);
        arrangement.Segments.ShouldHaveSingleItem().Slots.Select(slot => slot.Item.Id).ShouldBe(["bold", "italic"]);
    }

    [Fact]
    public void DropdownChildrenAreResolvedAndIncludedInTheItems()
    {
        var arrangement = ToolbarArrangement.Create(new ToolbarLayout(new ToolbarGroup("headings")), _items);

        var slot = arrangement.Slots.ShouldHaveSingleItem();
        slot.Children.Select(child => child.Id).ShouldBe(["h1", "h2", "h3", "h4", "h5", "h6"]);
        arrangement.Items.Select(item => item.Id).ShouldBe(["headings", "h1", "h2", "h3", "h4", "h5", "h6"]);
    }

    [Fact]
    public void MissingDropdownChildrenAreReported()
    {
        var items = Registries.Items(options => options.RemoveToolbarItem("h5"));

        var arrangement = ToolbarArrangement.Create(new ToolbarLayout(new ToolbarGroup("headings")), items);

        arrangement.MissingIds.ShouldBe(["h5"]);
        arrangement.Slots[0].Children.Select(child => child.Id).ShouldBe(["h1", "h2", "h3", "h4", "h6"]);
    }

    [Fact]
    public void ItemsAppearingTwiceAreListedOnce()
    {
        var arrangement = ToolbarArrangement.Create(new ToolbarLayout(new ToolbarGroup("headings", "h2", "bold"), new ToolbarItemRef("bold")), _items);

        arrangement.Slots.Count.ShouldBe(4);
        arrangement.Items.Select(item => item.Id).ShouldBe(["headings", "h1", "h2", "h3", "h4", "h5", "h6", "bold"]);
    }

    [Fact]
    public void NullArgumentsAreRejected()
    {
        Should.Throw<ArgumentNullException>(() => ToolbarArrangement.Create(null!, _items));
        Should.Throw<ArgumentNullException>(() => ToolbarArrangement.Create(ToolbarLayout.Simple, null!));
    }
}
