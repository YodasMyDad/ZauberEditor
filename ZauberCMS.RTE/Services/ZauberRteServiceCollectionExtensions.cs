using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using ZauberCMS.RTE.Interop;

namespace ZauberCMS.RTE.Services;

/// <summary>Registers the rich-text editor's services.</summary>
public static class ZauberRteServiceCollectionExtensions
{
    /// <summary>Adds the editor's services; safe to call more than once.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Registers custom toolbar items.</param>
    public static IServiceCollection AddZauberRte(this IServiceCollection services, Action<ZauberRteOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddOptions<ZauberRteOptions>();
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton(static provider => new ToolbarRegistry(provider.GetRequiredService<IOptions<ZauberRteOptions>>()));
        services.TryAddScoped<IEditorInteropFactory>(static provider => new EditorInteropFactory(
            provider.GetRequiredService<IJSRuntime>(),
            provider.GetRequiredService<ILogger<EditorInterop>>()));
        return services;
    }
}
