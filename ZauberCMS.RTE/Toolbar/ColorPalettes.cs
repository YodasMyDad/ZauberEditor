using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Toolbar;

internal static class ColorPalettes
{
    public static IReadOnlyList<ColorItem> Text { get; } =
    [
        TextColor("red", "Red", "#ef4444"),
        TextColor("orange", "Orange", "#ea580c"),
        TextColor("amber", "Amber", "#d97706"),
        TextColor("green", "Green", "#16a34a"),
        TextColor("teal", "Teal", "#0d9488"),
        TextColor("blue", "Blue", "#3b82f6"),
        TextColor("purple", "Purple", "#a855f7"),
        TextColor("pink", "Pink", "#ec4899"),
        TextColor("grey", "Grey", "#64748b"),
        TextColor("default", "Default colour", null)
    ];

    public static IReadOnlyList<ColorItem> Highlight { get; } =
    [
        HighlightColor("yellow", "Yellow", "rgb(250 204 21 / 40%)"),
        HighlightColor("orange", "Orange", "rgb(251 146 60 / 40%)"),
        HighlightColor("red", "Red", "rgb(248 113 113 / 40%)"),
        HighlightColor("pink", "Pink", "rgb(244 114 182 / 40%)"),
        HighlightColor("purple", "Purple", "rgb(192 132 252 / 40%)"),
        HighlightColor("blue", "Blue", "rgb(96 165 250 / 40%)"),
        HighlightColor("teal", "Teal", "rgb(45 212 191 / 40%)"),
        HighlightColor("green", "Green", "rgb(74 222 128 / 40%)"),
        HighlightColor("grey", "Grey", "rgb(148 163 184 / 40%)"),
        HighlightColor("none", "No highlight", null)
    ];

    private static ColorItem TextColor(string name, string label, string? value) =>
        new(ToolbarItemIds.TextColor, "color", name, label, value);

    private static ColorItem HighlightColor(string name, string label, string? value) =>
        new(ToolbarItemIds.BackgroundColor, "background-color", name, label, value);
}
