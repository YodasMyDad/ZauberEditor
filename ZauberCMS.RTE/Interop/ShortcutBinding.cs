using ZauberCMS.RTE.Toolbar;

namespace ZauberCMS.RTE.Interop;

internal sealed record ShortcutBinding(string ItemId, string Key, bool Mod, bool Shift, bool Alt, bool Custom = false)
{
    public static ShortcutBinding? From(string id, string? keys, bool custom = false) =>
        KeyboardShortcut.TryParse(keys, out var shortcut)
            ? new(id, shortcut.Key, shortcut.Mod, shortcut.Shift, shortcut.Alt, custom)
            : null;
}
