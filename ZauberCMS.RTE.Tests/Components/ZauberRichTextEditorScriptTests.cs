using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Microsoft.JSInterop;
using ZauberCMS.RTE.Components;
using ZauberCMS.RTE.Interop;
using ZauberCMS.RTE.Services;

namespace ZauberCMS.RTE.Tests.Components;

public sealed class ZauberRichTextEditorScriptTests : BunitContext
{
    private readonly BunitJSModuleInterop _module;
    private readonly BunitJSModuleInterop _instance;

    public ZauberRichTextEditorScriptTests()
    {
        Services.AddSingleton<TimeProvider>(new FakeTimeProvider());
        Services.AddZauberRte();
        _module = JSInterop.SetupModule(EditorInteropFactory.ModulePath);
        _instance = _module.SetupModule(invocation => invocation.Identifier == "createEditor");
    }

    [Fact]
    public void ImportsTheModuleAndCreatesTheEditorWithItsElements()
    {
        var cut = Render<ZauberRichTextEditor>(parameters => parameters.Add(editor => editor.Value, "<p>Hello</p>"));

        var create = _module.VerifyInvoke("createEditor");
        create.Arguments[0].ShouldBeElementReferenceTo(cut.Find("div.zauber-rte"));
        create.Arguments[1].ShouldBeElementReferenceTo(cut.Find(".rte-content"));
        create.Arguments[2].ShouldBeElementReferenceTo(cut.Find(".rte-source-view"));
        create.Arguments[3].ShouldBeElementReferenceTo(cut.Find(".rte-toolbar"));
        create.Arguments[4].ShouldBeOfType<DotNetObjectReference<EditorCallbacks>>();
        create.Arguments[5].ShouldBeOfType<EditorInitOptions>().Html.ShouldBe("<p>Hello</p>");
    }

    [Fact]
    public async Task ToolbarCommandsReachTheScriptInstance()
    {
        _instance.SetupVoid("toggleMark", "strong").SetVoidResult();
        var cut = Render<ZauberRichTextEditor>();

        await cut.Find(".rte-btn-bold").ClickAsync(new());

        _instance.VerifyInvoke("toggleMark").Arguments.ShouldBe(["strong"]);
    }

    [Fact]
    public async Task DisposeTearsDownTheScriptInstanceAndReleasesCallbacks()
    {
        _instance.SetupVoid("dispose").SetVoidResult();
        Render<ZauberRichTextEditor>();
        var callbacks = _module.VerifyInvoke("createEditor").Arguments[4].ShouldBeOfType<DotNetObjectReference<EditorCallbacks>>();

        await DisposeComponentsAsync();

        _instance.VerifyInvoke("dispose");
        Should.Throw<ObjectDisposedException>(() => callbacks.Value);
    }

    [Fact]
    public async Task DisposeAfterTheCircuitHasGoneDoesNotThrow()
    {
        _instance.SetupVoid("dispose").SetException(new JSDisconnectedException("The circuit has gone."));
        Render<ZauberRichTextEditor>();
        var callbacks = _module.VerifyInvoke("createEditor").Arguments[4].ShouldBeOfType<DotNetObjectReference<EditorCallbacks>>();

        await Should.NotThrowAsync(DisposeComponentsAsync);

        _instance.VerifyInvoke("dispose");
        Should.Throw<ObjectDisposedException>(() => callbacks.Value);
    }
}
