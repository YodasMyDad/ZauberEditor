using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Tests.Models;

public sealed class HtmlPolicyTagAttributesTests
{
    [Fact]
    public void TagAttributesAreEmptyByDefault() => new HtmlPolicy().TagAttributes.ShouldBeEmpty();

    [Fact]
    public void LookupsIgnoreCaseAndMergeDuplicateTags()
    {
        var policy = new HtmlPolicy
        {
            TagAttributes = new Dictionary<string, IReadOnlySet<string>>
            {
                ["A"] = new HashSet<string> { "target" },
                ["a"] = new HashSet<string> { "download" }
            }
        };

        policy.TagAttributes["a"].ShouldBe(["target", "download"], ignoreOrder: true);
        policy.TagAttributes["a"].Contains("TARGET").ShouldBeTrue();
    }

    [Fact]
    public void PoliciesWithEqualTagAttributesAreEqual()
    {
        var first = Policy(("a", ["target"]));
        var second = Policy(("A", ["TARGET"]));

        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }

    [Fact]
    public void ChangedTagAttributesMakePoliciesDifferent()
    {
        Policy(("a", ["target"])).ShouldNotBe(Policy(("a", ["download"])));
        Policy(("a", ["target"])).ShouldNotBe(Policy(("img", ["target"])));
        Policy(("a", ["target"])).ShouldNotBe(new HtmlPolicy());
    }

    [Fact]
    public void NullIsRejected() =>
        Should.Throw<ArgumentNullException>(() => new HtmlPolicy { TagAttributes = null! });

    private static HtmlPolicy Policy(params (string Tag, string[] Attributes)[] entries) => new()
    {
        TagAttributes = entries.ToDictionary(entry => entry.Tag, IReadOnlySet<string> (entry) => new HashSet<string>(entry.Attributes))
    };
}
