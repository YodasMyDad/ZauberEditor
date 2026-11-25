using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZauberCMS.RTE.Components;
using ZauberCMS.RTE.Interop;
using ZauberCMS.RTE.Models;
using ZauberCMS.RTE.Services;
using ZauberCMS.RTE.Tests.TestSupport;

namespace ZauberCMS.RTE.Tests.Components;

public sealed class ZauberRichTextEditorRenderingTests : EditorTestContext
{
    [Fact]
    public void RendersTheEditorStructure()
    {
        var cut = RenderEditor();

        var root = cut.Find("div.zauber-rte");
        root.ClassList.ShouldBe(["zauber-rte", "rte-theme-auto"], ignoreOrder: true);
        cut.FindAll(".zauber-rte > .rte-toolbar").Count.ShouldBe(1);
        cut.FindAll(".zauber-rte > .rte-editor-container > .rte-content").Count.ShouldBe(1);
        cut.FindAll(".zauber-rte > .rte-editor-container > textarea.rte-source-view").Count.ShouldBe(1);
        cut.Find(".zauber-rte > .rte-editor-resize-handle").GetAttribute("aria-hidden").ShouldBe("true");
        cut.FindAll(".rte-slideout-overlay").ShouldBeEmpty();
    }

    [Fact]
    public void ContentIsAnAccessibleTextboxThatTheScriptOwns()
    {
        var cut = RenderEditor(parameters => parameters.Add(editor => editor.Value, "<p>Hello</p>").Add(editor => editor.Placeholder, "Write here"));

        var content = cut.Find(".rte-content");
        content.GetAttribute("contenteditable").ShouldBe("true");
        content.GetAttribute("role").ShouldBe("textbox");
        content.GetAttribute("aria-multiline").ShouldBe("true");
        content.GetAttribute("aria-label").ShouldBe("Rich text editor");
        content.GetAttribute("aria-readonly").ShouldBe("false");
        content.GetAttribute("data-placeholder").ShouldBe("Write here");
        content.HasAttribute("hidden").ShouldBeFalse();
        content.ChildNodes.ShouldBeEmpty();
        var source = cut.Find(".rte-source-view");
        source.HasAttribute("hidden").ShouldBeTrue();
        source.GetAttribute("aria-label").ShouldBe("HTML source");
        source.GetAttribute("spellcheck").ShouldBe("false");
    }

    [Fact]
    public void ToolbarIsLabelledAndControlsTheContent()
    {
        var cut = RenderEditor(parameters => parameters.Add(editor => editor.AriaLabel, "Article body"));

        var toolbar = cut.Find(".rte-toolbar");
        var content = cut.Find(".rte-content");
        toolbar.GetAttribute("role").ShouldBe("toolbar");
        toolbar.GetAttribute("aria-label").ShouldBe("Article body toolbar");
        content.GetAttribute("aria-label").ShouldBe("Article body");
        content.Id.ShouldNotBeNullOrEmpty();
        toolbar.GetAttribute("aria-controls").ShouldBe(content.Id);
    }

    [Fact]
    public void ContentIdsAreUniquePerEditor()
    {
        var first = RenderEditor().Find(".rte-content").Id;
        var second = RenderEditor().Find(".rte-content").Id;

        first.ShouldNotBe(second);
    }

    [Fact]
    public void ToolbarHasGroupsSeparatorsAndLabelledButtons()
    {
        var cut = RenderEditor();

        cut.FindAll(".rte-toolbar > [role=group]").Count.ShouldBe(8);
        var separators = cut.FindAll(".rte-toolbar > [role=separator]");
        separators.Count.ShouldBe(7);
        separators.ShouldAllBe(separator => separator.GetAttribute("aria-orientation") == "vertical");
        var buttons = cut.FindAll(".rte-toolbar button.rte-toolbar-btn");
        buttons.Count.ShouldBe(33);
        buttons.ShouldAllBe(button => button.GetAttribute("type") == "button" && !string.IsNullOrWhiteSpace(button.GetAttribute("aria-label")));
        cut.FindAll(".rte-toolbar button .rte-icon").ShouldAllBe(icon => icon.GetAttribute("aria-hidden") == "true");
    }

    [Fact]
    public void TogglesExposeAriaPressedAndActionsDoNot()
    {
        var cut = RenderEditor();

        cut.Find(".rte-btn-bold").GetAttribute("aria-pressed").ShouldBe("false");
        cut.Find(".rte-btn-alignLeft").GetAttribute("aria-pressed").ShouldBe("true");
        cut.Find(".rte-btn-undo").HasAttribute("aria-pressed").ShouldBeFalse();
        cut.Find(".rte-btn-clean-html").HasAttribute("aria-pressed").ShouldBeFalse();
    }

    [Fact]
    public void ButtonsShowTheirShortcutInTheTitle()
    {
        var cut = RenderEditor();

        cut.Find(".rte-btn-bold").GetAttribute("title").ShouldBe("Bold (Ctrl+B)");
        cut.Find(".rte-btn-ol").GetAttribute("title").ShouldBe("Numbered list (Ctrl+Shift+7)");
        cut.Find(".rte-btn-table").GetAttribute("title").ShouldBe("Table");
    }

    [Fact]
    public void ToolbarHasASingleTabStopOnTheFirstEnabledButton()
    {
        var cut = RenderEditor();

        var stops = cut.FindAll(".rte-toolbar button[tabindex='0']");
        stops.ShouldHaveSingleItem().ClassList.ShouldContain("rte-btn-bold");
        cut.FindAll(".rte-toolbar button.rte-toolbar-btn").Count(button => button.GetAttribute("tabindex") == "-1").ShouldBe(32);
    }

    [Fact]
    public void ArrowKeyNavigationIsLeftToTheScriptSoTheTabStopIsNotRewritten()
    {
        var cut = RenderEditor(parameters => parameters.Add(editor => editor.Toolbar, new ToolbarLayout(new ToolbarGroup("bold", "undo", "italic"))));

        Should.Throw<MissingEventHandlerException>(() => cut.Find(".rte-toolbar").KeyDown(new KeyboardEventArgs { Key = "ArrowRight" }));

        cut.Find(".rte-btn-bold").GetAttribute("tabindex").ShouldBe("0");
        cut.Find(".rte-btn-italic").GetAttribute("tabindex").ShouldBe("-1");
    }

    [Fact]
    public async Task TabStopMovesToTheFirstEnabledButtonWhenTheStateChanges()
    {
        var cut = RenderEditor(parameters => parameters.Add(editor => editor.Toolbar, new ToolbarLayout(new ToolbarGroup("undo", "bold"))));
        cut.Find(".rte-btn-bold").GetAttribute("tabindex").ShouldBe("0");

        await Callbacks.OnStateChangedAsync(new EditorSnapshot([], "p", "left", false, true, true, false, false, false, false, false, false, false, false, 0, 0));

        cut.Find(".rte-btn-undo").GetAttribute("tabindex").ShouldBe("0");
        cut.Find(".rte-btn-bold").GetAttribute("tabindex").ShouldBe("-1");
    }

    [Fact]
    public void DropdownTriggerAnnouncesItsMenu()
    {
        var cut = RenderEditor();

        var trigger = cut.Find(".rte-dropdown-headings > button");
        trigger.GetAttribute("aria-haspopup").ShouldBe("menu");
        trigger.GetAttribute("aria-expanded").ShouldBe("false");
        trigger.GetAttribute("aria-label").ShouldBe("Headings");
        cut.FindAll("[role=menu]").ShouldBeEmpty();

        trigger.Click();

        cut.Find(".rte-dropdown-headings > button").GetAttribute("aria-expanded").ShouldBe("true");
        var menu = cut.Find("[role=menu]");
        menu.GetAttribute("aria-label").ShouldBe("Headings");
        var items = cut.FindAll("[role=menu] > button");
        items.Count.ShouldBe(8);
        items.ShouldAllBe(item => item.GetAttribute("role") == "menuitemradio");
        items.Select(item => item.GetAttribute("aria-checked")).ShouldBe(["true", "false", "false", "false", "false", "false", "false", "false"]);
    }

    [Fact]
    public void ClickingOutsideTheMenuClosesIt()
    {
        var cut = RenderEditor();
        cut.Find(".rte-dropdown-headings > button").Click();

        cut.Find(".rte-dropdown-backdrop").Click();

        cut.FindAll("[role=menu]").ShouldBeEmpty();
        cut.Find(".rte-dropdown-headings > button").GetAttribute("aria-expanded").ShouldBe("false");
    }

    [Fact]
    public async Task ReportedStateIsReflectedInTheToolbar()
    {
        EditorState? reported = null;
        var cut = RenderEditor(parameters => parameters.Add(editor => editor.OnStateChanged, (EditorState state) => reported = state));

        await Callbacks.OnStateChangedAsync(new EditorSnapshot(["strong"], "h2", "center", false, true, true, false, false, false, false, false, false, false, false, 0, 0));

        cut.Find(".rte-btn-bold").GetAttribute("aria-pressed").ShouldBe("true");
        cut.Find(".rte-btn-bold").ClassList.ShouldContain("rte-btn-active");
        cut.Find(".rte-btn-italic").GetAttribute("aria-pressed").ShouldBe("false");
        cut.Find(".rte-btn-alignCenter").GetAttribute("aria-pressed").ShouldBe("true");
        cut.Find(".rte-btn-alignLeft").GetAttribute("aria-pressed").ShouldBe("false");
        cut.Find(".rte-btn-undo").HasAttribute("disabled").ShouldBeFalse();
        cut.Find(".rte-btn-redo").HasAttribute("disabled").ShouldBeTrue();
        cut.Find(".rte-dropdown-headings .rte-dropdown-label").TextContent.ShouldBe("H2");
        reported.ShouldNotBeNull().BlockType.ShouldBe("h2");
        cut.Instance.Api.State.ShouldBeSameAs(reported);
    }

    [Fact]
    public async Task ActiveHeadingIsCheckedInTheMenu()
    {
        var cut = RenderEditor();
        await Callbacks.OnStateChangedAsync(new EditorSnapshot([], "h3", null, false, true, false, false, false, false, false, false, false, false, false, 0, 0));

        cut.Find(".rte-dropdown-headings > button").Click();

        cut.Find(".rte-dropdown-h3").GetAttribute("aria-checked").ShouldBe("true");
        cut.Find(".rte-dropdown-h1").GetAttribute("aria-checked").ShouldBe("false");
    }

    [Fact]
    public void ExtraAttributesAreAppliedToTheRoot()
    {
        var cut = RenderEditor(parameters => parameters
            .AddUnmatched("class", "my-editor")
            .AddUnmatched("style", "color: red")
            .AddUnmatched("data-testid", "editor"));

        var root = cut.Find("div.zauber-rte");
        root.ClassList.ShouldContain("my-editor");
        root.ClassList.ShouldContain("zauber-rte");
        root.GetAttribute("data-testid").ShouldBe("editor");
        root.GetAttribute("style").ShouldBe("--rte-height:300px;--rte-min-height:100px;color: red");
    }

    [Fact]
    public void OnlyTheConfiguredDimensionsAreEmitted()
    {
        var settings = new EditorSettings { Dimensions = new() { Width = "40rem", Height = null, MinHeight = null, MaxHeight = "80vh" } };

        var cut = RenderEditor(parameters => parameters.Add(editor => editor.Settings, settings));

        cut.Find("div.zauber-rte").GetAttribute("style").ShouldBe("--rte-width:40rem;--rte-max-height:80vh");
    }

    [Fact]
    public void NoDimensionsMeansNoStyle()
    {
        var settings = new EditorSettings { Dimensions = new() { Height = null, MinHeight = null } };

        var cut = RenderEditor(parameters => parameters.Add(editor => editor.Settings, settings));

        cut.Find("div.zauber-rte").HasAttribute("style").ShouldBeFalse();
    }

    [Fact]
    public void StylesheetIsLinkedAndTheRootHiddenUntilTheScriptHasLoaded()
    {
        const string stylesheet = "_content/ZauberCMS.RTE/css/zauber-editor.css";
        var creation = DeferCreation();
        var cut = RenderEditor();

        cut.Find("link[rel=stylesheet]").GetAttribute("href").ShouldBe(stylesheet);
        cut.Find("div.zauber-rte").GetAttribute("style").ShouldEndWith("visibility:var(--_rte-styled, hidden)");
        InitOptions.ShouldNotBeNull().Stylesheet.ShouldBe(stylesheet);

        creation.SetResult(Interop);

        cut.WaitForAssertion(() => cut.FindAll("link").ShouldBeEmpty());
        cut.Find("div.zauber-rte").GetAttribute("style").ShouldBe("--rte-height:300px;--rte-min-height:100px");
    }

    [Fact]
    public void RootIsShownWhenTheScriptFailsToLoad()
    {
        var creation = DeferCreation();
        var cut = RenderEditor();

        creation.SetResult(null);

        cut.WaitForAssertion(() => cut.Find("div.zauber-rte").GetAttribute("style").ShouldBe("--rte-height:300px;--rte-min-height:100px"));
        cut.FindAll("link").ShouldBeEmpty();
    }

    [Fact]
    public void AutoLoadStylesOffLinksNothingAndNeverHidesTheRoot()
    {
        Services.AddZauberRte(options => options.AutoLoadStyles = false);
        DeferCreation();

        var cut = RenderEditor();

        cut.FindAll("link").ShouldBeEmpty();
        cut.Find("div.zauber-rte").GetAttribute("style").ShouldBe("--rte-height:300px;--rte-min-height:100px");
        InitOptions.ShouldNotBeNull().Stylesheet.ShouldBeNull();
    }

    [Fact]
    public void ToolbarParameterOverridesTheSettingsLayout()
    {
        var cut = RenderEditor(parameters => parameters
            .Add(editor => editor.Settings, new EditorSettings { ToolbarLayout = ToolbarLayout.Simple })
            .Add(editor => editor.Toolbar, new ToolbarLayout(new ToolbarItemRef("undo"))));

        cut.FindAll(".rte-toolbar button").ShouldHaveSingleItem().ClassList.ShouldContain("rte-btn-undo");
        cut.Find(".rte-toolbar > div").ClassList.ShouldContain("rte-toolbar-single-item");
    }

    [Fact]
    public void UnknownToolbarIdsAreSkipped()
    {
        var cut = RenderEditor(parameters => parameters.Add(editor => editor.Toolbar, new ToolbarLayout(new ToolbarGroup("bold", "nope"))));

        cut.FindAll(".rte-toolbar button").ShouldHaveSingleItem().ClassList.ShouldContain("rte-btn-bold");
    }

    [Fact]
    public void NeverRegisteredIdsLogAWarningAndRemovedIdsLogDebugOnce()
    {
        var logger = Substitute.For<ILogger<ZauberRichTextEditor>>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        Services.AddSingleton(logger);
        Services.AddZauberRte(options => options.RemoveToolbarItem("italic"));
        var layout = new ToolbarLayout(new ToolbarGroup("bold", "nope", "ITALIC"));

        var cut = RenderEditor(parameters => parameters.Add(editor => editor.Toolbar, layout));
        cut.Render(parameters => parameters.Add(editor => editor.Toolbar, layout));

        var logs = SubstituteCalls.Logs(logger);
        logs.Length.ShouldBe(2);
        logs.ShouldContain(log => log.Level == LogLevel.Warning && log.Message.Contains("nope") && !log.Message.Contains("ITALIC"));
        logs.ShouldContain(log => log.Level == LogLevel.Debug && log.Message.Contains("ITALIC") && !log.Message.Contains("nope"));
    }
}
