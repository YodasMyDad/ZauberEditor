using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Microsoft.JSInterop;
using ZauberCMS.RTE.Components;
using ZauberCMS.RTE.Interop;
using ZauberCMS.RTE.Services;

namespace ZauberCMS.RTE.Tests.Components;

public abstract class EditorTestContext : BunitContext
{
    private Task<IEditorInterop?> _creation;

    protected EditorTestContext()
    {
        _creation = Task.FromResult<IEditorInterop?>(Interop);
        Factory.CreateAsync(Arg.Any<ElementReference>(), Arg.Any<ElementReference>(), Arg.Any<ElementReference>(), Arg.Any<ElementReference>(), Arg.Any<DotNetObjectReference<EditorCallbacks>>(), Arg.Any<EditorInitOptions>())
            .Returns(call =>
            {
                CallbackReference = call.ArgAt<DotNetObjectReference<EditorCallbacks>>(4);
                InitOptions = call.ArgAt<EditorInitOptions>(5);
                return _creation;
            });
        Services.AddSingleton<TimeProvider>(Time);
        Services.AddScoped(_ => Factory);
        Services.AddZauberRte();
    }

    internal IEditorInterop Interop { get; } = Substitute.For<IEditorInterop>();

    internal IEditorInteropFactory Factory { get; } = Substitute.For<IEditorInteropFactory>();

    internal FakeTimeProvider Time { get; } = new();

    internal DotNetObjectReference<EditorCallbacks>? CallbackReference { get; private set; }

    internal EditorCallbacks Callbacks => CallbackReference?.Value ?? throw new InvalidOperationException("The editor has not been initialised.");

    internal EditorInitOptions? InitOptions { get; private set; }

    internal TaskCompletionSource<IEditorInterop?> DeferCreation()
    {
        var creation = new TaskCompletionSource<IEditorInterop?>(TaskCreationOptions.RunContinuationsAsynchronously);
        _creation = creation.Task;
        return creation;
    }

    internal IRenderedComponent<ZauberRichTextEditor> RenderEditor(
        Action<ComponentParameterCollectionBuilder<ZauberRichTextEditor>>? parameters = null) =>
        Render<ZauberRichTextEditor>(parameters ?? (static _ => { }));
}
