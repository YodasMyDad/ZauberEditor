namespace ZauberCMS.RTE.Models;

/// <summary>What caused a content change.</summary>
public enum ChangeSource
{
    /// <summary>Typing.</summary>
    Input,

    /// <summary>Pasting.</summary>
    Paste,

    /// <summary>A toolbar item or shortcut.</summary>
    Command,

    /// <summary>Undo or redo.</summary>
    History,

    /// <summary>A call to <see cref="IEditorApi.SetHtmlAsync"/>.</summary>
    Api,

    /// <summary>An edit in the HTML source view.</summary>
    SourceView,

    /// <summary>Resizing an image.</summary>
    ImageResize
}
