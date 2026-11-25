using System.Diagnostics.CodeAnalysis;
using ZauberCMS.RTE.Interop;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Components.Internal;

internal sealed class EditorApi(IEditorHost host) : IEditorApi
{
    public EditorState State => host.State;

    public EditorSettings Settings => host.Settings;

    public Task ToggleMarkAsync(string tag, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);
        return RunAsync(interop => interop.ToggleMarkAsync(tag, cancellationToken), cancellationToken);
    }

    public Task WrapSelectionAsync(string tag, IReadOnlyDictionary<string, string>? attributes = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);
        return RunAsync(interop => interop.WrapAsync(tag, attributes, cancellationToken), cancellationToken);
    }

    public Task UnwrapSelectionAsync(string tag, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);
        return RunAsync(interop => interop.UnwrapAsync(tag, cancellationToken), cancellationToken);
    }

    public Task SetBlockTypeAsync(string blockType, IReadOnlyDictionary<string, string>? attributes = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(blockType);
        return RunAsync(interop => interop.SetBlockTypeAsync(blockType, attributes, cancellationToken), cancellationToken);
    }

    public Task SetBlockStyleAsync(IReadOnlyDictionary<string, string> styles, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(styles);
        return RunAsync(interop => interop.SetBlockStyleAsync(styles, cancellationToken), cancellationToken);
    }

    public Task ClearFormattingAsync(CancellationToken cancellationToken = default) =>
        RunAsync(interop => interop.ClearFormattingAsync(cancellationToken), cancellationToken);

    public Task InsertHtmlAsync(string html, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(html);
        return RunAsync(interop => interop.InsertHtmlAsync(html, cancellationToken), cancellationToken);
    }

    public Task InsertTextAsync(string text, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(text);
        return RunAsync(interop => interop.InsertTextAsync(text, cancellationToken), cancellationToken);
    }

    public Task<string> GetHtmlAsync(CancellationToken cancellationToken = default) =>
        RunAsync(interop => interop.GetHtmlAsync(Settings.MaxContentBytes, cancellationToken), cancellationToken);

    public Task<string> GetTextAsync(CancellationToken cancellationToken = default) =>
        RunAsync(interop => interop.GetTextAsync(Settings.MaxContentBytes, cancellationToken), cancellationToken);

    public Task SetHtmlAsync(string html, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(html);
        if (State.IsReadOnly)
        {
            throw new InvalidOperationException("The editor is read-only. Set its Value parameter instead.");
        }

        return RunAsync(interop => interop.SetHtmlAsync(html, cancellationToken), cancellationToken);
    }

    public Task<SelectionInfo?> GetSelectionAsync(CancellationToken cancellationToken = default) =>
        RunAsync(interop => interop.GetSelectionAsync(Settings.MaxContentBytes, cancellationToken), cancellationToken);

    public Task SaveSelectionAsync(CancellationToken cancellationToken = default) =>
        RunAsync(interop => interop.SaveSelectionAsync(cancellationToken), cancellationToken);

    public Task RestoreSelectionAsync(CancellationToken cancellationToken = default) =>
        RunAsync(interop => interop.RestoreSelectionAsync(cancellationToken), cancellationToken);

    public Task ClearSavedSelectionAsync(CancellationToken cancellationToken = default) =>
        RunAsync(interop => interop.ClearSavedSelectionAsync(cancellationToken), cancellationToken);

    public Task<LinkInfo?> GetLinkAtCursorAsync(CancellationToken cancellationToken = default) =>
        RunAsync(interop => interop.GetLinkAsync(Settings.MaxContentBytes, cancellationToken), cancellationToken);

    public Task SelectLinkAtCursorAsync(CancellationToken cancellationToken = default) =>
        RunAsync(interop => interop.SelectLinkAsync(cancellationToken), cancellationToken);

    public Task<ImageInfo?> GetImageAtCursorAsync(CancellationToken cancellationToken = default) =>
        RunAsync(interop => interop.GetImageAsync(Settings.MaxContentBytes, cancellationToken), cancellationToken);

    public Task SelectImageAtCursorAsync(CancellationToken cancellationToken = default) =>
        RunAsync(interop => interop.SelectImageAsync(cancellationToken), cancellationToken);

    public Task UndoAsync(CancellationToken cancellationToken = default) =>
        RunAsync(interop => interop.UndoAsync(cancellationToken), cancellationToken);

    public Task RedoAsync(CancellationToken cancellationToken = default) =>
        RunAsync(interop => interop.RedoAsync(cancellationToken), cancellationToken);

    public Task FocusAsync(CancellationToken cancellationToken = default) =>
        RunAsync(interop => interop.FocusAsync(cancellationToken), cancellationToken);

    public Task ToggleSourceViewAsync(CancellationToken cancellationToken = default) =>
        host.ToggleSourceViewAsync(cancellationToken);

    public Task SetThemeAsync(Theme theme, CancellationToken cancellationToken = default) =>
        host.SetThemeAsync(theme, cancellationToken);

    public Task OpenPanelAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TPanel>(
        IReadOnlyDictionary<string, object?>? parameters = null,
        CancellationToken cancellationToken = default)
        where TPanel : PanelBase =>
        host.OpenPanelAsync(PanelRequest.Create<TPanel>(parameters), cancellationToken);

    public Task ClosePanelAsync(CancellationToken cancellationToken = default) => host.ClosePanelAsync(cancellationToken);

    public Task ShowToastAsync(string message, ToastType type = ToastType.Info, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        return host.ShowToastAsync(message, type, cancellationToken);
    }

    private async Task RunAsync(Func<IEditorInterop, Task> action, CancellationToken cancellationToken) =>
        await action(await host.GetInteropAsync(cancellationToken));

    private async Task<TResult> RunAsync<TResult>(Func<IEditorInterop, Task<TResult>> action, CancellationToken cancellationToken) =>
        await action(await host.GetInteropAsync(cancellationToken));
}
