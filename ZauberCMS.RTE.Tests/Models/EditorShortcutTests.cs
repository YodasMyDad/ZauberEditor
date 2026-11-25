using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Tests.Models;

public sealed class EditorShortcutTests
{
    private static readonly Func<IEditorApi, CancellationToken, Task> Handler = static (_, _) => Task.CompletedTask;

    [Theory]
    [InlineData("Control+s")]
    [InlineData("Ctrl+Shift+Enter")]
    [InlineData("Alt+1")]
    public void ValidKeysAreKept(string keys) => new EditorShortcut(keys, Handler).Keys.ShouldBe(keys);

    [Theory]
    [InlineData("Control+")]
    [InlineData("Control+Shift")]
    [InlineData("Hyper+s")]
    public void InvalidKeysAreRejected(string keys) =>
        Should.Throw<ArgumentException>(() => new EditorShortcut(keys, Handler)).ParamName.ShouldBe("Keys");

    [Fact]
    public void MissingValuesAreRejected()
    {
        Should.Throw<ArgumentException>(() => new EditorShortcut(" ", Handler));
        Should.Throw<ArgumentNullException>(() => new EditorShortcut("Control+s", null!));
        Should.Throw<ArgumentException>(() => new EditorShortcut("Control+s", Handler) with { Keys = "Shift" });
    }

    [Fact]
    public void ShortcutsWithTheSameKeysAndHandlerAreEqual() =>
        new EditorShortcut("Control+s", Handler).ShouldBe(new EditorShortcut("Control+s", Handler));
}
