using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Interop;

internal sealed record HtmlPolicyOptions(
    IReadOnlyList<string> AllowedTags,
    IReadOnlyList<string> AllowedAttributes,
    IReadOnlyDictionary<string, string[]> TagAttributes,
    bool AllowDataUrls,
    bool AllowExternalImages)
{
    public static HtmlPolicyOptions From(HtmlPolicy policy) => new(
        NameList.Normalise(policy.AllowedTags),
        NameList.Normalise(policy.AllowedAttributes),
        policy.TagAttributes.ToDictionary(pair => pair.Key.ToLowerInvariant(), pair => NameList.Normalise(pair.Value)),
        policy.AllowDataUrls,
        policy.AllowExternalImages);
}
