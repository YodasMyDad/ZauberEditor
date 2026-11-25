using Microsoft.Extensions.DependencyInjection;
using ZauberCMS.RTE.Models;
using ZauberCMS.RTE.Services;
using ZauberCMS.RTE.Tests.TestSupport;
using ZauberCMS.RTE.Toolbar;

namespace ZauberCMS.RTE.Tests.Services;

public sealed class ToolbarItemCatalogTests
{
    [Fact]
    public async Task ListsEveryBuiltInInTheirUsualOrder()
    {
        await using var scope = Scope();

        var items = Catalog(scope).GetItems();

        items.Select(item => item.Id).ShouldBe(BuiltInToolbarItems.All.Select(item => item.Id));
        items.ShouldAllBe(item => item.IsBuiltIn);
    }

    [Fact]
    public async Task DescribesTheItem()
    {
        await using var scope = Scope();

        var items = Catalog(scope).GetItems();

        var bold = items.Single(item => item.Id == ToolbarItemIds.Bold);
        bold.Label.ShouldBe("Bold");
        bold.Shortcut.ShouldBe("Control+b");
        bold.IconSvg.ShouldNotBeNullOrEmpty();
        bold.Kind.ShouldBe(ToolbarItemKind.Button);
        var headings = items.Single(item => item.Id == ToolbarItemIds.Headings);
        headings.Kind.ShouldBe(ToolbarItemKind.Dropdown);
        headings.ChildItemIds.ShouldContain(ToolbarItemIds.Heading2);
    }

    [Fact]
    public async Task CustomItemsFollowTheBuiltInsAndOverridesAreNotBuiltIn()
    {
        await using var scope = Scope(options => options
            .AddToolbarItem(new ShoutingBoldItem())
            .AddToolbarItem<RecordingItem>()
            .AddToolbarItem<FailingItem>());

        var items = Catalog(scope).GetItems();

        var bold = items.Single(item => string.Equals(item.Id, ToolbarItemIds.Bold, StringComparison.OrdinalIgnoreCase));
        bold.Id.ShouldBe("BOLD");
        bold.IsBuiltIn.ShouldBeFalse();
        items.Select(item => item.Id).TakeLast(2).ShouldBe([FailingItem.ItemId, RecordingItem.ItemId]);
        items.Single(item => item.Id == RecordingItem.ItemId).IsBuiltIn.ShouldBeFalse();
        items.Single(item => item.Id == ToolbarItemIds.Italic).IsBuiltIn.ShouldBeTrue();
    }

    [Fact]
    public async Task RemovedItemsAreNotListed()
    {
        await using var scope = Scope(options => options.RemoveToolbarItem(ToolbarItemIds.Table));

        Catalog(scope).GetItems().ShouldNotContain(item => item.Id == ToolbarItemIds.Table);
    }

    [Fact]
    public async Task ItemsAreDescribedOncePerScopeAndDisposedWithIt()
    {
        var scope = Scope(options => options.AddToolbarItem<TrackedItem>());
        var catalog = Catalog(scope);

        var first = catalog.GetItems();

        catalog.GetItems().ShouldBeSameAs(first);
        Catalog(scope).ShouldBeSameAs(catalog);
        var created = scope.ServiceProvider.GetRequiredService<ItemTracker>().Created.ShouldHaveSingleItem();
        created.IsDisposed.ShouldBeFalse();
        await scope.DisposeAsync();
        created.IsDisposed.ShouldBeTrue();
    }

    private static IToolbarItemCatalog Catalog(AsyncServiceScope scope) => scope.ServiceProvider.GetRequiredService<IToolbarItemCatalog>();

    private static AsyncServiceScope Scope(Action<ZauberRteOptions>? configure = null) =>
        new ServiceCollection()
            .AddLogging()
            .AddScoped<ItemTracker>()
            .AddZauberRte(configure)
            .BuildServiceProvider()
            .CreateAsyncScope();
}
