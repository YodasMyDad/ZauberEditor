using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ZauberCMS.RTE.Interop;

internal interface IEditorInteropFactory
{
    Task<IEditorInterop?> CreateAsync(
        ElementReference root,
        ElementReference content,
        ElementReference source,
        ElementReference toolbar,
        DotNetObjectReference<EditorCallbacks> callbacks,
        EditorInitOptions options);
}
