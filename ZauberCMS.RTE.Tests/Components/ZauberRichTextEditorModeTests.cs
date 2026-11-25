using Bunit;
using ZauberCMS.RTE.Interop;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Tests.Components;

public sealed class ZauberRichTextEditorModeTests : EditorTestContext
{
    [Theory]
    [InlineData(Theme.Auto, "rte-theme-auto")]
    [InlineData(Theme.Light, "rte-theme-light")]
    [InlineData(Theme.Dark, "rte-theme-dark")]
    public void ThemeSetsTheRootClass(Theme theme, string themeClass)
    {
        var cut = RenderEditor(parameters => parameters.Add(editor => editor.Theme, theme));

        cut.Find("div.zauber-rte").ClassList.ShouldContain(themeClass);
        cut.FindAll("[class*='rte-theme-']").Count.ShouldBe(1);
    }

    [Fact]
    public void ChangingTheThemeParameterUpdatesTheEditor()
    {
        var cut = RenderEditor(parameters => parameters.Add(editor => editor.Theme, Theme.Light));

        cut.Render(parameters => parameters.Add(editor => editor.Theme, Theme.Dark));

        cut.Find("div.zauber-rte").ClassList.ShouldContain("rte-theme-dark");
        cut.Find(".rte-btn-themeToggle").GetAttribute("aria-pressed").ShouldBe("true");
        cut.Instance.Api.State.Theme.ShouldBe(Theme.Dark);
    }

    [Fact]
    public async Task ThemeToggleSwitchesThemeAndRaisesThemeChanged()
    {
        List<Theme> changes = [];
        var cut = RenderEditor(parameters => parameters
            .Add(editor => editor.Theme, Theme.Light)
            .Add(editor => editor.ThemeChanged, (Theme theme) => changes.Add(theme)));

        await cut.Find(".rte-btn-themeToggle").ClickAsync(new());

        cut.Find("div.zauber-rte").ClassList.ShouldContain("rte-theme-dark");
        cut.Find(".rte-btn-themeToggle").GetAttribute("aria-pressed").ShouldBe("true");

        await cut.Find(".rte-btn-themeToggle").ClickAsync(new());

        cut.Find("div.zauber-rte").ClassList.ShouldContain("rte-theme-light");
        changes.ShouldBe([Theme.Dark, Theme.Light]);
    }

    [Fact]
    public async Task ThemeToggleInAutoModeFollowsTheSystemPreference()
    {
        List<Theme> changes = [];
        var cut = RenderEditor(parameters => parameters.Add(editor => editor.ThemeChanged, (Theme theme) => changes.Add(theme)));
        await Callbacks.OnStateChangedAsync(new EditorSnapshot([], "p", "left", false, false, false, false, false, false, false, false, false, false, PrefersDark: true, Words: 0, Characters: 0));

        cut.Find(".rte-btn-themeToggle").GetAttribute("aria-pressed").ShouldBe("true");

        await cut.Find(".rte-btn-themeToggle").ClickAsync(new());

        changes.ShouldBe([Theme.Light]);
        cut.Find("div.zauber-rte").ClassList.ShouldContain("rte-theme-light");
    }

    [Fact]
    public async Task FullscreenAndPlainPasteFollowTheScriptState()
    {
        var cut = RenderEditor();
        cut.Find("div.zauber-rte").ClassList.ShouldNotContain("rte-fullscreen");

        await Callbacks.OnStateChangedAsync(new EditorSnapshot([], "p", "left", false, false, false, false, false, false, false, false, Fullscreen: true, PlainPaste: true, PrefersDark: false, Words: 0, Characters: 0));

        cut.Find("div.zauber-rte").ClassList.ShouldContain("rte-fullscreen");
        cut.Find(".rte-btn-fullscreen").GetAttribute("aria-pressed").ShouldBe("true");
        cut.Find(".rte-btn-pastePlainText").GetAttribute("aria-pressed").ShouldBe("true");
    }

    [Fact]
    public async Task SettingTheSameThemeRaisesNothing()
    {
        List<Theme> changes = [];
        var cut = RenderEditor(parameters => parameters
            .Add(editor => editor.Theme, Theme.Dark)
            .Add(editor => editor.ThemeChanged, (Theme theme) => changes.Add(theme)));

        await cut.InvokeAsync(() => cut.Instance.Api.SetThemeAsync(Theme.Dark, TestContext.Current.CancellationToken));

        changes.ShouldBeEmpty();
    }

    [Fact]
    public void ReadOnlyLocksTheContentAndDisablesEditingTools()
    {
        var cut = RenderEditor(parameters => parameters.Add(editor => editor.ReadOnly, true));

        cut.Find("div.zauber-rte").ClassList.ShouldContain("rte-readonly");
        var content = cut.Find(".rte-content");
        content.GetAttribute("contenteditable").ShouldBe("false");
        content.GetAttribute("aria-readonly").ShouldBe("true");
        cut.Find(".rte-source-view").HasAttribute("readonly").ShouldBeTrue();
        cut.FindAll(".rte-toolbar button.rte-toolbar-btn")
            .Where(button => !button.ClassList.Contains("rte-btn-themeToggle") && !button.ClassList.Contains("rte-btn-fullscreen"))
            .ShouldAllBe(button => button.HasAttribute("disabled"));
        cut.Find(".rte-btn-themeToggle").HasAttribute("disabled").ShouldBeFalse();
        cut.Find(".rte-btn-fullscreen").HasAttribute("disabled").ShouldBeFalse();
        cut.Find(".rte-btn-fullscreen").GetAttribute("tabindex").ShouldBe("0");
        InitOptions.ShouldNotBeNull().ReadOnly.ShouldBeTrue();
    }

    [Fact]
    public async Task TogglingReadOnlyReconfiguresTheScript()
    {
        var cut = RenderEditor();

        cut.Render(parameters => parameters.Add(editor => editor.ReadOnly, true));

        await Interop.Received(1).ConfigureAsync(Arg.Is<EditorInitOptions>(options => options.ReadOnly), Arg.Any<CancellationToken>());
        cut.Find(".rte-btn-bold").HasAttribute("disabled").ShouldBeTrue();
        cut.Instance.Api.State.IsReadOnly.ShouldBeTrue();

        cut.Render(parameters => parameters.Add(editor => editor.ReadOnly, false));

        await Interop.Received(1).ConfigureAsync(Arg.Is<EditorInitOptions>(options => !options.ReadOnly), Arg.Any<CancellationToken>());
        cut.Find(".rte-btn-bold").HasAttribute("disabled").ShouldBeFalse();
    }

    [Fact]
    public async Task UnchangedParametersDoNotReconfigure()
    {
        var settings = new EditorSettings();
        var cut = RenderEditor(parameters => parameters.Add(editor => editor.Settings, settings));

        cut.Render(parameters => parameters.Add(editor => editor.Settings, settings).Add(editor => editor.Placeholder, null));

        await Interop.DidNotReceive().ConfigureAsync(Arg.Any<EditorInitOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StructurallyEqualSettingsAndLayoutDoNotReconfigure()
    {
        var cut = RenderEditor(parameters => parameters.Add(editor => editor.Settings, Settings()).Add(editor => editor.Toolbar, Layout()));

        cut.Render(parameters => parameters.Add(editor => editor.Settings, Settings()).Add(editor => editor.Toolbar, Layout()));

        await Interop.DidNotReceive().ConfigureAsync(Arg.Any<EditorInitOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ChangedLayoutReconfiguresAndResetsTheTabStop()
    {
        var cut = RenderEditor(parameters => parameters.Add(editor => editor.Toolbar, Layout()));

        cut.Render(parameters => parameters.Add(editor => editor.Toolbar, Layout(ToolbarItemIds.Underline)));

        await Interop.Received(1).ConfigureAsync(Arg.Any<EditorInitOptions>(), Arg.Any<CancellationToken>());
        cut.Find(".rte-btn-bold").GetAttribute("tabindex").ShouldBe("0");
        cut.Find(".rte-btn-italic").GetAttribute("tabindex").ShouldBe("-1");
    }

    [Fact]
    public async Task ChangedPolicyReconfigures()
    {
        var cut = RenderEditor(parameters => parameters.Add(editor => editor.Settings, Settings()));

        cut.Render(parameters => parameters.Add(editor => editor.Settings, Settings("em")));

        await Interop.Received(1).ConfigureAsync(
            Arg.Is<EditorInitOptions>(options => options.Policy.AllowedTags.Contains("em")),
            Arg.Any<CancellationToken>());
    }

    private static ToolbarLayout Layout(params string[] extra) =>
        new(new ToolbarGroup([ToolbarItemIds.Bold, ToolbarItemIds.Italic, .. extra]), new ToolbarSeparator());

    private static EditorSettings Settings(params string[] extraTags) => new()
    {
        Dimensions = new() { Height = "200px" },
        HtmlPolicy = new() { AllowedTags = new HashSet<string>(["p", "strong", .. extraTags]) },
        ImageConstraints = new() { AllowedImageTypes = new HashSet<string>([".png"]) },
    };

    [Fact]
    public async Task SourceViewSwapsTheContentForTheTextarea()
    {
        var cut = RenderEditor();

        await cut.Find(".rte-btn-viewSource").ClickAsync(new());

        await Interop.Received(1).EnterSourceViewAsync(Arg.Any<CancellationToken>());
        cut.Find("div.zauber-rte").ClassList.ShouldContain("rte-source-mode");
        cut.Find(".rte-content").HasAttribute("hidden").ShouldBeTrue();
        cut.Find(".rte-source-view").HasAttribute("hidden").ShouldBeFalse();
        cut.Find(".rte-btn-viewSource").GetAttribute("aria-pressed").ShouldBe("true");
        cut.Find(".rte-btn-bold").HasAttribute("disabled").ShouldBeTrue();

        await cut.Find(".rte-btn-viewSource").ClickAsync(new());

        await Interop.Received(1).ExitSourceViewAsync(Arg.Any<CancellationToken>());
        cut.Find("div.zauber-rte").ClassList.ShouldNotContain("rte-source-mode");
        cut.Find(".rte-content").HasAttribute("hidden").ShouldBeFalse();
    }

    [Fact]
    public async Task OverlappingSourceViewTogglesStayInStepWithTheScript()
    {
        var entering = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Interop.EnterSourceViewAsync(Arg.Any<CancellationToken>()).Returns(entering.Task);
        var cut = RenderEditor();
        var token = TestContext.Current.CancellationToken;

        var first = cut.InvokeAsync(() => cut.Instance.Api.ToggleSourceViewAsync(token));
        var second = cut.InvokeAsync(() => cut.Instance.Api.ToggleSourceViewAsync(token));
        entering.SetResult();
        await Task.WhenAll(first, second).WaitAsync(TimeSpan.FromSeconds(5), token);

        Received.InOrder(() =>
        {
            Interop.EnterSourceViewAsync(Arg.Any<CancellationToken>());
            Interop.ExitSourceViewAsync(Arg.Any<CancellationToken>());
        });
        cut.Instance.Api.State.IsSourceView.ShouldBeFalse();
        cut.Find("div.zauber-rte").ClassList.ShouldNotContain("rte-source-mode");

        await cut.InvokeAsync(() => cut.Instance.Api.ToggleSourceViewAsync(token));

        await Interop.Received(2).EnterSourceViewAsync(Arg.Any<CancellationToken>());
        cut.Instance.Api.State.IsSourceView.ShouldBeTrue();
    }
}
