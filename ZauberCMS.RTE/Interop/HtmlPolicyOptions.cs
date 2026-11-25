using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Interop;

internal sealed record HtmlPolicyOptions(
    IReadOnlyList<string> AllowedTags,
    IReadOnlyList<string> AllowedAttributes,
    bool AllowDataUrls,
    bool AllowExternalImages)
{
    public static HtmlPolicyOptions From(HtmlPolicy policy) => new(
        NameList.Normalise(policy.AllowedTags),
        NameList.Normalise(policy.AllowedAttributes),
        policy.AllowDataUrls,
        policy.AllowExternalImages);
}
