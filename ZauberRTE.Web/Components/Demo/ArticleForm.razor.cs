using ZauberCMS.RTE.Models;
using ZauberRTE.Web.Client.Components;
using ZauberRTE.Web.Models;

namespace ZauberRTE.Web.Components.Demo;

public sealed partial class ArticleForm
{
    private static readonly EditorSettings CompactSettings = new() { Dimensions = new() { Height = "160px" } };

    private readonly ArticleModel _article = new();
    private string? _saved;

    private void Save() => _saved = $"Saved {ByteSize.Format(_article.Body)} of HTML.";
}
