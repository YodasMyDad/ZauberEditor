using System.Diagnostics;
using Microsoft.AspNetCore.Components;

namespace ZauberRTE.Web.Components.Pages;

public sealed partial class ErrorPage
{
    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    private string? RequestId { get; set; }

    private bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    protected override void OnInitialized() => RequestId = Activity.Current?.Id ?? HttpContext?.TraceIdentifier;
}
