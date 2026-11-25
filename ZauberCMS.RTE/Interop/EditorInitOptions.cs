using System.Collections.Frozen;
using ZauberCMS.RTE.Models;
using ZauberCMS.RTE.Toolbar;

namespace ZauberCMS.RTE.Interop;

internal sealed record EditorInitOptions(
    string? Html,
    bool ReadOnly,
    string? Placeholder,
    IReadOnlyList<string> TrackedTags,
    IReadOnlyDictionary<string, string> TagAliases,
    IReadOnlyList<ShortcutBinding> Shortcuts,
    HtmlPolicyOptions Policy,
    int HistoryLimit,
    int ChangeDelay,
    int InlineLimit,
    ImageOptions Image)
{
    public const int DefaultInlineLimit = 12_000;

    public static EditorInitOptions Create(
        string? html,
        bool readOnly,
        string? placeholder,
        EditorSettings settings,
        IReadOnlyList<IToolbarItem> items)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(items);
        return new(
            html,
            readOnly,
            placeholder,
            NameList.Normalise(items.SelectMany(item => item.TrackedTags)),
            items.SelectMany(Aliases).DistinctBy(alias => alias.Key).ToFrozenDictionary(),
            [.. items.Select(Binding).OfType<ShortcutBinding>()],
            HtmlPolicyOptions.From(settings.HtmlPolicy),
            settings.HistoryLimit,
            (int)settings.ChangeDelay.TotalMilliseconds,
            DefaultInlineLimit,
            ImageOptions.From(settings.ImageConstraints));
    }

    private static IEnumerable<KeyValuePair<string, string>> Aliases(IToolbarItem item) =>
        item.PrimaryTag is { } primary
            ? item.TrackedTags
                .Where(tag => !string.Equals(tag, primary, StringComparison.OrdinalIgnoreCase))
                .Select(tag => KeyValuePair.Create(tag.ToLowerInvariant(), primary.ToLowerInvariant()))
            : [];

    private static ShortcutBinding? Binding(IToolbarItem item) =>
        KeyboardShortcut.TryParse(item.Shortcut, out var shortcut)
            ? new(item.Id, shortcut.Key, shortcut.Mod, shortcut.Shift, shortcut.Alt)
            : null;
}
