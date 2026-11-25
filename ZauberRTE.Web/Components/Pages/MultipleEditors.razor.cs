using ZauberCMS.RTE.Models;
using ZauberRTE.Web.Toolbar;

namespace ZauberRTE.Web.Components.Pages;

public sealed partial class MultipleEditors
{
    private readonly Sample[] _samples =
    [
        new("Simple", new() { ToolbarLayout = ToolbarLayout.Simple }, "<p>The <strong>simple</strong> layout.</p>"),
        new("Full", new() { ToolbarLayout = ToolbarLayout.Full }, "<p>The <em>full</em> layout.</p>"),
        new("Custom", DemoToolbar.Settings, "<p>A <mark>custom</mark> layout with the demo's own items.</p>")
    ];

    private sealed class Sample(string title, EditorSettings settings, string html)
    {
        public string Title => title;

        public EditorSettings Settings => settings;

        public string? Html { get; set; } = html;
    }
}
