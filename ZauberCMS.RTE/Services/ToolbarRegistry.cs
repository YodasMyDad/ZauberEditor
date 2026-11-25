using System.Collections.Frozen;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZauberCMS.RTE.Models;
using ZauberCMS.RTE.Toolbar;

namespace ZauberCMS.RTE.Services;

internal sealed class ToolbarRegistry(IOptions<ZauberRteOptions> options, ILogger<ToolbarRegistry> logger)
{
    private readonly ToolbarItemRegistration[] _registrations =
    [
        .. BuiltInToolbarItems.All.Select(ToolbarItemRegistration.ForInstance),
        .. options.Value.Registrations
    ];

    private readonly FrozenSet<string> _removedIds = options.Value.Registrations
        .Select(registration => registration.RemovedId)
        .OfType<string>()
        .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public ToolbarItemSet CreateItemSet(IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);
        Dictionary<string, IToolbarItem> items = [with(StringComparer.OrdinalIgnoreCase)];
        List<IToolbarItem> created = [];
        foreach (var registration in _registrations)
        {
            registration.ApplyTo(items, created, services, logger);
        }

        return new(items.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase), created, _removedIds);
    }
}
