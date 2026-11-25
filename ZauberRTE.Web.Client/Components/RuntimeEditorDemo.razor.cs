using Microsoft.AspNetCore.Components;

namespace ZauberRTE.Web.Client.Components;

public sealed partial class RuntimeEditorDemo
{
    private string? _html = "<p>This editor's C# runs in the browser when the renderer below says <strong>WebAssembly</strong>.</p>";

    [Parameter, EditorRequired]
    public string Heading { get; set; } = string.Empty;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
