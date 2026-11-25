using System.Collections.Frozen;
using ZauberCMS.RTE.Models;

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
    long MaxContentBytes,
    ImageOptions Image)
{
    public const int DefaultInlineLimit = 12_000;

    public string? Stylesheet { get; init; }

    public static EditorInitOptions Create(
        string? html,
        bool readOnly,
        string? placeholder,
        EditorSettings settings,
        IReadOnlyList<IToolbarItem> items,
        IReadOnlyList<ShortcutBinding> editorShortcuts)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(editorShortcuts);
        return new(
            html,
            readOnly,
            placeholder,
            NameList.Normalise(items.SelectMany(item => item.TrackedTags)),
            items.SelectMany(Aliases).DistinctBy(alias => alias.Key).ToFrozenDictionary(),
            [.. items.Select(item => ShortcutBinding.From(item.Id, item.Shortcut)).OfType<ShortcutBinding>(), .. editorShortcuts],
            HtmlPolicyOptions.From(settings.HtmlPolicy),
            settings.HistoryLimit,
            (int)settings.ChangeDelay.TotalMilliseconds,
            DefaultInlineLimit,
            settings.MaxContentBytes,
            ImageOptions.From(settings.ImageConstraints));
    }

    private static IEnumerable<KeyValuePair<string, string>> Aliases(IToolbarItem item) =>
        item.PrimaryTag is { } primary
            ? item.TrackedTags
                .Where(tag => !string.Equals(tag, primary, StringComparison.OrdinalIgnoreCase))
                .Select(tag => KeyValuePair.Create(tag.ToLowerInvariant(), primary.ToLowerInvariant()))
            : [];
}
