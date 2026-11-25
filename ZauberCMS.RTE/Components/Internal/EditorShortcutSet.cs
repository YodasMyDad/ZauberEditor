using System.Collections.Frozen;
using ZauberCMS.RTE.Interop;
using ZauberCMS.RTE.Models;
using ZauberCMS.RTE.Toolbar;

namespace ZauberCMS.RTE.Components.Internal;

internal sealed class EditorShortcutSet
{
    private readonly IReadOnlyList<EditorShortcut> _source;
    private readonly FrozenDictionary<string, EditorShortcut> _byId;

    private EditorShortcutSet(IReadOnlyList<EditorShortcut> source, FrozenDictionary<string, EditorShortcut> byId, IReadOnlyList<ShortcutBinding> bindings)
    {
        _source = source;
        _byId = byId;
        Bindings = bindings;
    }

    public static EditorShortcutSet Empty { get; } = new([], FrozenDictionary<string, EditorShortcut>.Empty, []);

    public IReadOnlyList<ShortcutBinding> Bindings { get; }

    public EditorShortcut? Find(string id) => _byId.GetValueOrDefault(id);

    public EditorShortcutSet Update(IReadOnlyList<EditorShortcut>? shortcuts)
    {
        var source = shortcuts ?? [];
        if (ValueEquality.Sequence(_source, source))
        {
            return this;
        }

        Dictionary<string, EditorShortcut> byId = new(StringComparer.Ordinal);
        foreach (var shortcut in LayoutGuard.NoNulls(source, nameof(shortcuts)))
        {
            if (KeyboardShortcut.TryParse(shortcut.Keys, out var keys))
            {
                byId[keys.ToString()] = shortcut;
            }
        }

        ShortcutBinding[] bindings = [.. byId.Select(pair => ShortcutBinding.From(pair.Key, pair.Value.Keys, custom: true)).OfType<ShortcutBinding>()];
        return new(source, byId.ToFrozenDictionary(StringComparer.Ordinal), ValueEquality.Sequence(Bindings, bindings) ? Bindings : bindings);
    }
}
