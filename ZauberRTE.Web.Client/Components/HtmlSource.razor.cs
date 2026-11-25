using Microsoft.AspNetCore.Components;

namespace ZauberRTE.Web.Client.Components;

public sealed partial class HtmlSource
{
    [Parameter, EditorRequired]
    public string? Html { get; set; }

    [Parameter]
    public string Label { get; set; } = "Bound value";

    private string Size => ByteSize.Format(Html);
}
