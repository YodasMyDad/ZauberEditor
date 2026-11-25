using Microsoft.AspNetCore.Components;

namespace ZauberCMS.RTE.Models;

/// <summary>Base class for components shown in the editor's slide-out panel.</summary>
public abstract class PanelBase : ComponentBase
{
    private readonly string _idPrefix = $"rte-panel-{Guid.NewGuid():N}";

    /// <summary>The editor that opened the panel.</summary>
    [CascadingParameter]
    public IEditorApi Api { get; set; } = default!;

    /// <summary>The panel heading, also its accessible name. Changes are shown whenever the panel re-renders.</summary>
    public abstract string Title { get; }

    internal Action? Rendering { get; set; }

    /// <summary>Returns an element id that is unique to this panel instance.</summary>
    /// <param name="name">A name for the element within the panel.</param>
    protected string ElementId(string name) => $"{_idPrefix}-{name}";

    /// <summary>Closes the panel and returns focus to the editor.</summary>
    protected Task CloseAsync() => Api.ClosePanelAsync();

    /// <summary>Keeps the panel heading in step with <see cref="Title"/>. Call the base method when overriding.</summary>
    protected override bool ShouldRender()
    {
        Rendering?.Invoke();
        return true;
    }
}
