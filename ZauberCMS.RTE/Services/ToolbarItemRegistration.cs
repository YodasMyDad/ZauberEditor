using Microsoft.Extensions.Logging;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Services;

internal sealed record ToolbarItemRegistration(
    IToolbarItem? Instance,
    Type? ItemType,
    Func<IServiceProvider, IToolbarItem>? Factory,
    string? RemovedId)
{
    public static ToolbarItemRegistration ForInstance(IToolbarItem item) => new(item, null, null, null);

    public static ToolbarItemRegistration ForFactory(Type itemType, Func<IServiceProvider, IToolbarItem> factory) =>
        new(null, itemType, factory, null);

    public static ToolbarItemRegistration ForRemoval(string id) => new(null, null, null, id);

    public void ApplyTo(Dictionary<string, IToolbarItem> items, List<IToolbarItem> created, IServiceProvider services, ILogger logger)
    {
        switch (this)
        {
            case { Instance: { } instance }:
                items[instance.Id] = instance;
                break;
            case { ItemType: { } itemType, Factory: { } factory }:
                if (Create(itemType, factory, services, logger) is { } item)
                {
                    created.Add(item);
                    items[item.Id] = item;
                }

                break;
            case { RemovedId: { } id }:
                items.Remove(id);
                break;
        }
    }

    private static IToolbarItem? Create(Type itemType, Func<IServiceProvider, IToolbarItem> factory, IServiceProvider services, ILogger logger)
    {
        try
        {
            return factory(services);
        }
        catch (InvalidOperationException exception)
        {
            logger.ItemNotCreated(itemType.FullName ?? itemType.Name, exception);
            return null;
        }
    }
}
