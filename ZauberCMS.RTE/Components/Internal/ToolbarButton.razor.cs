using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Components.Internal;

/// <summary>A toolbar button. Used by <see cref="ZauberRichTextEditor"/>; not for direct use.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed partial class ToolbarButton
{
    private (IToolbarItem? Item, bool IsActive, bool IsEnabled, int TabIndex) _rendered;
    private bool _shouldRender = true;

    /// <summary>The item the button runs.</summary>
    [Parameter, EditorRequired]
    public IToolbarItem Item { get; set; } = default!;

    /// <summary>Whether the button shows as pressed.</summary>
    [Parameter]
    public bool IsActive { get; set; }

    /// <summary>Whether the button can be used.</summary>
    [Parameter]
    public bool IsEnabled { get; set; }

    /// <summary>The button's tab index.</summary>
    [Parameter]
    public int TabIndex { get; set; }

    /// <summary>Raised when the button is clicked.</summary>
    [Parameter]
    public EventCallback<IToolbarItem> OnExecute { get; set; }

    private string CssClass => ToolbarPresenter.ButtonClass($"rte-toolbar-btn rte-btn-{Item.Id}", IsActive, IsEnabled);

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        var current = (Item, IsActive, IsEnabled, TabIndex);
        _shouldRender |= current != _rendered;
        _rendered = current;
    }

    /// <inheritdoc />
    protected override bool ShouldRender()
    {
        var shouldRender = _shouldRender;
        _shouldRender = false;
        return shouldRender;
    }

    private Task ExecuteAsync() => OnExecute.InvokeAsync(Item);
}
