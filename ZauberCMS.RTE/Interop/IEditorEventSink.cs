using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Interop;

internal interface IEditorEventSink
{
    Task ContentChangedAsync(string? html, ChangeSource source, int revision);

    Task StateChangedAsync(EditorSnapshot snapshot);

    Task ShortcutAsync(string itemId);

    Task FocusChangedAsync(bool focused);

    Task ImageResizedAsync(int width, int height);

    Task PanelEscapeAsync();

    void ScriptError(string message);

    Task InputRejectedAsync(string message);
}
