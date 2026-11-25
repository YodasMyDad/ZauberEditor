using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using ZauberCMS.RTE.Interop;
using ZauberCMS.RTE.Models;
using ZauberCMS.RTE.Services;
using ZauberCMS.RTE.Tests.TestSupport;

namespace ZauberCMS.RTE.Tests.Services;

public sealed class ZauberRteServiceCollectionExtensionsTests
{
    private readonly ServiceCollection _services = [];

    [Fact]
    public void RegistersTheRegistryAsSingletonAndTheScriptFactoryAsScoped()
    {
        _services.AddZauberRte();

        Single<ToolbarRegistry>().Lifetime.ShouldBe(ServiceLifetime.Singleton);
        Single<IEditorInteropFactory>().Lifetime.ShouldBe(ServiceLifetime.Scoped);
        Single<TimeProvider>().Lifetime.ShouldBe(ServiceLifetime.Singleton);
    }

    [Fact]
    public void CallingTwiceRegistersEachServiceOnce()
    {
        _services.AddZauberRte();
        var count = _services.Count;

        _services.AddZauberRte();

        Single<ToolbarRegistry>();
        Single<IEditorInteropFactory>();
        Single<TimeProvider>();
        _services.Count.ShouldBe(count);
    }

    [Fact]
    public void ExistingRegistrationsAreKept()
    {
        var time = new FakeTimeProvider();
        _services.AddSingleton<TimeProvider>(time);

        _services.AddZauberRte();

        using var provider = _services.BuildServiceProvider();
        provider.GetRequiredService<TimeProvider>().ShouldBeSameAs(time);
    }

    [Fact]
    public async Task ConfigurationFromEveryCallIsApplied()
    {
        _services.AddZauberRte(options => options.RemoveToolbarItem(ToolbarItemIds.Table));
        _services.AddZauberRte(options => options.AddToolbarItem(new RecordingItem()));
        _services.Configure<ZauberRteOptions>(options => options.RemoveToolbarItem(ToolbarItemIds.Undo));

        await using var provider = _services.BuildServiceProvider();
        await using var items = provider.GetRequiredService<ToolbarRegistry>().CreateItemSet(provider);

        items.Items.ShouldNotContainKey(ToolbarItemIds.Table);
        items.Items.ShouldNotContainKey(ToolbarItemIds.Undo);
        items.Items.ShouldContainKey(RecordingItem.ItemId);
    }

    [Fact]
    public void NullServicesAreRejected() =>
        Should.Throw<ArgumentNullException>(() => ZauberRteServiceCollectionExtensions.AddZauberRte(null!));

    private ServiceDescriptor Single<TService>() =>
        _services.Where(descriptor => descriptor.ServiceType == typeof(TService)).ShouldHaveSingleItem();
}
