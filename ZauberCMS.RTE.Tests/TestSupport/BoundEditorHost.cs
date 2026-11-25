using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using ZauberCMS.RTE.Components;

namespace ZauberCMS.RTE.Tests.TestSupport;

internal sealed class BoundEditorHost : ComponentBase
{
    private bool _initialised;

    [Parameter]
    public string? InitialHtml { get; set; }

    [Parameter]
    public Func<string?, Task>? BeforeStoring { get; set; }

    public string? Html { get; private set; }

    protected override void OnParametersSet()
    {
        if (!_initialised)
        {
            _initialised = true;
            Html = InitialHtml;
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<ZauberRichTextEditor>(0);
        builder.AddComponentParameter(1, nameof(ZauberRichTextEditor.Value), Html);
        builder.AddComponentParameter(2, nameof(ZauberRichTextEditor.ValueChanged), EventCallback.Factory.Create<string?>(this, StoreAsync));
        builder.CloseComponent();
    }

    private async Task StoreAsync(string? html)
    {
        if (BeforeStoring is not null)
        {
            await BeforeStoring(html);
        }

        Html = html;
    }
}
