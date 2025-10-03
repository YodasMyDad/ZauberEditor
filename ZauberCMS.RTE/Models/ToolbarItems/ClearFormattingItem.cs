namespace ZauberCMS.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for cleaning HTML completely
/// </summary>
public class ClearFormattingItem : ToolbarItemBase
{
    public override string Id => "clean-html";
    public override string Label => "Clean Html";
    public override string IconCss => "fa-eraser";
    public override ToolbarPlacement Placement => ToolbarPlacement.Inline;

    public override Task ExecuteAsync(IEditorApi api) => api.CleanHtmlAsync();
}
