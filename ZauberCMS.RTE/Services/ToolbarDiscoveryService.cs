using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Services;

/// <summary>
/// Metadata about a discovered toolbar item type
/// </summary>
public class ToolbarItemMetadata
{
    public required string Id { get; init; }
    public required Type Type { get; init; }
    public IToolbarItem? CachedInstance { get; set; }
}

/// <summary>
/// Service for discovering and managing toolbar items from assemblies
/// </summary>
public class ToolbarDiscoveryService(ILogger<ToolbarDiscoveryService> logger, IServiceProvider serviceProvider)
{
    private readonly Dictionary<string, ToolbarItemMetadata> _toolbarItems = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<Assembly> _scannedAssemblies = [];

    /// <summary>
    /// Scans the specified assemblies for IToolbarItem implementations
    /// </summary>
    public void ScanAssemblies(IEnumerable<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            if (_scannedAssemblies.Contains(assembly))
            {
                continue;
            }

            try
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types!.Where(t => t != null).ToArray()!;
                }

                var toolbarItemTypes = types
                    .Where(t => t != null &&
                               typeof(IToolbarItem).IsAssignableFrom(t) &&
                               !t.IsAbstract &&
                               !t.IsInterface)
                    .ToList();

                foreach (var type in toolbarItemTypes)
                {
                    try
                    {
                        // Create a temporary instance in a scope to get the ID
                        var tempInstance = CreateTemporaryInstance(type);
                        if (tempInstance == null)
                        {
                            logger.LogWarning("Toolbar item {Type} could not be instantiated and will be skipped", type.FullName);
                            continue;
                        }

                        var id = tempInstance.Id;
                        if (_toolbarItems.ContainsKey(id))
                        {
                            logger.LogWarning("Toolbar item with ID '{Id}' already exists. Skipping duplicate from {Type}",
                                id, type.FullName);
                            continue;
                        }

                        _toolbarItems[id] = new ToolbarItemMetadata
                        {
                            Id = id,
                            Type = type,
                            CachedInstance = null // Will be instantiated per-scope by factory
                        };
                        
                        logger.LogInformation("Registered toolbar item '{Id}' from {Type}", id, type.FullName);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to register toolbar item {Type}", type.FullName);
                    }
                }

                _scannedAssemblies.Add(assembly);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to scan assembly {Assembly}", assembly.FullName);
            }
        }
    }

    /// <summary>
    /// Creates a temporary instance in a scope just to read metadata (like ID)
    /// The instance is discarded after reading the ID
    /// </summary>
    private IToolbarItem? CreateTemporaryInstance(Type type)
    {
        try
        {
            // Create a scope to safely resolve scoped services
            using var scope = serviceProvider.CreateScope();
            return (IToolbarItem)ActivatorUtilities.CreateInstance(scope.ServiceProvider, type);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create temporary instance of {Type}", type.FullName);
            return null;
        }
    }

    /// <summary>
    /// Gets all discovered toolbar item metadata
    /// </summary>
    public IReadOnlyDictionary<string, ToolbarItemMetadata> GetAllItemMetadata() => _toolbarItems;

    /// <summary>
    /// Gets toolbar item metadata by ID
    /// </summary>
    public ToolbarItemMetadata? GetItemMetadata(string id) =>
        _toolbarItems.TryGetValue(id, out var metadata) ? metadata : null;

    /// <summary>
    /// Checks if a toolbar item with the specified ID exists
    /// </summary>
    public bool HasItem(string id) => _toolbarItems.ContainsKey(id);

    /// <summary>
    /// Registers a toolbar item instance manually (for default items or pre-created instances)
    /// </summary>
    public void RegisterItem(IToolbarItem item)
    {
        if (_toolbarItems.ContainsKey(item.Id))
        {
            logger.LogWarning("Toolbar item with ID '{Id}' already exists. Skipping duplicate registration", item.Id);
            return;
        }

        _toolbarItems[item.Id] = new ToolbarItemMetadata
        {
            Id = item.Id,
            Type = item.GetType(),
            CachedInstance = item // Pre-created instance
        };
        
        logger.LogInformation("Manually registered toolbar item '{Id}'", item.Id);
    }

    /// <summary>
    /// Registers toolbar item metadata manually
    /// </summary>
    public void RegisterMetadata(ToolbarItemMetadata metadata)
    {
        if (_toolbarItems.ContainsKey(metadata.Id))
        {
            logger.LogWarning("Toolbar item with ID '{Id}' already exists. Skipping duplicate registration", metadata.Id);
            return;
        }

        _toolbarItems[metadata.Id] = metadata;
        logger.LogInformation("Manually registered toolbar item metadata '{Id}'", metadata.Id);
    }
}

/// <summary>
/// Options for configuring Zauber RTE
/// </summary>
public class ZauberRteOptions
{
    /// <summary>
    /// Assemblies to scan for toolbar items
    /// </summary>
    public List<Assembly> Assemblies { get; set; } = [];

    /// <summary>
    /// When true, custom toolbar items from user assemblies can override built-in items with the same ID
    /// </summary>
    public bool AllowOverrides { get; set; } = true;
}

/// <summary>
/// Service collection extensions for Zauber RTE
/// </summary>
public static class ZauberRteServiceCollectionExtensions
{
    /// <summary>
    /// Adds Zauber RTE services to the service collection, scanning the entry assembly only.
    /// </summary>
    public static IServiceCollection AddZauberRte(this IServiceCollection services)
        => AddZauberRte(services, Array.Empty<Assembly>());

    /// <summary>
    /// Adds Zauber RTE services to the service collection, scanning the entry assembly and any additional assemblies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="additionalAssemblies">Additional assemblies to scan for toolbar items.</param>
    public static IServiceCollection AddZauberRte(this IServiceCollection services, params Assembly[] additionalAssemblies)
        => AddZauberRte(services, options => { }, additionalAssemblies);

    /// <summary>
    /// Adds Zauber RTE services to the service collection with advanced configuration options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configuration action for advanced options.</param>
    /// <param name="additionalAssemblies">Additional assemblies to scan for toolbar items.</param>
    public static IServiceCollection AddZauberRte(this IServiceCollection services, Action<ZauberRteOptions> configure, params Assembly[] additionalAssemblies)
    {
        var options = new ZauberRteOptions();
        configure?.Invoke(options);

        // Collect assemblies to scan
        var assembliesToScan = new HashSet<Assembly>();
        var entry = Assembly.GetEntryAssembly();
        if (entry != null) assembliesToScan.Add(entry);
        foreach (var assembly in additionalAssemblies.Where(a => a != null)) 
            assembliesToScan.Add(assembly);
        foreach (var assembly in options.Assemblies.Where(a => a != null)) 
            assembliesToScan.Add(assembly);

        // Register the discovery service with initialization
        services.AddSingleton(provider =>
        {
            var discoveryService = new ToolbarDiscoveryService(
                provider.GetRequiredService<ILogger<ToolbarDiscoveryService>>(),
                provider);
            
            if (options.AllowOverrides)
            {
                // Scan user assemblies first, then add defaults only if not present
                // This allows users to override built-in toolbar items
                discoveryService.ScanAssemblies(assembliesToScan);
                
                foreach (var item in DefaultToolbarItems.GetAllDefaultItems())
                {
                    if (!discoveryService.HasItem(item.Id))
                    {
                        discoveryService.RegisterItem(item);
                    }
                }
            }
            else
            {
                // Scan defaults first - user items with duplicate IDs will be skipped
                discoveryService.ScanAssemblies([typeof(ZauberRteServiceCollectionExtensions).Assembly]);
                discoveryService.ScanAssemblies(assembliesToScan);
            }

            return discoveryService;
        });

        // Register the factory as scoped - this allows toolbar items to use scoped services
        services.AddScoped<ToolbarItemFactory>();

        // Register the JS runtime
        services.AddScoped<IZauberJsRuntime, ZauberJsRuntime>();

        return services;
    }
}
