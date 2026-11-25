using ZauberCMS.RTE.Models;
using ZauberCMS.RTE.Tests.TestSupport;

namespace ZauberCMS.RTE.Tests.Models;

public sealed class ToolbarItemBaseTests
{
    private readonly ToolbarItemBase _item = new RecordingItem();

    [Fact]
    public void DefaultsDescribeAPlainButton()
    {
        _item.Kind.ShouldBe(ToolbarItemKind.Button);
        _item.IsToggle.ShouldBeFalse();
        _item.ChildItemIds.ShouldBeEmpty();
        _item.TrackedTags.ShouldBeEmpty();
        _item.PrimaryTag.ShouldBeNull();
        _item.IconSvg.ShouldBeNull();
        _item.IconCss.ShouldBeNull();
        _item.Shortcut.ShouldBeNull();
        _item.Tooltip.ShouldBeNull();
    }

    [Theory]
    [InlineData(false, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    public void EnabledOnlyWhileEditable(bool readOnly, bool sourceView, bool enabled) =>
        _item.IsEnabled(EditorState.Empty with { IsReadOnly = readOnly, IsSourceView = sourceView }).ShouldBe(enabled);

    [Fact]
    public void WithoutAPrimaryTagTheItemIsNeverActive() =>
        _item.IsActive(States.Marks("strong", "em", "code")).ShouldBeFalse();

    [Fact]
    public void NullStateIsRejected()
    {
        Should.Throw<ArgumentNullException>(() => _item.IsEnabled(null!));
        Should.Throw<ArgumentNullException>(() => _item.IsActive(null!));
    }
}
