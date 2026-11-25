using Microsoft.AspNetCore.Components;

namespace ZauberRTE.Web.Client.Components;

public sealed partial class HtmlPreview
{
    [Parameter, EditorRequired]
    public string? Html { get; set; }
}
