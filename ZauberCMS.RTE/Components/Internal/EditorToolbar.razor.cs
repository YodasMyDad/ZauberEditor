using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using ZauberCMS.RTE.Models;
using ZauberCMS.RTE.Services;

namespace ZauberCMS.RTE.Components.Internal;

/// <summary>Renders the editor toolbar. Used by <see cref="ZauberRichTextEditor"/>; not for direct use.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed partial class EditorToolbar
{
    private bool[] _enabled = [];
    private int _tabStop = -1;

    /// <summary>The current editor state.</summary>
    [Parameter, EditorRequired]
    public EditorState State { get; set; } = default!;

    /// <summary>Raised when an item is chosen.</summary>
    [Parameter]
    public EventCallback<IToolbarItem> OnExecute { get; set; }

    /// <summary>Accessible name of the toolbar.</summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>Id of the content element the toolbar controls.</summary>
    [Parameter]
    public string? ContentId { get; set; }

    internal ElementReference Element { get; private set; }

    [CascadingParameter]
    private ToolbarArrangement Arrangement { get; set; } = ToolbarArrangement.Empty;

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        _enabled = [.. Arrangement.Slots.Select(slot => slot.Item.IsEnabled(State))];
        _tabStop = Array.IndexOf(_enabled, true);
    }

    private int TabIndexFor(int index) => index == _tabStop ? 0 : -1;
}
