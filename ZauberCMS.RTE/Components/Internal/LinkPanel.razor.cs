using System.ComponentModel;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Components.Internal;

/// <summary>Inserts or edits a link. Opened by the built-in link item; not for direct use.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed partial class LinkPanel
{
    private string _url = string.Empty;
    private string _text = string.Empty;
    private string _title = string.Empty;
    private string _target = string.Empty;
    private string _selectedText = string.Empty;
    private string? _error;

    /// <inheritdoc />
    public override string Title => "Link";

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        if (await Api.GetLinkAtCursorAsync() is { } link)
        {
            (_url, _text, _title, _target) = (link.Href, link.Text, link.Title ?? string.Empty, link.Target ?? string.Empty);
            _selectedText = link.Text;
            await Api.SelectLinkAtCursorAsync();
        }
        else if (await Api.GetSelectionAsync() is { IsCollapsed: false } selection)
        {
            (_text, _selectedText) = (selection.Text, selection.Text);
        }

        StateHasChanged();
    }

    private async Task SaveAsync()
    {
        var url = _url.Trim();
        if (url.Length == 0)
        {
            return;
        }

        if (!UrlRules.HasAllowedScheme(url, UrlRules.LinkSchemes))
        {
            _error = "Use a web address, a mailto: or tel: link, or a relative path.";
            return;
        }

        var attributes = LinkAttributes(url);
        if (_selectedText.Length > 0 && _text == _selectedText)
        {
            await Api.WrapSelectionAsync("a", attributes);
        }
        else
        {
            await Api.InsertHtmlAsync(HtmlBuilder.Link(url, string.IsNullOrWhiteSpace(_text) ? url : _text).Attrs(attributes).Build());
        }

        await CloseAsync();
    }

    private Dictionary<string, string> LinkAttributes(string url)
    {
        var target = NullIfEmpty(_target);
        KeyValuePair<string, string?>[] attributes =
        [
            new("href", url),
            new("title", NullIfEmpty(_title)),
            new("target", target),
            new("rel", target is "_blank" ? "noopener noreferrer" : null),
        ];
        return attributes.Where(attribute => attribute.Value is not null).ToDictionary(attribute => attribute.Key, attribute => attribute.Value!);
    }

    private static string? NullIfEmpty(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
