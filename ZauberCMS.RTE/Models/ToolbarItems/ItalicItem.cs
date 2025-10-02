namespace ZauberCMS.RTE.Models.ToolbarItems;

/// <summary>
/// Toolbar item for italic formatting
/// </summary>
public class ItalicItem : ToolbarItemBase
{
    public override string Id => "italic";
    public override string Label => "Italic";
    public override string Tooltip => "Italic (Ctrl+I)";
    public override string IconCss => "fa-italic";
    public override string Shortcut => "Control+i";
    public override ToolbarPlacement Placement => ToolbarPlacement.Inline;
    public override bool IsToggle => true;
    public override string[] TrackedTags => ["em", "i"];
    public override string PrimaryTag => "em";

    public override bool IsActive(EditorState state) => state.ActiveMarks.Contains("em");
    public override Task ExecuteAsync(IEditorApi api) => api.ToggleMarkAsync("em");
}
