using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Endpoints;

namespace ZauberRTE.Web.Components;

public sealed partial class App
{
    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    private IComponentRenderMode? PageRenderMode =>
        HttpContext?.GetEndpoint()?.Metadata.GetMetadata<ComponentTypeMetadata>()?.Type.GetCustomAttribute<RenderModeAttribute>()?.Mode;
}
