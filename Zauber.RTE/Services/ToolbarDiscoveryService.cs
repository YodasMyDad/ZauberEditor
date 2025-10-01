using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Zauber.RTE.Models;

namespace Zauber.RTE.Services;

/// <summary>
/// Service for discovering and managing toolbar items from assemblies
/// </summary>
public class ToolbarDiscoveryService(ILogger<ToolbarDiscoveryService> logger)
{
    private readonly Dictionary<string, IToolbarItem> _toolbarItems = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<Assembly> _scannedAssemblies = new();

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
                var toolbarItemTypes = assembly.GetTypes()
                    .Where(t => typeof(IToolbarItem).IsAssignableFrom(t) &&
                               !t.IsAbstract &&
                               !t.IsInterface &&
                               t.GetConstructor(Type.EmptyTypes) != null)
                    .ToList();

                foreach (var type in toolbarItemTypes)
                {
                    try
                    {
                        var instance = (IToolbarItem)Activator.CreateInstance(type)!;
                        if (_toolbarItems.ContainsKey(instance.Id))
                        {
                            logger.LogWarning("Toolbar item with ID '{Id}' already exists. Skipping duplicate from {Type}",
                                instance.Id, type.FullName);
                            continue;
                        }

                        _toolbarItems[instance.Id] = instance;
                        logger.LogInformation("Registered toolbar item '{Id}' from {Type}", instance.Id, type.FullName);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to instantiate toolbar item {Type}", type.FullName);
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
    /// Gets all discovered toolbar items
    /// </summary>
    public IReadOnlyDictionary<string, IToolbarItem> GetAllItems() => _toolbarItems;

    /// <summary>
    /// Gets a toolbar item by ID
    /// </summary>
    public IToolbarItem? GetItem(string id) =>
        _toolbarItems.TryGetValue(id, out var item) ? item : null;

    /// <summary>
    /// Gets toolbar items by placement category
    /// </summary>
    public IEnumerable<IToolbarItem> GetItemsByPlacement(ToolbarPlacement placement) =>
        _toolbarItems.Values.Where(item => item.Placement == placement);

    /// <summary>
    /// Gets toolbar items filtered by the provided predicate
    /// </summary>
    public IEnumerable<IToolbarItem> GetItems(Func<IToolbarItem, bool> predicate) =>
        _toolbarItems.Values.Where(predicate);

    /// <summary>
    /// Checks if a toolbar item with the specified ID exists
    /// </summary>
    public bool HasItem(string id) => _toolbarItems.ContainsKey(id);

    /// <summary>
    /// Registers a toolbar item manually
    /// </summary>
    public void RegisterItem(IToolbarItem item)
    {
        if (_toolbarItems.ContainsKey(item.Id))
        {
            logger.LogWarning("Toolbar item with ID '{Id}' already exists. Skipping duplicate registration", item.Id);
            return;
        }

        _toolbarItems[item.Id] = item;
        logger.LogInformation("Manually registered toolbar item '{Id}'", item.Id);
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
    /// Adds Zauber RTE services to the service collection
    /// </summary>
    public static IServiceCollection AddZauberRte(this IServiceCollection services, Action<ZauberRteOptions>? configure = null)
    {
        var options = new ZauberRteOptions();
        configure?.Invoke(options);

        // Register the discovery service with initialization
        services.AddSingleton(provider =>
        {
            var discoveryService = new ToolbarDiscoveryService(provider.GetRequiredService<ILogger<ToolbarDiscoveryService>>());
            
            if (options.AllowOverrides)
            {
                // Scan user assemblies first, then add defaults only if not present
                // This allows users to override built-in toolbar items
                discoveryService.ScanAssemblies(options.Assemblies);
                
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
                discoveryService.ScanAssemblies(options.Assemblies);
            }

            return discoveryService;
        });

        // Register the JS runtime
        services.AddScoped<IZauberJsRuntime, ZauberJsRuntime>();

        return services;
    }
}
