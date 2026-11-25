using System.Net;
using System.Text;

namespace ZauberCMS.RTE.Models;

/// <summary>Builds an HTML fragment with encoded text and attribute values.</summary>
public sealed class HtmlBuilder
{
    private readonly string? _tagName;
    private readonly string? _encodedText;
    private readonly List<KeyValuePair<string, string>> _attributes = [];
    private readonly List<HtmlBuilder> _children = [];

    private HtmlBuilder(string? tagName, string? encodedText)
    {
        _tagName = tagName;
        _encodedText = encodedText;
    }

    /// <summary>Starts an element.</summary>
    /// <param name="tagName">The element name, such as <c>p</c>.</param>
    public static HtmlBuilder Element(string tagName)
    {
        EnsureValidName(tagName);
        return new(tagName.ToLowerInvariant(), null);
    }

    /// <summary>Starts a link with text.</summary>
    /// <param name="href">The link target.</param>
    /// <param name="text">The link text.</param>
    public static HtmlBuilder Link(string href, string text) => Element("a").Attr("href", href).Text(text);

    /// <summary>Starts an image.</summary>
    /// <param name="src">The image source.</param>
    /// <param name="alt">The alternative text.</param>
    public static HtmlBuilder Image(string src, string? alt = null) => Element("img").Attr("src", src).Attr("alt", alt);

    /// <summary>Starts a figure containing an image and a caption.</summary>
    /// <param name="src">The image source.</param>
    /// <param name="alt">The alternative text.</param>
    /// <param name="caption">The caption text.</param>
    public static HtmlBuilder Figure(string src, string? alt, string caption) =>
        Element("figure").Class("rte-figure").Child(Image(src, alt)).Child(Element("figcaption").Text(caption));

    /// <summary>Sets an attribute; a null value leaves it out.</summary>
    /// <param name="name">The attribute name.</param>
    /// <param name="value">The attribute value, encoded on output.</param>
    public HtmlBuilder Attr(string name, string? value)
    {
        EnsureValidName(name);
        if (value is null)
        {
            return this;
        }

        var key = name.ToLowerInvariant();
        _attributes.RemoveAll(attribute => attribute.Key == key);
        _attributes.Add(new(key, value));
        return this;
    }

    /// <summary>Sets several attributes.</summary>
    /// <param name="attributes">The attributes to set.</param>
    public HtmlBuilder Attrs(IReadOnlyDictionary<string, string> attributes)
    {
        ArgumentNullException.ThrowIfNull(attributes);
        foreach (var (name, value) in attributes)
        {
            Attr(name, value);
        }

        return this;
    }

    /// <summary>Sets the <c>class</c> attribute.</summary>
    /// <param name="className">One or more class names.</param>
    public HtmlBuilder Class(string className) => Attr("class", className);

    /// <summary>Appends encoded text.</summary>
    /// <param name="text">The text.</param>
    public HtmlBuilder Text(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        _children.Add(new(null, WebUtility.HtmlEncode(text)));
        return this;
    }

    /// <summary>Appends a child element.</summary>
    /// <param name="child">The child element.</param>
    public HtmlBuilder Child(HtmlBuilder child)
    {
        ArgumentNullException.ThrowIfNull(child);
        _children.Add(child);
        return this;
    }

    /// <summary>Returns the HTML.</summary>
    public string Build() => AppendTo(new StringBuilder()).ToString();

    /// <summary>Returns the HTML.</summary>
    public override string ToString() => Build();

    private StringBuilder AppendTo(StringBuilder html)
    {
        if (_tagName is null)
        {
            return html.Append(_encodedText);
        }

        html.Append('<').Append(_tagName);
        foreach (var (name, value) in _attributes)
        {
            html.Append(' ').Append(name).Append("=\"").Append(WebUtility.HtmlEncode(value)).Append('"');
        }

        html.Append('>');
        if (IsVoid(_tagName))
        {
            return html;
        }

        foreach (var child in _children)
        {
            child.AppendTo(html);
        }

        return html.Append("</").Append(_tagName).Append('>');
    }

    private static bool IsVoid(string tagName) =>
        tagName is "area" or "br" or "col" or "embed" or "hr" or "img" or "input" or "source" or "wbr";

    private static void EnsureValidName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (!char.IsAsciiLetter(name[0]) || !name.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or ':'))
        {
            throw new ArgumentException($"'{name}' is not a valid HTML name.", nameof(name));
        }
    }
}
