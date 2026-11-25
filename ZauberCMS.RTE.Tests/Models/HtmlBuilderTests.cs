using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Tests.Models;

public sealed class HtmlBuilderTests
{
    [Theory]
    [InlineData("\" onerror=\"alert(1)", "&quot; onerror=&quot;alert(1)")]
    [InlineData("\"><script>alert(1)</script>", "&quot;&gt;&lt;script&gt;alert(1)&lt;/script&gt;")]
    [InlineData("a & b", "a &amp; b")]
    [InlineData("it's", "it&#39;s")]
    public void AttributeValuesAreEncoded(string value, string encoded) =>
        HtmlBuilder.Element("span").Attr("title", value).Build().ShouldBe($"<span title=\"{encoded}\"></span>");

    [Theory]
    [InlineData("<script>alert(1)</script>", "&lt;script&gt;alert(1)&lt;/script&gt;")]
    [InlineData("Fish & chips", "Fish &amp; chips")]
    [InlineData("\"quoted\"", "&quot;quoted&quot;")]
    public void TextIsEncoded(string text, string encoded) =>
        HtmlBuilder.Element("p").Text(text).Build().ShouldBe($"<p>{encoded}</p>");

    [Theory]
    [InlineData("img onerror=alert(1)")]
    [InlineData("1h")]
    [InlineData("p>")]
    [InlineData("a\"")]
    [InlineData("-x")]
    [InlineData(" ")]
    [InlineData("")]
    public void InvalidTagNamesAreRejected(string name) =>
        Should.Throw<ArgumentException>(() => HtmlBuilder.Element(name));

    [Theory]
    [InlineData("on click")]
    [InlineData("title=\"x\"")]
    [InlineData("a>")]
    [InlineData("/x")]
    [InlineData("")]
    public void InvalidAttributeNamesAreRejected(string name) =>
        Should.Throw<ArgumentException>(() => HtmlBuilder.Element("p").Attr(name, "x"));

    [Fact]
    public void NamesAreLowerCasedAndValidNamesAccepted() =>
        HtmlBuilder.Element("DIV").Attr("Data-Item_Id", "1").Attr("xml:lang", "en").Build()
            .ShouldBe("<div data-item_id=\"1\" xml:lang=\"en\"></div>");

    [Fact]
    public void NullAttributeValueIsLeftOut() =>
        HtmlBuilder.Element("a").Attr("title", null).Build().ShouldBe("<a></a>");

    [Fact]
    public void SettingAnAttributeAgainReplacesIt() =>
        HtmlBuilder.Element("p").Class("one").Attr("CLASS", "two").Build().ShouldBe("<p class=\"two\"></p>");

    [Fact]
    public void AttrsSetsEveryAttribute() =>
        HtmlBuilder.Element("td").Attrs(new Dictionary<string, string> { ["colspan"] = "2", ["scope"] = "row" }).Build()
            .ShouldBe("<td colspan=\"2\" scope=\"row\"></td>");

    [Fact]
    public void VoidElementsHaveNoClosingTagOrChildren() =>
        HtmlBuilder.Element("br").Text("ignored").Build().ShouldBe("<br>");

    [Fact]
    public void ChildrenAreNestedInOrder() =>
        HtmlBuilder.Element("ul")
            .Child(HtmlBuilder.Element("li").Text("one"))
            .Child(HtmlBuilder.Element("li").Text("two"))
            .ToString()
            .ShouldBe("<ul><li>one</li><li>two</li></ul>");

    [Fact]
    public void LinkEncodesHrefAndText() =>
        HtmlBuilder.Link("/search?q=a&b", "<b>x</b>").Build().ShouldBe("<a href=\"/search?q=a&amp;b\">&lt;b&gt;x&lt;/b&gt;</a>");

    [Fact]
    public void ImageWithoutAltLeavesItOut() =>
        HtmlBuilder.Image("/a.png").Build().ShouldBe("<img src=\"/a.png\">");

    [Fact]
    public void FigureWrapsImageAndCaption() =>
        HtmlBuilder.Figure("/a.png", "A", "Caption & more").Build()
            .ShouldBe("<figure class=\"rte-figure\"><img src=\"/a.png\" alt=\"A\"><figcaption>Caption &amp; more</figcaption></figure>");

    [Fact]
    public void NullArgumentsAreRejected()
    {
        var builder = HtmlBuilder.Element("p");

        Should.Throw<ArgumentNullException>(() => builder.Text(null!));
        Should.Throw<ArgumentNullException>(() => builder.Child(null!));
        Should.Throw<ArgumentNullException>(() => builder.Attrs(null!));
    }
}
