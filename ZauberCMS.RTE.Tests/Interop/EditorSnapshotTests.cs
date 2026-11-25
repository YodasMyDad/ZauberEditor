using ZauberCMS.RTE.Interop;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Tests.Interop;

public sealed class EditorSnapshotTests
{
    [Fact]
    public void CopiesTheReportedSelectionState()
    {
        var snapshot = new EditorSnapshot(["strong", "EM"], "H2", "center", true, true, true, true, true, true, true, true, true, true, true, 12, 64);

        var state = snapshot.ApplyTo(EditorState.Empty);

        state.ActiveMarks.ShouldBe(["strong", "EM"], ignoreOrder: true);
        state.ActiveMarks.Contains("em").ShouldBeTrue();
        state.BlockType.ShouldBe("h2");
        state.HeadingLevel.ShouldBe(2);
        state.Alignment.ShouldBe(TextAlignment.Center);
        state.HasSelection.ShouldBeTrue();
        state.HasFocus.ShouldBeTrue();
        state.CanUndo.ShouldBeTrue();
        state.CanRedo.ShouldBeTrue();
        state.InLink.ShouldBeTrue();
        state.InImage.ShouldBeTrue();
        state.InTable.ShouldBeTrue();
        state.CanOutdent.ShouldBeTrue();
        state.IsFullscreen.ShouldBeTrue();
        state.IsPlainTextPaste.ShouldBeTrue();
        state.PrefersDarkScheme.ShouldBeTrue();
        state.WordCount.ShouldBe(12);
        state.CharacterCount.ShouldBe(64);
    }

    [Fact]
    public void KeepsStateTheScriptDoesNotOwn()
    {
        var current = EditorState.Empty with { IsSourceView = true, IsReadOnly = true, Theme = Theme.Dark };

        var state = Empty.ApplyTo(current);

        state.IsSourceView.ShouldBeTrue();
        state.IsReadOnly.ShouldBeTrue();
        state.Theme.ShouldBe(Theme.Dark);
    }

    [Fact]
    public void MissingValuesFallBackToAPlainParagraph()
    {
        var current = EditorState.Empty with { BlockType = "h1", CanUndo = true };

        var state = Empty.ApplyTo(current);

        state.ActiveMarks.ShouldBeEmpty();
        state.BlockType.ShouldBe("p");
        state.Alignment.ShouldBe(TextAlignment.Left);
        state.CanUndo.ShouldBeFalse();
    }

    [Theory]
    [InlineData("left", TextAlignment.Left)]
    [InlineData("center", TextAlignment.Center)]
    [InlineData("right", TextAlignment.Right)]
    [InlineData("justify", TextAlignment.Justify)]
    [InlineData("start", TextAlignment.Left)]
    [InlineData(null, TextAlignment.Left)]
    public void AlignmentIsMapped(string? alignment, TextAlignment expected) =>
        (Empty with { Alignment = alignment }).ApplyTo(EditorState.Empty).Alignment.ShouldBe(expected);

    private static EditorSnapshot Empty => new(null, null, null, false, false, false, false, false, false, false, false, false, false, false, 0, 0);
}
