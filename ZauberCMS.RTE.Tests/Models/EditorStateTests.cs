using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Tests.Models;

public sealed class EditorStateTests
{
    [Theory]
    [InlineData("h1", 1)]
    [InlineData("h6", 6)]
    [InlineData("p", 0)]
    [InlineData("h0", 0)]
    [InlineData("h7", 0)]
    [InlineData("h", 0)]
    [InlineData("h10", 0)]
    [InlineData("blockquote", 0)]
    public void HeadingLevelComesFromTheBlockType(string blockType, int level) =>
        (EditorState.Empty with { BlockType = blockType }).HeadingLevel.ShouldBe(level);

    [Theory]
    [InlineData(Theme.Light, false, false)]
    [InlineData(Theme.Light, true, false)]
    [InlineData(Theme.Dark, false, true)]
    [InlineData(Theme.Auto, false, false)]
    [InlineData(Theme.Auto, true, true)]
    public void IsDarkFollowsTheThemeAndSystemPreference(Theme theme, bool prefersDark, bool isDark) =>
        (EditorState.Empty with { Theme = theme, PrefersDarkScheme = prefersDark }).IsDark.ShouldBe(isDark);

    [Fact]
    public void EmptyStateIsAnEditableParagraph()
    {
        var state = EditorState.Empty;

        state.BlockType.ShouldBe("p");
        state.ActiveMarks.ShouldBeEmpty();
        state.Alignment.ShouldBe(TextAlignment.Left);
        state.Theme.ShouldBe(Theme.Auto);
        state.IsReadOnly.ShouldBeFalse();
        state.IsSourceView.ShouldBeFalse();
    }
}
