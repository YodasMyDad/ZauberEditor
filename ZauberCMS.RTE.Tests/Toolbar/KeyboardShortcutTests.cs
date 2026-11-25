using ZauberCMS.RTE.Toolbar;

namespace ZauberCMS.RTE.Tests.Toolbar;

public sealed class KeyboardShortcutTests
{
    [Theory]
    [InlineData("Control+b", "b", true, false, false)]
    [InlineData("Ctrl+B", "b", true, false, false)]
    [InlineData("Cmd+k", "k", true, false, false)]
    [InlineData("Meta+K", "k", true, false, false)]
    [InlineData("control+shift+7", "7", true, true, false)]
    [InlineData("Control+Shift+Z", "z", true, true, false)]
    [InlineData(" Ctrl + Alt + 0 ", "0", true, false, true)]
    [InlineData("Option+Shift+F2", "f2", false, true, true)]
    [InlineData("Control+Enter", "enter", true, false, false)]
    [InlineData("Escape", "escape", false, false, false)]
    public void ParsesModifiersAndNormalisesTheKey(string text, string key, bool mod, bool shift, bool alt)
    {
        KeyboardShortcut.TryParse(text, out var shortcut).ShouldBeTrue();

        shortcut.ShouldBe(new KeyboardShortcut(key, mod, shift, alt));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("+")]
    [InlineData("Hyper+b")]
    [InlineData("b+Control")]
    [InlineData("Control+")]
    [InlineData("Control+Shift")]
    [InlineData("Shift")]
    public void RejectsInvalidShortcuts(string? text)
    {
        KeyboardShortcut.TryParse(text, out var shortcut).ShouldBeFalse();

        shortcut.ShouldBe(default);
    }

    [Theory]
    [InlineData("Control+b", "Ctrl+B")]
    [InlineData("cmd+shift+7", "Ctrl+Shift+7")]
    [InlineData("Alt+enter", "Alt+Enter")]
    [InlineData("Control+Alt+Shift+x", "Ctrl+Shift+Alt+X")]
    public void DisplaysInACanonicalForm(string text, string expected)
    {
        KeyboardShortcut.TryParse(text, out var shortcut).ShouldBeTrue();

        shortcut.ToString().ShouldBe(expected);
    }
}
