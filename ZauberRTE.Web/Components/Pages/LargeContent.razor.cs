using System.Text;
using ZauberCMS.RTE.Models;
using ZauberRTE.Web.Client.Components;

namespace ZauberRTE.Web.Components.Pages;

public sealed partial class LargeContent
{
    private const int TargetBytes = 200 * 1024;
    private const int ParagraphsPerSection = 4;
    private const string Filler =
        "Rich-text documents grow quickly once they hold real articles, tables and pasted content. This paragraph is " +
        "repeated to build a document that is far larger than a single SignalR message, so every edit you make here " +
        "is sent back to the server as a stream rather than inline.";

    private static readonly EditorSettings TallSettings = new() { Dimensions = new() { Height = "420px" } };

    private string? _html = "<p>Press <strong>Load</strong> to replace this with a large document.</p>";
    private string? _loadedSize;
    private int _changeCount;
    private ChangeSource? _lastSource;

    private void Load()
    {
        _html = BuildDocument();
        _loadedSize = ByteSize.Format(_html);
        (_changeCount, _lastSource) = (0, null);
    }

    private void Clear() => _html = string.Empty;

    private void CountChange(EditorChangeArgs change) => (_changeCount, _lastSource) = (_changeCount + 1, change.Source);

    private static string BuildDocument()
    {
        var html = new StringBuilder();
        for (var section = 1; html.Length < TargetBytes; section++)
        {
            html.Append(HtmlBuilder.Element("h2").Text($"Section {section}").Build());
            for (var paragraph = 1; paragraph <= ParagraphsPerSection; paragraph++)
            {
                html.Append(HtmlBuilder.Element("p").Text($"{section}.{paragraph} {Filler}").Build());
            }
        }

        return html.ToString();
    }
}
