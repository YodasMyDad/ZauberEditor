using ZauberCMS.RTE.Components;
using ZauberCMS.RTE.Models;

namespace ZauberRTE.Web.Components.Pages;

public sealed partial class HostCss
{
    private static readonly Theme[] Themes = [Theme.Light, Theme.Dark];

    private ZauberRichTextEditor? _full;
    private Theme _theme = Theme.Light;
    private string? _html = """
        <h1>Heading one</h1>
        <p>A paragraph with <strong>bold</strong>, <em>italic</em>, <code>inline code</code>, <mark>marked</mark> text, H<sub>2</sub>O, x<sup>2</sup> and <a href="https://example.com">a link</a>.</p>
        <h2>Heading two</h2>
        <ul><li>First item</li><li>Second item<ul><li>Nested item</li></ul></li></ul>
        <ol><li>One</li><li>Two</li></ol>
        <h3>Heading three</h3>
        <blockquote><p>A quotation.</p></blockquote>
        <table><thead><tr><th>Name</th><th>Value</th></tr></thead><tbody><tr><td>Alpha</td><td>1</td></tr><tr><td>Beta</td><td>2</td></tr></tbody></table>
        <pre><code>const answer = 42;</code></pre>
        <p><img src="favicon.png" alt="Logo" width="48" height="48"></p>
        <hr>
        <p>The end.</p>
        """;
    private string? _neutralHtml = "<p>Neutral content uses the <strong>system font</strong>.</p><ul><li>List item</li></ul>";

    private Task ShowToastAsync() => _full?.Api.ShowToastAsync("Saved to the server.", ToastType.Success) ?? Task.CompletedTask;
}
