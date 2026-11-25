namespace ZauberCMS.RTE.Toolbar;

internal readonly record struct KeyboardShortcut(string Key, bool Mod, bool Shift, bool Alt)
{
    public static bool TryParse(string? text, out KeyboardShortcut shortcut)
    {
        shortcut = default;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var parts = text.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts is [] || ModifierOf(parts[^1]) is not Modifier.None)
        {
            return false;
        }

        var (mod, shift, alt) = (false, false, false);
        foreach (var part in parts[..^1])
        {
            switch (ModifierOf(part))
            {
                case Modifier.Mod:
                    mod = true;
                    break;
                case Modifier.Shift:
                    shift = true;
                    break;
                case Modifier.Alt:
                    alt = true;
                    break;
                default:
                    return false;
            }
        }

        shortcut = new(parts[^1].ToLowerInvariant(), mod, shift, alt);
        return true;
    }

    public override string ToString()
    {
        List<string> parts = [];
        if (Mod)
        {
            parts.Add("Ctrl");
        }

        if (Shift)
        {
            parts.Add("Shift");
        }

        if (Alt)
        {
            parts.Add("Alt");
        }

        parts.Add(Key.Length == 1 ? Key.ToUpperInvariant() : string.Concat(Key[..1].ToUpperInvariant(), Key[1..]));
        return string.Join('+', parts);
    }

    private static Modifier ModifierOf(string part) => part.ToUpperInvariant() switch
    {
        "CONTROL" or "CTRL" or "CMD" or "META" or "MOD" => Modifier.Mod,
        "SHIFT" => Modifier.Shift,
        "ALT" or "OPTION" => Modifier.Alt,
        _ => Modifier.None
    };

    private enum Modifier
    {
        None,
        Mod,
        Shift,
        Alt
    }
}
