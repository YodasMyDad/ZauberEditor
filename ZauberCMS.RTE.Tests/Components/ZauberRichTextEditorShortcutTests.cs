using Bunit;
using ZauberCMS.RTE.Components;
using ZauberCMS.RTE.Interop;
using ZauberCMS.RTE.Models;
using ZauberCMS.RTE.Services;
using ZauberCMS.RTE.Tests.TestSupport;

namespace ZauberCMS.RTE.Tests.Components;

public sealed class ZauberRichTextEditorShortcutTests : EditorTestContext
{
    private const string SaveId = "Ctrl+S";

    private readonly List<string> _executed = [];
    private readonly List<CommandFailedArgs> _failures = [];
    private readonly List<(IEditorApi Api, CancellationToken Token)> _saves = [];

    [Fact]
    public void ShortcutsAreSentToTheScriptAfterTheItemShortcuts()
    {
        RenderWithShortcuts([new("Control+s", SaveAsync)]);

        var options = InitOptions.ShouldNotBeNull();
        options.Shortcuts[^1].ShouldBe(new ShortcutBinding(SaveId, "s", true, false, false, Custom: true));
        options.Shortcuts.Count(binding => !binding.Custom).ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task PressingTheShortcutRunsTheHandlerWithTheEditorsApi()
    {
        var cut = RenderWithShortcuts([new("Control+s", SaveAsync)]);

        await Callbacks.OnShortcutAsync(SaveId, true);

        var (api, token) = _saves.ShouldHaveSingleItem();
        api.ShouldBeSameAs(cut.Instance.Api);
        token.IsCancellationRequested.ShouldBeFalse();
        _executed.ShouldBe(["Control+s"]);
    }

    [Fact]
    public async Task ShortcutsAreIgnoredWhileReadOnly()
    {
        RenderWithShortcuts([new("Control+s", SaveAsync)], readOnly: true);

        await Callbacks.OnShortcutAsync(SaveId, true);

        _saves.ShouldBeEmpty();
        _executed.ShouldBeEmpty();
    }

    [Fact]
    public async Task UnknownShortcutIsIgnored()
    {
        RenderWithShortcuts([new("Control+s", SaveAsync)]);

        await Callbacks.OnShortcutAsync("Ctrl+Q", true);
        await Callbacks.OnShortcutAsync(SaveId, false);

        _saves.ShouldBeEmpty();
    }

    [Fact]
    public async Task TheLastShortcutWithTheSameKeysWins()
    {
        var first = 0;
        RenderWithShortcuts(
        [
            new("Control+s", (_, _) => Task.FromResult(first++)),
            new("Ctrl+S", SaveAsync)
        ]);

        await Callbacks.OnShortcutAsync(SaveId, true);

        first.ShouldBe(0);
        _saves.ShouldHaveSingleItem();
        InitOptions.ShouldNotBeNull().Shortcuts.Count(binding => binding.Custom).ShouldBe(1);
    }

    [Fact]
    public async Task FailingHandlerIsLoggedToastedAndRaisesOnCommandFailed()
    {
        var failure = new InvalidOperationException("Save failed.");
        var cut = RenderWithShortcuts([new("Control+s", (_, _) => Task.FromException(failure))]);

        await Callbacks.OnShortcutAsync(SaveId, true);

        cut.Find(".rte-toast-error").TextContent.ShouldContain("didn't work");
        _failures.ShouldHaveSingleItem().ShouldBe(new CommandFailedArgs("Control+s", failure));
        _executed.ShouldBeEmpty();
    }

    [Fact]
    public async Task FailingItemRaisesOnCommandFailedWithItsId()
    {
        Services.AddZauberRte(options => options.AddToolbarItem<FailingItem>());
        var cut = RenderWithShortcuts([], toolbar: new ToolbarLayout(new ToolbarItemRef(FailingItem.ItemId)));

        await cut.Find($".rte-btn-{FailingItem.ItemId}").ClickAsync(new());

        var failed = _failures.ShouldHaveSingleItem();
        failed.ItemId.ShouldBe(FailingItem.ItemId);
        failed.Exception.ShouldBeOfType<InvalidOperationException>();
    }

    [Fact]
    public async Task AFailingOnCommandFailedHandlerDoesNotBreakTheEditor()
    {
        var cut = RenderEditor(parameters => parameters
            .Add(editor => editor.Shortcuts, [new EditorShortcut("Control+s", (_, _) => throw new InvalidOperationException("Save failed."))])
            .Add(editor => editor.OnCommandFailed, (CommandFailedArgs _) => throw new InvalidOperationException("Handler failed.")));

        await Callbacks.OnShortcutAsync(SaveId, true);

        cut.FindAll(".rte-toast-error").ShouldNotBeEmpty();
    }

    [Fact]
    public async Task AnEqualListDoesNotReconfigureTheScript()
    {
        var cut = RenderWithShortcuts([new("Control+s", SaveAsync)]);

        cut.Render(parameters => parameters.Add(editor => editor.Shortcuts, [new EditorShortcut("Control+s", SaveAsync)]));

        await Interop.DidNotReceive().ConfigureAsync(Arg.Any<EditorInitOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ANewHandlerForTheSameKeysIsUsedWithoutReconfiguring()
    {
        var replaced = 0;
        var cut = RenderWithShortcuts([new("Control+s", SaveAsync)]);

        cut.Render(parameters => parameters.Add(editor => editor.Shortcuts, [new EditorShortcut("Control+s", (_, _) => Task.FromResult(replaced++))]));
        await Callbacks.OnShortcutAsync(SaveId, true);

        replaced.ShouldBe(1);
        _saves.ShouldBeEmpty();
        await Interop.DidNotReceive().ConfigureAsync(Arg.Any<EditorInitOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ChangedKeysReconfigureTheScript()
    {
        var cut = RenderWithShortcuts([new("Control+s", SaveAsync)]);

        cut.Render(parameters => parameters.Add(editor => editor.Shortcuts, [new EditorShortcut("Control+Shift+s", SaveAsync)]));

        await Interop.Received(1).ConfigureAsync(
            Arg.Is<EditorInitOptions>(options => options.Shortcuts.Any(binding => binding.Custom && binding.ItemId == "Ctrl+Shift+S")
                && !options.Shortcuts.Any(binding => binding.ItemId == SaveId)),
            Arg.Any<CancellationToken>());
        await Callbacks.OnShortcutAsync(SaveId, true);
        _saves.ShouldBeEmpty();
    }

    [Fact]
    public async Task RemovingTheShortcutsReconfiguresTheScript()
    {
        var cut = RenderWithShortcuts([new("Control+s", SaveAsync)]);

        cut.Render(parameters => parameters.Add(editor => editor.Shortcuts, null));

        await Interop.Received(1).ConfigureAsync(
            Arg.Is<EditorInitOptions>(options => !options.Shortcuts.Any(binding => binding.Custom)),
            Arg.Any<CancellationToken>());
    }

    private Task SaveAsync(IEditorApi api, CancellationToken cancellationToken)
    {
        _saves.Add((api, cancellationToken));
        return Task.CompletedTask;
    }

    private IRenderedComponent<ZauberRichTextEditor> RenderWithShortcuts(
        IReadOnlyList<EditorShortcut> shortcuts,
        bool readOnly = false,
        ToolbarLayout? toolbar = null) =>
        RenderEditor(parameters => parameters
            .Add(editor => editor.Shortcuts, shortcuts)
            .Add(editor => editor.Toolbar, toolbar)
            .Add(editor => editor.ReadOnly, readOnly)
            .Add(editor => editor.OnCommandExecuted, (CommandExecutedArgs args) => _executed.Add(args.ItemId))
            .Add(editor => editor.OnCommandFailed, (CommandFailedArgs args) => _failures.Add(args)));
}
