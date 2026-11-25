using ZauberCMS.RTE.Components.Internal;

namespace ZauberCMS.RTE.Tests.Components.Internal;

public sealed class UrlRulesTests
{
    [Theory]
    [InlineData("https://example.com")]
    [InlineData("HTTP://EXAMPLE.COM")]
    [InlineData("  https://example.com  ")]
    [InlineData("mailto:someone@example.com")]
    [InlineData("tel:+441234567890")]
    [InlineData("/relative/path")]
    [InlineData("page.html")]
    [InlineData("page?time=10:30")]
    [InlineData("#section:2")]
    [InlineData("folder/file:name")]
    [InlineData("//cdn.example.com/a.png")]
    [InlineData("")]
    public void LinkAllowsWebMailPhoneAndRelativeUrls(string url) =>
        UrlRules.HasAllowedScheme(url, UrlRules.LinkSchemes).ShouldBeTrue();

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("  JavaScript:alert(1)")]
    [InlineData("vbscript:msgbox(1)")]
    [InlineData("data:text/html;base64,PHNjcmlwdD4=")]
    [InlineData("file:///etc/passwd")]
    [InlineData("ftp://example.com")]
    [InlineData("java\tscript:alert(1)")]
    public void LinkRejectsOtherSchemes(string url) =>
        UrlRules.HasAllowedScheme(url, UrlRules.LinkSchemes).ShouldBeFalse();

    [Theory]
    [InlineData("https://example.com/a.png", true)]
    [InlineData("/images/a.png", true)]
    [InlineData("mailto:someone@example.com", false)]
    [InlineData("tel:123", false)]
    [InlineData("data:image/svg+xml,<svg onload=alert(1)>", false)]
    [InlineData("javascript:alert(1)", false)]
    public void ImageAllowsOnlyWebAndRelativeUrls(string url, bool allowed) =>
        UrlRules.HasAllowedScheme(url, UrlRules.ImageSchemes).ShouldBe(allowed);
}
