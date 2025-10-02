using System.Text;

namespace ZauberCMS.RTE.Models;

/// <summary>
/// Fluent helper for building HTML elements with attributes
/// </summary>
public class HtmlBuilder
{
    private readonly string _tagName;
    private readonly Dictionary<string, string> _attributes = new();
    private string? _content;
    private readonly List<HtmlBuilder> _children = new();

    private HtmlBuilder(string tagName)
    {
        _tagName = tagName;
    }

    /// <summary>
    /// Creates a new HTML element builder
    /// </summary>
    public static HtmlBuilder Element(string tagName) => new(tagName);

    /// <summary>
    /// Creates an anchor (link) element
    /// </summary>
    public static HtmlBuilder Link(string href, string text) =>
        Element("a").Attr("href", href).Text(text);

    /// <summary>
    /// Creates an image element
    /// </summary>
    public static HtmlBuilder Image(string src, string? alt = null)
    {
        var builder = Element("img").Attr("src", src);
        if (!string.IsNullOrEmpty(alt))
            builder.Attr("alt", alt);
        return builder;
    }

    /// <summary>
    /// Creates a figure with image and caption
    /// </summary>
    public static HtmlBuilder Figure(string src, string? alt, string caption)
    {
        var img = Image(src, alt);
        var figcaption = Element("figcaption").Text(caption);
        return Element("figure").Class("rte-figure").Child(img).Child(figcaption);
    }

    /// <summary>
    /// Adds an attribute to the element
    /// </summary>
    public HtmlBuilder Attr(string name, string value)
    {
        _attributes[name] = value;
        return this;
    }

    /// <summary>
    /// Adds a CSS class to the element
    /// </summary>
    public HtmlBuilder Class(string className) => Attr("class", className);

    /// <summary>
    /// Adds an ID to the element
    /// </summary>
    public HtmlBuilder Id(string id) => Attr("id", id);

    /// <summary>
    /// Adds a style attribute to the element
    /// </summary>
    public HtmlBuilder Style(string style) => Attr("style", style);

    /// <summary>
    /// Adds multiple attributes from a dictionary
    /// </summary>
    public HtmlBuilder Attrs(Dictionary<string, string> attributes)
    {
        foreach (var (key, value) in attributes)
            _attributes[key] = value;
        return this;
    }

    /// <summary>
    /// Sets the text content (will be HTML encoded)
    /// </summary>
    public HtmlBuilder Text(string text)
    {
        _content = System.Net.WebUtility.HtmlEncode(text);
        return this;
    }

    /// <summary>
    /// Sets raw HTML content (will NOT be encoded)
    /// </summary>
    public HtmlBuilder Html(string html)
    {
        _content = html;
        return this;
    }

    /// <summary>
    /// Adds a child element
    /// </summary>
    public HtmlBuilder Child(HtmlBuilder child)
    {
        _children.Add(child);
        return this;
    }

    /// <summary>
    /// Builds the HTML string
    /// </summary>
    public string Build()
    {
        var sb = new StringBuilder();
        sb.Append('<').Append(_tagName);

        foreach (var (key, value) in _attributes)
        {
            sb.Append(' ').Append(key).Append("=\"").Append(value).Append('"');
        }

        // Self-closing tags
        if (IsSelfClosing(_tagName) && _content == null && _children.Count == 0)
        {
            sb.Append(" />");
            return sb.ToString();
        }

        sb.Append('>');

        // Add content
        if (_content != null)
            sb.Append(_content);

        // Add children
        foreach (var child in _children)
            sb.Append(child.Build());

        sb.Append("</").Append(_tagName).Append('>');

        return sb.ToString();
    }

    /// <summary>
    /// Implicitly converts to string
    /// </summary>
    public static implicit operator string(HtmlBuilder builder) => builder.Build();

    /// <summary>
    /// Returns the built HTML string
    /// </summary>
    public override string ToString() => Build();

    private static bool IsSelfClosing(string tagName) =>
        tagName is "img" or "br" or "hr" or "input" or "meta" or "link";
}

