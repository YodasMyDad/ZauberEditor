namespace ZauberCMS.RTE.Models;

/// <summary>Configuration for an editor instance. Pass a new instance to apply changes.</summary>
public sealed record EditorSettings
{
    /// <summary>The toolbar layout used when the component's <c>Toolbar</c> parameter is not set.</summary>
    public ToolbarLayout ToolbarLayout { get; init; } = ToolbarLayout.Full;

    /// <summary>Editor size.</summary>
    public EditorDimensions Dimensions { get; init; } = new();

    /// <summary>Image insertion and resizing rules.</summary>
    public ImageConstraints ImageConstraints { get; init; } = new();

    /// <summary>The HTML allowed in the content.</summary>
    public HtmlPolicy HtmlPolicy { get; init; } = new();

    /// <summary>Maximum number of undo steps.</summary>
    public int HistoryLimit { get; init; } = 100;

    /// <summary>How long typing must pause before the bound value updates.</summary>
    public TimeSpan ChangeDelay { get; init; } = TimeSpan.FromMilliseconds(250);

    /// <summary>Largest content, in UTF-8 bytes, the editor will transfer to .NET.</summary>
    public long MaxContentBytes { get; init; } = 10 * 1024 * 1024;
}
