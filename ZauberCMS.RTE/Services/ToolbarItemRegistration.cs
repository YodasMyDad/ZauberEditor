using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Services;

internal sealed record ToolbarItemRegistration(
    IToolbarItem? Instance,
    Func<IServiceProvider, IToolbarItem>? Factory,
    string? RemovedId)
{
    public static ToolbarItemRegistration ForInstance(IToolbarItem item) => new(item, null, null);

    public static ToolbarItemRegistration ForFactory(Func<IServiceProvider, IToolbarItem> factory) => new(null, factory, null);

    public static ToolbarItemRegistration ForRemoval(string id) => new(null, null, id);

    public void ApplyTo(Dictionary<string, IToolbarItem> items, List<IToolbarItem> created, IServiceProvider services)
    {
        switch (this)
        {
            case { Instance: { } instance }:
                items[instance.Id] = instance;
                break;
            case { Factory: { } factory }:
                var item = factory(services);
                created.Add(item);
                items[item.Id] = item;
                break;
            case { RemovedId: { } id }:
                items.Remove(id);
                break;
        }
    }
}
