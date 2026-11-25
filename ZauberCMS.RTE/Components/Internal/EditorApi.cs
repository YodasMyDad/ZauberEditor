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

    public Task IndentAsync(CancellationToken cancellationToken = default) =>
        RunAsync(interop => interop.IndentAsync(cancellationToken), cancellationToken);

    public Task OutdentAsync(CancellationToken cancellationToken = default) =>
        RunAsync(interop => interop.OutdentAsync(cancellationToken), cancellationToken);

    public Task SetTextStyleAsync(string styleName, string? value, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(styleName);
        return RunAsync(interop => interop.SetTextStyleAsync(styleName, value, cancellationToken), cancellationToken);
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

    public Task FlushAsync(CancellationToken cancellationToken = default) => host.FlushAsync(cancellationToken);

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

    public Task<EmbedInfo?> GetEmbedAtCursorAsync(CancellationToken cancellationToken = default) =>
        RunAsync(interop => interop.GetEmbedAsync(Settings.MaxContentBytes, cancellationToken), cancellationToken);

    public Task<bool> InsertEmbedAsync(string url, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        return RunAsync(interop => interop.InsertEmbedAsync(url, cancellationToken), cancellationToken);
    }

    public Task UndoAsync(CancellationToken cancellationToken = default) =>
        RunAsync(interop => interop.UndoAsync(cancellationToken), cancellationToken);

    public Task RedoAsync(CancellationToken cancellationToken = default) =>
        RunAsync(interop => interop.RedoAsync(cancellationToken), cancellationToken);

    public Task FocusAsync(CancellationToken cancellationToken = default) =>
        RunAsync(interop => interop.FocusAsync(cancellationToken), cancellationToken);

    public Task ToggleSourceViewAsync(CancellationToken cancellationToken = default) =>
        host.ToggleSourceViewAsync(cancellationToken);

    public Task SetFullscreenAsync(bool fullscreen, CancellationToken cancellationToken = default) =>
        RunAsync(interop => interop.SetFullscreenAsync(fullscreen, cancellationToken), cancellationToken);

    public Task SetPlainTextPasteAsync(bool plainText, CancellationToken cancellationToken = default) =>
        RunAsync(interop => interop.SetPlainTextPasteAsync(plainText, cancellationToken), cancellationToken);

    public Task<FindResult> FindAsync(string text, bool matchCase = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(text);
        return RunAsync(interop => interop.FindAsync(text, matchCase, cancellationToken), cancellationToken);
    }

    public Task<FindResult> ReplaceAsync(string text, string replacement, bool matchCase = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(replacement);
        return RunAsync(interop => interop.ReplaceAsync(text, replacement, matchCase, cancellationToken), cancellationToken);
    }

    public Task<int> ReplaceAllAsync(string text, string replacement, bool matchCase = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(replacement);
        return RunAsync(interop => interop.ReplaceAllAsync(text, replacement, matchCase, cancellationToken), cancellationToken);
    }

    public Task SetThemeAsync(Theme theme, CancellationToken cancellationToken = default) =>
        host.SetThemeAsync(theme, cancellationToken);

    public Task OpenPanelAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TPanel>(
        IReadOnlyDictionary<string, object?>? parameters = null,
        CancellationToken cancellationToken = default)
        where TPanel : PanelBase =>
        OpenPanelAsync(typeof(TPanel), parameters, cancellationToken);

    public Task OpenPanelAsync(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type panelType,
        IReadOnlyDictionary<string, object?>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(panelType);
        if (panelType.IsAbstract || !panelType.IsAssignableTo(typeof(PanelBase)))
        {
            throw new ArgumentException($"'{panelType.FullName}' is not a concrete component derived from PanelBase.", nameof(panelType));
        }

        return host.OpenPanelAsync(PanelRequest.Create(panelType, parameters), cancellationToken);
    }

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
