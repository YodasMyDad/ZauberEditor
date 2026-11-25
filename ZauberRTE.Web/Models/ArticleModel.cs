namespace ZauberRTE.Web.Models;

public sealed class ArticleModel
{
    [RequiredContent(ErrorMessage = "Write some text before saving.")]
    public string? Body { get; set; }
}
