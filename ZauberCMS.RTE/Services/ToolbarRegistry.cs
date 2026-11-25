using System.Collections.Frozen;
using Microsoft.Extensions.Options;
using ZauberCMS.RTE.Models;
using ZauberCMS.RTE.Toolbar;

namespace ZauberCMS.RTE.Services;

internal sealed class ToolbarRegistry(IOptions<ZauberRteOptions> options)
{
    private readonly ToolbarItemRegistration[] _registrations =
    [
        .. BuiltInToolbarItems.All.Select(ToolbarItemRegistration.ForInstance),
        .. options.Value.Registrations
    ];

    public ToolbarItemSet CreateItemSet(IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);
        Dictionary<string, IToolbarItem> items = new(StringComparer.OrdinalIgnoreCase);
        List<IToolbarItem> created = [];
        foreach (var registration in _registrations)
        {
            registration.ApplyTo(items, created, services);
        }

        return new(items.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase), created);
    }
}
