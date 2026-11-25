using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Components.Internal;

/// <summary>Shows short status messages. Used by <see cref="ZauberRichTextEditor"/>; not for direct use.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed partial class ToastRegion : IDisposable
{
    private const int MaxToasts = 3;
    private static readonly TimeSpan Lifetime = TimeSpan.FromSeconds(5);

    private readonly List<ToastMessage> _toasts = [];
    private int _nextId;

    [Inject]
    private TimeProvider TimeProvider { get; set; } = default!;

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var toast in _toasts)
        {
            toast.Timer.Dispose();
        }

        _toasts.Clear();
    }

    internal void Show(string message, ToastType type)
    {
        var id = ++_nextId;
        var timer = TimeProvider.CreateTimer(_ => _ = InvokeAsync(() => Expire(id)), null, Lifetime, Timeout.InfiniteTimeSpan);
        _toasts.Add(new(id, message, type, timer));
        if (_toasts.Count > MaxToasts)
        {
            Remove(_toasts[0]);
        }

        StateHasChanged();
    }

    private static string TypeClass(ToastType type) => type switch
    {
        ToastType.Success => "rte-toast-success",
        ToastType.Warning => "rte-toast-warning",
        ToastType.Error => "rte-toast-error",
        _ => "rte-toast-info"
    };

    private void Expire(int id)
    {
        Dismiss(id);
        StateHasChanged();
    }

    private void Dismiss(int id)
    {
        if (_toasts.Find(toast => toast.Id == id) is { } toast)
        {
            Remove(toast);
        }
    }

    private void Remove(ToastMessage toast)
    {
        toast.Timer.Dispose();
        _toasts.Remove(toast);
    }
}
