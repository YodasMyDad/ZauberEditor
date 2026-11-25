using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Components.Internal;

/// <summary>A toolbar button with a menu of child items. Used by <see cref="ZauberRichTextEditor"/>; not for direct use.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed partial class ToolbarDropdown
{
    private ElementReference _trigger;
    private ElementReference[] _menuItems = [];
    private bool[] _childActive = [];
    private bool[] _childEnabled = [];
    private (IToolbarItem? Item, bool IsActive, bool IsEnabled, int TabIndex) _rendered;
    private bool _shouldRender = true;
    private bool _isOpen;
    private int _focused = -1;
    private int? _pendingFocus;

    /// <summary>The dropdown item.</summary>
    [Parameter, EditorRequired]
    public IToolbarItem Item { get; set; } = default!;

    /// <summary>The items listed in the menu.</summary>
    [Parameter, EditorRequired]
    public IReadOnlyList<IToolbarItem> Children { get; set; } = [];

    /// <summary>The current editor state.</summary>
    [Parameter, EditorRequired]
    public EditorState State { get; set; } = default!;

    /// <summary>Whether the dropdown can be used.</summary>
    [Parameter]
    public bool IsEnabled { get; set; }

    /// <summary>The trigger's tab index.</summary>
    [Parameter]
    public int TabIndex { get; set; }

    /// <summary>Raised when a menu item is chosen.</summary>
    [Parameter]
    public EventCallback<IToolbarItem> OnExecute { get; set; }

    /// <summary>Raised when the menu closes on the already-checked item, so focus can return to the editor.</summary>
    [Parameter]
    public EventCallback OnDismiss { get; set; }

    private string TriggerClass =>
        ToolbarPresenter.ButtonClass("rte-toolbar-btn rte-dropdown-trigger", _rendered.IsActive, IsEnabled, _isOpen);

    private IToolbarItem? ActiveChild => Array.IndexOf(_childActive, true) is var index and >= 0 ? Children[index] : null;

    private IToolbarItem TriggerIconItem => ActiveChild is { } child && ToolbarPresenter.HasIcon(child) ? child : Item;

    private string ItemClass(int index) =>
        $"rte-dropdown-item rte-dropdown-{Children[index].Id}{(_childActive[index] ? " rte-dropdown-item-active" : null)}";

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        bool[] active = [.. Children.Select(child => child.IsActive(State))];
        bool[] enabled = [.. Children.Select(child => child.IsEnabled(State))];
        var current = (Item, Item.IsActive(State), IsEnabled, TabIndex);
        if (current == _rendered && active.SequenceEqual(_childActive) && enabled.SequenceEqual(_childEnabled))
        {
            return;
        }

        _rendered = current;
        _childActive = active;
        _childEnabled = enabled;
        if (_menuItems.Length != Children.Count)
        {
            _menuItems = new ElementReference[Children.Count];
        }

        if (!IsEnabled)
        {
            _isOpen = false;
        }

        _shouldRender = true;
    }

    /// <inheritdoc />
    protected override bool ShouldRender()
    {
        var shouldRender = _shouldRender;
        _shouldRender = false;
        return shouldRender;
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_pendingFocus is { } index)
        {
            _pendingFocus = null;
            await FocusItemAsync(index);
        }
    }

    private void Toggle()
    {
        if (_isOpen)
        {
            Close();
        }
        else
        {
            Open(fromEnd: false);
        }
    }

    private void Open(bool fromEnd)
    {
        var selected = Array.IndexOf(_childActive, true);
        _pendingFocus = selected >= 0 && _childEnabled[selected] ? selected : NextEnabled(fromEnd ? 0 : -1, fromEnd ? -1 : 1);
        _isOpen = true;
        _shouldRender = true;
    }

    private void Close()
    {
        _isOpen = false;
        _focused = -1;
        _shouldRender = true;
    }

    private async Task CloseAndFocusTriggerAsync()
    {
        Close();
        await _trigger.FocusAsync();
    }

    private async Task SelectAsync(int index)
    {
        if (!_childEnabled[index])
        {
            return;
        }

        Close();
        var child = Children[index];
        if (child.IsToggle && _childActive[index])
        {
            await OnDismiss.InvokeAsync();
            return;
        }

        await OnExecute.InvokeAsync(child);
    }

    private async Task OnTriggerKeyDownAsync(KeyboardEventArgs args)
    {
        switch (args.Key)
        {
            case "ArrowDown" when !_isOpen:
                Open(fromEnd: false);
                break;
            case "ArrowUp" when !_isOpen:
                Open(fromEnd: true);
                break;
            case "Escape" when _isOpen:
                await CloseAndFocusTriggerAsync();
                break;
        }
    }

    private async Task OnMenuKeyDownAsync(KeyboardEventArgs args)
    {
        switch (args.Key)
        {
            case "ArrowDown":
                await FocusItemAsync(NextEnabled(_focused, 1));
                break;
            case "ArrowUp":
                await FocusItemAsync(NextEnabled(_focused < 0 ? 0 : _focused, -1));
                break;
            case "Home":
                await FocusItemAsync(NextEnabled(-1, 1));
                break;
            case "End":
                await FocusItemAsync(NextEnabled(0, -1));
                break;
            case "Enter" or " " when _focused >= 0:
                await SelectAsync(_focused);
                break;
            case "Escape" or "Tab":
                await CloseAndFocusTriggerAsync();
                break;
        }
    }

    private int NextEnabled(int from, int step)
    {
        var count = Children.Count;
        for (var offset = 1; offset <= count; offset++)
        {
            var index = ((from + (step * offset)) % count + count) % count;
            if (_childEnabled[index])
            {
                return index;
            }
        }

        return -1;
    }

    private async Task FocusItemAsync(int index)
    {
        if (index < 0 || index >= _menuItems.Length)
        {
            return;
        }

        _focused = index;
        await _menuItems[index].FocusAsync();
    }
}
