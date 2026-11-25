using Microsoft.Extensions.DependencyInjection;
using ZauberCMS.RTE.Models;
using ZauberCMS.RTE.Services;
using ZauberCMS.RTE.Tests.TestSupport;
using ZauberCMS.RTE.Toolbar;

namespace ZauberCMS.RTE.Tests.Services;

public sealed class ToolbarRegistryTests
{
    [Fact]
    public void EveryBuiltInIsRegisteredIncludingTable()
    {
        var items = Registries.Items();

        items.Keys.ShouldBe(BuiltInToolbarItems.All.Select(item => item.Id), ignoreOrder: true);
        items.ShouldContainKey(ToolbarItemIds.Table);
    }

    [Fact]
    public void LookupIgnoresCase()
    {
        var items = Registries.Items();

        items["BOLD"].ShouldBeSameAs(items["bold"]);
        items["AlignLeft"].Id.ShouldBe(ToolbarItemIds.AlignLeft);
    }

    [Fact]
    public void LaterRegistrationWithTheSameIdReplacesTheBuiltIn()
    {
        var replacement = new ShoutingBoldItem();

        var items = Registries.Items(options => options.AddToolbarItem(replacement));

        items["bold"].ShouldBeSameAs(replacement);
        items.Count.ShouldBe(BuiltInToolbarItems.All.Count);
    }

    [Fact]
    public void LastOfSeveralRegistrationsWins()
    {
        var first = new RecordingItem();
        var second = new RecordingItem();

        var items = Registries.Items(options => options.AddToolbarItem(first).AddToolbarItem(second));

        items[RecordingItem.ItemId].ShouldBeSameAs(second);
    }

    [Fact]
    public void RemoveToolbarItemRemovesABuiltInIgnoringCase()
    {
        var items = Registries.Items(options => options.RemoveToolbarItem("TABLE"));

        items.ShouldNotContainKey(ToolbarItemIds.Table);
        items.Count.ShouldBe(BuiltInToolbarItems.All.Count - 1);
    }

    [Fact]
    public void RegistrationsApplyInOrder()
    {
        var replacement = new ShoutingBoldItem();

        Registries.Items(options => options.RemoveToolbarItem("bold").AddToolbarItem(replacement))["bold"].ShouldBeSameAs(replacement);
        Registries.Items(options => options.AddToolbarItem(replacement).RemoveToolbarItem("bold")).ShouldNotContainKey("bold");
    }

    [Fact]
    public void InvalidRegistrationsAreRejected()
    {
        var options = new ZauberRteOptions();

        Should.Throw<ArgumentNullException>(() => options.AddToolbarItem(null!));
        Should.Throw<ArgumentException>(() => options.RemoveToolbarItem(" "));
        Should.Throw<ArgumentNullException>(() => options.AddToolbarItemsFromAssembly(null!));
        Should.Throw<ArgumentNullException>(() => Registries.Create().CreateItemSet(null!));
    }

    [Fact]
    public async Task TypedItemsAreCreatedPerEditorWithScopedDependencies()
    {
        var services = new ServiceCollection();
        services.AddScoped<IGreeter, Greeter>();
        services.AddZauberRte(options => options.AddToolbarItem<GreetingItem>());
        await using var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<ToolbarRegistry>();
        await using var firstScope = provider.CreateAsyncScope();
        await using var secondScope = provider.CreateAsyncScope();

        await using var first = registry.CreateItemSet(firstScope.ServiceProvider);
        await using var second = registry.CreateItemSet(firstScope.ServiceProvider);
        await using var third = registry.CreateItemSet(secondScope.ServiceProvider);

        var firstItem = first.Items[GreetingItem.ItemId].ShouldBeOfType<GreetingItem>();
        var secondItem = second.Items[GreetingItem.ItemId].ShouldBeOfType<GreetingItem>();
        var thirdItem = third.Items[GreetingItem.ItemId].ShouldBeOfType<GreetingItem>();
        firstItem.ShouldNotBeSameAs(secondItem);
        firstItem.Label.ShouldBe("Hello");
        firstItem.Greeter.ShouldBeSameAs(firstScope.ServiceProvider.GetRequiredService<IGreeter>());
        secondItem.Greeter.ShouldBeSameAs(firstItem.Greeter);
        thirdItem.Greeter.ShouldNotBeSameAs(firstItem.Greeter);
    }

    [Fact]
    public async Task ItemSetDisposesOnlyTheItemsItCreated()
    {
        var shared = new DisposableItem();
        var services = new ServiceCollection().BuildServiceProvider();
        var registry = Registries.Create(options => options.AddToolbarItem<DisposableItem>());
        var sharedRegistry = Registries.Create(options => options.AddToolbarItem(shared));

        var items = registry.CreateItemSet(services);
        var created = items.Items[DisposableItem.ItemId].ShouldBeOfType<DisposableItem>();
        await items.DisposeAsync();
        await sharedRegistry.CreateItemSet(services).DisposeAsync();

        created.IsDisposed.ShouldBeTrue();
        shared.IsDisposed.ShouldBeFalse();
    }

    [Fact]
    public void AssemblyScanningIsOptIn()
    {
        var services = new ServiceCollection().AddScoped<IGreeter, Greeter>().BuildServiceProvider();

        var withoutScanning = Registries.Create().CreateItemSet(services).Items;
        var withScanning = Registries.Create(options => options.AddToolbarItemsFromAssembly(typeof(GreetingItem).Assembly))
            .CreateItemSet(services)
            .Items;

        withoutScanning.ShouldNotContainKey(GreetingItem.ItemId);
        withoutScanning.ShouldNotContainKey(RecordingItem.ItemId);
        withScanning[GreetingItem.ItemId].ShouldBeOfType<GreetingItem>();
        withScanning[RecordingItem.ItemId].ShouldBeOfType<RecordingItem>();
        withScanning[FailingItem.ItemId].ShouldBeOfType<FailingItem>();
        withScanning["bold"].ShouldBeOfType<ShoutingBoldItem>();
    }
}
