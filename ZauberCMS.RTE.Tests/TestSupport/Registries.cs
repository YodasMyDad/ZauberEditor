using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ZauberCMS.RTE.Models;
using ZauberCMS.RTE.Services;

namespace ZauberCMS.RTE.Tests.TestSupport;

internal static class Registries
{
    private static readonly IServiceProvider NoServices = new ServiceCollection().BuildServiceProvider();

    public static ToolbarRegistry Create(Action<ZauberRteOptions>? configure = null)
    {
        var options = new ZauberRteOptions();
        configure?.Invoke(options);
        return new(Options.Create(options));
    }

    public static IReadOnlyDictionary<string, IToolbarItem> Items(Action<ZauberRteOptions>? configure = null) =>
        Create(configure).CreateItemSet(NoServices).Items;
}
