using ZauberCMS.RTE.Models;

namespace ZauberRTE.Web.Components.Pages;

public sealed partial class CustomItems
{
    private string? _html = """
        <p>Select some of this text and <mark>highlight</mark> it, give it a <span class="ink-blue">colour</span>, or insert a callout below.</p>
        <aside class="callout callout-tip"><p>Callouts are inserted by a custom panel.</p></aside>
        """;
    private string? _lastCommand;

    private void ShowCommand(CommandExecutedArgs args) => _lastCommand = args.ItemId;
}
