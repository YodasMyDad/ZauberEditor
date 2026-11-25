using ZauberCMS.RTE.Interop;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Components.Internal;

internal interface IEditorHost
{
    EditorState State { get; }

    EditorSettings Settings { get; }

    Task<IEditorInterop> GetInteropAsync(CancellationToken cancellationToken);

    Task ToggleSourceViewAsync(CancellationToken cancellationToken);

    Task SetThemeAsync(Theme theme, CancellationToken cancellationToken);

    Task OpenPanelAsync(PanelRequest request, CancellationToken cancellationToken);

    Task ClosePanelAsync(CancellationToken cancellationToken);

    Task ShowToastAsync(string message, ToastType type, CancellationToken cancellationToken);
}
