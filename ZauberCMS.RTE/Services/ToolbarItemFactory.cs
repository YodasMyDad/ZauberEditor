using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Services;

/// <summary>
/// Factory service for creating toolbar item instances in the appropriate scope
/// </summary>
public class ToolbarItemFactory(
    ToolbarDiscoveryService discoveryService,
    IServiceProvider serviceProvider,
    ILogger<ToolbarItemFactory> logger)
{
    private readonly Dictionary<string, IToolbarItem> _scopedCache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a toolbar item by ID, creating it if necessary in the current scope
    /// </summary>
    public IToolbarItem? GetItem(string id)
    {
        // Check scoped cache first (for custom items with dependencies)
        if (_scopedCache.TryGetValue(id, out var cachedItem))
        {
            return cachedItem;
        }

        var metadata = discoveryService.GetItemMetadata(id);
        if (metadata == null)
        {
            return null;
        }

        // If there's a cached instance (from default items), use it directly without adding to scoped cache
        // This is a shared singleton instance that doesn't need per-scope caching
        if (metadata.CachedInstance != null)
        {
            return metadata.CachedInstance;
        }

        // Create instance in current scope (for custom items with scoped dependencies)
        try
        {
            var instance = (IToolbarItem)ActivatorUtilities.CreateInstance(serviceProvider, metadata.Type);
            _scopedCache[id] = instance;
            return instance;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to instantiate toolbar item {Type} with ID {Id}", metadata.Type.FullName, id);
            return null;
        }
    }

    /// <summary>
    /// Gets all toolbar items for the specified IDs
    /// </summary>
    public List<IToolbarItem> GetItems(IEnumerable<string> ids)
    {
        var items = new List<IToolbarItem>();
        foreach (var id in ids)
        {
            var item = GetItem(id);
            if (item != null)
            {
                items.Add(item);
            }
        }
        return items;
    }

    /// <summary>
    /// Gets all discovered toolbar items
    /// </summary>
    public List<IToolbarItem> GetAllItems()
    {
        var metadata = discoveryService.GetAllItemMetadata();
        var items = new List<IToolbarItem>();
        
        foreach (var meta in metadata.Values)
        {
            var item = GetItem(meta.Id);
            if (item != null)
            {
                items.Add(item);
            }
        }
        
        return items;
    }

    /// <summary>
    /// Gets toolbar items by placement category
    /// </summary>
    public List<IToolbarItem> GetItemsByPlacement(ToolbarPlacement placement)
    {
        return GetAllItems().Where(item => item.Placement == placement).ToList();
    }
}

