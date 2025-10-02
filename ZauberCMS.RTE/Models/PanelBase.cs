using Microsoft.AspNetCore.Components;

namespace ZauberCMS.RTE.Models;

/// <summary>
/// Base class for custom panel components that provides common functionality
/// </summary>
public abstract class PanelBase : ComponentBase, IDisposable
{
    /// <summary>
    /// The editor API instance for interacting with the editor
    /// </summary>
    [CascadingParameter]
    public IEditorApi? Api { get; set; }

    /// <summary>
    /// Closes the panel
    /// </summary>
    protected async Task CloseAsync()
    {
        if (Api != null)
        {
            await Api.ClearSavedSelectionRangeAsync();
            await Api.ClosePanelAsync();
        }
    }

    /// <summary>
    /// Override this method to perform cleanup when the panel is disposed
    /// </summary>
    protected virtual void OnDispose() { }

    /// <summary>
    /// Disposes the panel component
    /// </summary>
    public void Dispose()
    {
        OnDispose();
        GC.SuppressFinalize(this);
    }
}

