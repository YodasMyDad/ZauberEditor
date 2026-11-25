using Microsoft.AspNetCore.Components;
using ZauberCMS.RTE.Models;
using ZauberCMS.RTE.Services;

namespace ZauberRTE.Web.Components.Pages;

public sealed partial class ToolbarCatalog
{
    private readonly HashSet<string> _selected = new(StringComparer.OrdinalIgnoreCase)
    {
        ToolbarItemIds.Bold, ToolbarItemIds.Italic, ToolbarItemIds.Link
    };

    private string? _html = "<p>Pick toolbar items from the table above.</p>";

    [Inject]
    private IToolbarItemCatalog Catalog { get; set; } = default!;

    private IReadOnlyList<ToolbarItemDescriptor> Items => Catalog.GetItems();

    private ToolbarLayout Layout => new(new ToolbarGroup([.. Items.Select(item => item.Id).Where(_selected.Contains)]));

    private static MarkupString? Icon(ToolbarItemDescriptor item) => item.IconSvg is { Length: > 0 } svg ? new MarkupString(svg) : null;

    private void Toggle(string id)
    {
        if (!_selected.Remove(id))
        {
            _selected.Add(id);
        }
    }
}
