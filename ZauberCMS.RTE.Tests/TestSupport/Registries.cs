using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ZauberCMS.RTE.Models;
using ZauberCMS.RTE.Services;

namespace ZauberCMS.RTE.Tests.TestSupport;

internal static class Registries
{
    private static readonly IServiceProvider NoServices = new ServiceCollection().BuildServiceProvider();

    public static ToolbarRegistry Create(Action<ZauberRteOptions>? configure = null, ILogger<ToolbarRegistry>? logger = null)
    {
        var options = new ZauberRteOptions();
        configure?.Invoke(options);
        return new(Options.Create(options), logger ?? NullLogger<ToolbarRegistry>.Instance);
    }

    public static IReadOnlyDictionary<string, IToolbarItem> Items(Action<ZauberRteOptions>? configure = null) =>
        Create(configure).CreateItemSet(NoServices).Items;
}
