using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Components.Internal;

/// <summary>The dialog that hosts editor panels. Used by <see cref="ZauberRichTextEditor"/>; not for direct use.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed partial class SlideOutPanel
{
    private readonly string _titleId = $"rte-panel-title-{Guid.NewGuid():N}";
    private ErrorBoundary? _boundary;
    private RenderFragment? _shownContent;

    /// <summary>The editor the panel belongs to.</summary>
    [Parameter, EditorRequired]
    public IEditorApi Api { get; set; } = default!;

    /// <summary>The theme class applied to the dialog.</summary>
    [Parameter]
    public string? ThemeClass { get; set; }

    /// <summary>The panel to show, or null when closed.</summary>
    [Parameter]
    public RenderFragment? Content { get; set; }

    /// <summary>The dialog heading.</summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>Raised when the user closes the dialog.</summary>
    [Parameter]
    public EventCallback OnClose { get; set; }

    internal ElementReference Element { get; private set; }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (ReferenceEquals(Content, _shownContent))
        {
            return;
        }

        _shownContent = Content;
        _boundary?.Recover();
    }
}
