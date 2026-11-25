using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using ZauberCMS.RTE.Models;
using ZauberCMS.RTE.Toolbar;

namespace ZauberCMS.RTE.Components.Internal;

internal static class ToolbarPresenter
{
    public static string Title(IToolbarItem item)
    {
        var text = item.Tooltip ?? item.Label;
        return KeyboardShortcut.TryParse(item.Shortcut, out var shortcut) ? $"{text} ({shortcut})" : text;
    }

    public static string? Pressed(IToolbarItem item, bool isActive) => item.IsToggle ? Bool(isActive) : null;

    public static string Bool(bool value) => value ? "true" : "false";

    public static string ButtonClass(string baseClass, bool isActive, bool isEnabled, bool isOpen = false) =>
        string.Concat(
            baseClass,
            isActive ? " rte-btn-active" : null,
            isEnabled ? null : " rte-btn-disabled",
            isOpen ? " rte-dropdown-open" : null);

    public static bool HasIcon(IToolbarItem item) => item is { IconSvg.Length: > 0 } or { IconCss.Length: > 0 };

    public static RenderFragment Icon(IToolbarItem item, bool labelFallback = true) => builder =>
    {
        switch (item)
        {
            case { IconSvg: { Length: > 0 } svg }:
                AddSvg(builder, svg, null);
                break;
            case { IconCss: { Length: > 0 } css }:
                builder.OpenElement(0, "span");
                builder.AddAttribute(1, "class", "rte-icon");
                builder.AddAttribute(2, "aria-hidden", "true");
                builder.OpenElement(3, "i");
                builder.AddAttribute(4, "class", css);
                builder.CloseElement();
                builder.CloseElement();
                break;
            case not null when labelFallback:
                builder.OpenElement(5, "span");
                builder.AddAttribute(6, "class", "rte-toolbar-btn-text");
                builder.AddContent(7, item.Label);
                builder.CloseElement();
                break;
        }
    };

    public static RenderFragment Svg(string svg, string? extraClass = null) => builder => AddSvg(builder, svg, extraClass);

    private static void AddSvg(RenderTreeBuilder builder, string svg, string? extraClass)
    {
        builder.OpenElement(10, "span");
        builder.AddAttribute(11, "class", extraClass is null ? "rte-icon" : $"rte-icon {extraClass}");
        builder.AddAttribute(12, "aria-hidden", "true");
        builder.AddMarkupContent(13, svg);
        builder.CloseElement();
    }
}
