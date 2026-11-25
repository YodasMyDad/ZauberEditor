using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace ZauberCMS.RTE.Interop;

internal sealed class EditorInteropFactory(IJSRuntime jsRuntime, ILogger<EditorInterop> logger)
    : IEditorInteropFactory, IAsyncDisposable
{
    public const string ModulePath = "./_content/ZauberCMS.RTE/js/zauber-rte.js";

    private Task<IJSObjectReference?>? _module;

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(EditorInitOptions))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(HtmlPolicyOptions))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(ImageOptions))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(ShortcutBinding))]
    public async Task<IEditorInterop?> CreateAsync(
        ElementReference root,
        ElementReference content,
        ElementReference source,
        ElementReference toolbar,
        DotNetObjectReference<EditorCallbacks> callbacks,
        EditorInitOptions options)
    {
        if (await ImportAsync() is not { } module)
        {
            return null;
        }

        var instance = await EditorInterop.GuardAsync(
            module.InvokeAsync<IJSObjectReference>,
            "createEditor",
            [root, content, source, toolbar, callbacks, options],
            logger,
            CancellationToken.None);

        return instance is null ? null : new EditorInterop(instance, logger);
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not { IsCompletedSuccessfully: true } || await _module is not { } module)
        {
            return;
        }

        try
        {
            await module.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
            return;
        }
    }

    private async Task<IJSObjectReference?> ImportAsync()
    {
        var import = _module ??= EditorInterop.GuardAsync(jsRuntime.InvokeAsync<IJSObjectReference>, "import", [ModulePath], logger, CancellationToken.None);

        await ((Task)import).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        if ((!import.IsCompletedSuccessfully || await import is null) && ReferenceEquals(_module, import))
        {
            _module = null;
        }

        return await import;
    }
}
