using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.RegularExpressions;

namespace ZauberRTE.Web.Models;

[AttributeUsage(AttributeTargets.Property)]
public sealed partial class RequiredContentAttribute : ValidationAttribute
{
    public override bool IsValid(object? value) =>
        value is string html && !string.IsNullOrWhiteSpace(WebUtility.HtmlDecode(Tags().Replace(html, string.Empty)));

    [GeneratedRegex("<[^>]*>")]
    private static partial Regex Tags();
}
