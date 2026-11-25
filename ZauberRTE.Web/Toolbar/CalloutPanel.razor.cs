using ZauberCMS.RTE.Models;

namespace ZauberRTE.Web.Toolbar;

public sealed partial class CalloutPanel
{
    private static readonly string[] Styles = ["info", "tip", "warning"];

    private string _text = string.Empty;
    private string _style = Styles[0];

    public override string Title => "Insert callout";

    private async Task InsertAsync()
    {
        if (string.IsNullOrWhiteSpace(_text))
        {
            return;
        }

        var callout = HtmlBuilder.Element("aside")
            .Class($"callout callout-{_style}")
            .Child(HtmlBuilder.Element("p").Text(_text.Trim()));

        await Api.RestoreSelectionAsync();
        await Api.InsertHtmlAsync(callout.Build());
        await CloseAsync();
    }

    private async Task CancelAsync()
    {
        await Api.ClearSavedSelectionAsync();
        await CloseAsync();
    }
}
