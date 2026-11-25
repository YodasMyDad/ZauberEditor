using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ZauberCMS.RTE.Components.Internal;
using ZauberCMS.RTE.Interop;
using ZauberCMS.RTE.Models;
using ZauberCMS.RTE.Services;

namespace ZauberCMS.RTE.Components;

/// <summary>A rich-text (WYSIWYG) HTML editor.</summary>
public sealed partial class ZauberRichTextEditor : IAsyncDisposable, IEditorHost, IEditorEventSink
{
    private const string FailureMessage = "Sorry, that didn't work. Please try again.";
    private const string TooLargeMessage = "This content is too large to save. Remove some of it, such as large images.";

    private readonly string _contentId = $"rte-content-{Guid.NewGuid():N}";
    private readonly CancellationTokenSource _lifetime = new();
    private readonly TaskCompletionSource<IEditorInterop> _interop = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly ContentChunks _chunks = new();
    private Task? _disposal;
    private CancellationToken _token;
    private ElementReference _root;
    private ElementReference _content;
    private ElementReference _source;
    private EditorToolbar? _toolbar;
    private ToastRegion? _toasts;
    private SlideOutPanel? _panel;
    private DotNetObjectReference<EditorCallbacks>? _callbacks;
    private ToolbarItemSet _items = default!;
    private ContentSync _sync = default!;
    private EditorState _state = EditorState.Empty;
    private ToolbarArrangement _arrangement = ToolbarArrangement.Empty;
    private ToolbarLayout? _arrangedLayout;
    private EditorShortcutSet _shortcuts = EditorShortcutSet.Empty;
    private (EditorSettings? Settings, ToolbarArrangement? Arrangement, IReadOnlyList<ShortcutBinding>? Shortcuts, bool ReadOnly, string? Placeholder) _configuration;
    private bool _configurationPending;
    private Theme? _themeParameter;
    private (EditorDimensions? Source, string? Style) _dimensionStyle;
    private PanelRequest? _panelRequest;
    private string? _panelTitle;
    private TaskCompletionSource? _panelRendered;
    private EditorState? _renderedState;
    private bool _sourceViewTarget;
    private Task _contentWork = Task.CompletedTask;

    /// <summary>Programmatic access to this editor. Calls made before it has loaded wait until it is ready.</summary>
    public IEditorApi Api => field ??= new EditorApi(this);

    /// <summary>The HTML content. Use with <c>@bind-Value</c>.</summary>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>Raised when the content changes.</summary>
    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

    /// <summary>Identifies the bound value for <see cref="EditContext"/> validation.</summary>
    [Parameter]
    public Expression<Func<string?>>? ValueExpression { get; set; }

    /// <summary>Editor settings. Pass a new instance to apply changes.</summary>
    [Parameter]
    public EditorSettings Settings { get; set; } = new();

    /// <summary>The toolbar layout; defaults to <see cref="EditorSettings.ToolbarLayout"/>.</summary>
    [Parameter]
    public ToolbarLayout? Toolbar { get; set; }

    /// <summary>The colour theme. Use with <c>@bind-Theme</c> to follow the theme toggle.</summary>
    [Parameter]
    public Theme Theme { get; set; }

    /// <summary>Raised when the theme is changed from inside the editor.</summary>
    [Parameter]
    public EventCallback<Theme> ThemeChanged { get; set; }

    /// <summary>Prevents editing and disables the toolbar.</summary>
    [Parameter]
    public bool ReadOnly { get; set; }

    /// <summary>Keyboard shortcuts handled without a toolbar button. They win over item shortcuts with the same keys and are ignored while read-only.</summary>
    [Parameter]
    public IReadOnlyList<EditorShortcut>? Shortcuts { get; set; }

    /// <summary>Text shown while the editor is empty.</summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>Accessible name of the editing area.</summary>
    [Parameter]
    public string AriaLabel { get; set; } = "Rich text editor";

    /// <summary>Raised after the content changes, with what caused the change.</summary>
    [Parameter]
    public EventCallback<EditorChangeArgs> OnChange { get; set; }

    /// <summary>Raised when the selection, formatting or mode changes.</summary>
    [Parameter]
    public EventCallback<EditorState> OnStateChanged { get; set; }

    /// <summary>Raised after a toolbar item or shortcut has run.</summary>
    [Parameter]
    public EventCallback<CommandExecutedArgs> OnCommandExecuted { get; set; }

    /// <summary>Raised when a toolbar item or shortcut throws, after the failure is logged and an error toast is shown.</summary>
    [Parameter]
    public EventCallback<CommandFailedArgs> OnCommandFailed { get; set; }

    /// <summary>Raised after the user resizes an image.</summary>
    [Parameter]
    public EventCallback<ImageResizedArgs> OnImageResized { get; set; }

    /// <summary>Raised when the editor gains focus.</summary>
    [Parameter]
    public EventCallback OnFocus { get; set; }

    /// <summary>Raised when the editor loses focus.</summary>
    [Parameter]
    public EventCallback OnBlur { get; set; }

    /// <summary>Extra attributes applied to the root element.</summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    [CascadingParameter]
    private EditContext? EditContext { get; set; }

    [Inject]
    private ToolbarRegistry Registry { get; set; } = default!;

    [Inject]
    private IEditorInteropFactory InteropFactory { get; set; } = default!;

    [Inject]
    private IServiceProvider Services { get; set; } = default!;

    [Inject]
    private ILogger<ZauberRichTextEditor> Logger { get; set; } = default!;

    EditorState IEditorHost.State => _state;

    EditorSettings IEditorHost.Settings => Settings;

    private ToolbarLayout EffectiveLayout => Toolbar ?? Settings.ToolbarLayout;

    private string ThemeClass => _state.Theme switch
    {
        Theme.Light => "rte-theme-light",
        Theme.Dark => "rte-theme-dark",
        _ => "rte-theme-auto"
    };

    private string RootClass => string.Join(
        ' ',
        new[]
        {
            "zauber-rte",
            ThemeClass,
            ReadOnly ? "rte-readonly" : null,
            _state.IsSourceView ? "rte-source-mode" : null,
            _state.IsFullscreen ? "rte-fullscreen" : null,
            AdditionalAttributes?.GetValueOrDefault("class") as string
        }.Where(part => !string.IsNullOrWhiteSpace(part)));

    private string? RootStyle =>
        string.Join(';', new[] { _dimensionStyle.Style, AdditionalAttributes?.GetValueOrDefault("style") as string }.Where(part => !string.IsNullOrWhiteSpace(part)))
            is { Length: > 0 } style ? style : null;

    /// <summary>Delivers pending edits through <see cref="ValueChanged"/>, then releases the editor's script instance and references.</summary>
    public ValueTask DisposeAsync() => new(_disposal ??= DisposeCoreAsync());

    private async Task DisposeCoreAsync()
    {
        await DeliverPendingContentAsync();
        await _lifetime.CancelAsync();
        _interop.TrySetCanceled(_token);
        _panelRendered?.TrySetCanceled(_token);
        if (_interop.Task.IsCompletedSuccessfully)
        {
            await (await _interop.Task).DisposeAsync();
        }

        _callbacks?.Dispose();
        if (_items is not null)
        {
            await _items.DisposeAsync();
        }

        _lifetime.Dispose();
    }

    private async Task DeliverPendingContentAsync()
    {
        if (!_interop.Task.IsCompletedSuccessfully)
        {
            return;
        }

        try
        {
            await (await _interop.Task).FlushAsync(CancellationToken.None);
            await _contentWork;
        }
        catch (Exception exception) when (exception is JSDisconnectedException or JSException)
        {
            return;
        }
    }

    Task<IEditorInterop> IEditorHost.GetInteropAsync(CancellationToken cancellationToken) =>
        GetInteropAsync(cancellationToken);

    Task IEditorHost.ToggleSourceViewAsync(CancellationToken cancellationToken) => InvokeAsync(async () =>
    {
        var interop = await GetInteropAsync(cancellationToken);
        var target = _sourceViewTarget = !_sourceViewTarget;
        await (target ? interop.EnterSourceViewAsync(cancellationToken) : interop.ExitSourceViewAsync(cancellationToken));
        await SetStateAsync(_state with { IsSourceView = _sourceViewTarget });
    });

    Task IEditorHost.SetThemeAsync(Theme theme, CancellationToken cancellationToken) => InvokeAsync(async () =>
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_state.Theme == theme)
        {
            return;
        }

        await SetStateAsync(_state with { Theme = theme });
        await ThemeChanged.InvokeAsync(theme);
    });

    Task IEditorHost.OpenPanelAsync(PanelRequest request, CancellationToken cancellationToken) => InvokeAsync(async () =>
    {
        var interop = await GetInteropAsync(cancellationToken);
        if (_panelRequest is not null)
        {
            await interop.ClosePanelAsync(cancellationToken);
        }

        _panelRequest = request;
        _panelTitle = null;
        _panelRendered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        StateHasChanged();
        await _panelRendered.Task.WaitAsync(cancellationToken);
        if (_panel is not null)
        {
            await interop.OpenPanelAsync(_panel.Element, cancellationToken);
        }
    });

    Task IEditorHost.ClosePanelAsync(CancellationToken cancellationToken) => InvokeAsync(async () =>
    {
        if (_panelRequest is null)
        {
            return;
        }

        var interop = await GetInteropAsync(cancellationToken);
        await interop.ClosePanelAsync(cancellationToken);
        _panelRequest = null;
        _panelTitle = null;
        StateHasChanged();
    });

    Task IEditorHost.ShowToastAsync(string message, ToastType type, CancellationToken cancellationToken) => InvokeAsync(() =>
    {
        cancellationToken.ThrowIfCancellationRequested();
        _toasts?.Show(message, type);
    });

    Task IEditorHost.FlushAsync(CancellationToken cancellationToken) => InvokeAsync(async () =>
    {
        var interop = await GetInteropAsync(cancellationToken);
        await interop.FlushAsync(cancellationToken);
        await _contentWork.WaitAsync(cancellationToken);
    });

    Task IEditorEventSink.ContentChangedAsync(string? html, ChangeSource source, int revision) => InvokeAsync(() =>
    {
        var complete = _chunks.Complete(html, Settings.MaxContentBytes);
        return TrackContentWork(RunGuardedAsync(() => ApplyContentAsync(complete, source, revision), "content change"));
    });

    Task IEditorEventSink.ContentChunkAsync(string chunk) =>
        InvokeAsync(() => _chunks.Append(chunk, Settings.MaxContentBytes));

    Task IEditorEventSink.StateChangedAsync(EditorSnapshot snapshot) =>
        InvokeAsync(() => SetStateAsync(snapshot.ApplyTo(_state)));

    Task IEditorEventSink.ShortcutAsync(string id, bool custom) => InvokeAsync(() => (ReadOnly, custom) switch
    {
        (true, _) => Task.CompletedTask,
        (false, true) => _shortcuts.Find(id) is { } shortcut ? ExecuteShortcutAsync(shortcut) : Task.CompletedTask,
        (false, false) => _arrangement.Find(id) is { } item ? ExecuteItemAsync(item) : Task.CompletedTask
    });

    Task IEditorEventSink.FocusChangedAsync(bool focused) =>
        InvokeAsync(() => RunGuardedAsync(() => focused ? OnFocus.InvokeAsync() : OnBlur.InvokeAsync(), "focus change"));

    Task IEditorEventSink.ImageResizedAsync(int width, int height) =>
        InvokeAsync(() => RunGuardedAsync(() => OnImageResized.InvokeAsync(new(width, height)), "image resize"));

    Task IEditorEventSink.PanelEscapeAsync() => InvokeAsync(ClosePanelFromUserAsync);

    void IEditorEventSink.ScriptError(string message) => Logger.ScriptError(message);

    Task IEditorEventSink.InputRejectedAsync(string message) => InvokeAsync(() =>
    {
        Logger.InputRejected(message);
        _toasts?.Show(message, ToastType.Warning);
    });

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        _token = _lifetime.Token;
        _items = Registry.CreateItemSet(Services);
        _sync = new(Value);
    }

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        ArgumentNullException.ThrowIfNull(Settings);
        ApplyLocalParameters();
        var configurationChanged = TrackConfiguration();
        var pushValue = _sync.ParameterChanged(Value);
        if (!_interop.Task.IsCompletedSuccessfully)
        {
            _configurationPending |= configurationChanged;
            return;
        }

        var interop = await _interop.Task;
        await RunGuardedAsync(
            async () =>
            {
                if (configurationChanged)
                {
                    await interop.ConfigureAsync(BuildOptions(null), _token);
                }

                if (pushValue)
                {
                    await PushValueAsync(interop);
                }
            },
            "apply parameters");
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await InitializeAsync();
        }

        if (_panelRequest is null)
        {
            return;
        }

        _panelRendered?.TrySetResult();
        if (_panelRequest.Instance is { } panel)
        {
            panel.Rendering ??= RefreshPanelTitle;
        }

        RefreshPanelTitle();
    }

    /// <inheritdoc />
    protected override bool ShouldRender()
    {
        _renderedState = _state;
        return true;
    }

    private void ApplyLocalParameters()
    {
        if (Theme != _themeParameter)
        {
            _themeParameter = Theme;
            _state = _state with { Theme = Theme };
        }

        if (ReadOnly != _state.IsReadOnly)
        {
            _state = _state with { IsReadOnly = ReadOnly };
        }

        if (EffectiveLayout != _arrangedLayout)
        {
            _arrangedLayout = EffectiveLayout;
            _arrangement = ToolbarArrangement.Create(EffectiveLayout, _items.Items);
            LogMissingItems(_arrangement.MissingIds);
        }

        _shortcuts = _shortcuts.Update(Shortcuts);
        if (Settings.Dimensions != _dimensionStyle.Source)
        {
            _dimensionStyle = (Settings.Dimensions, BuildDimensionStyle(Settings.Dimensions));
        }
    }

    private void LogMissingItems(IReadOnlyList<string> missingIds)
    {
        var removed = missingIds.ToLookup(_items.WasRemoved);
        if (removed[false].Any())
        {
            Logger.UnknownToolbarItems(removed[false]);
        }

        if (removed[true].Any())
        {
            Logger.RemovedToolbarItems(removed[true]);
        }
    }

    private bool TrackConfiguration()
    {
        var configuration = (Settings, _arrangement, _shortcuts.Bindings, ReadOnly, Placeholder);
        if (configuration == _configuration)
        {
            return false;
        }

        _configuration = configuration;
        return true;
    }

    private async Task InitializeAsync()
    {
        _callbacks = DotNetObjectReference.Create(new EditorCallbacks(this));
        _configurationPending = false;
        if (await CreateInteropAsync(_callbacks) is not { } interop)
        {
            return;
        }

        await RunGuardedAsync(
            async () =>
            {
                if (_configurationPending)
                {
                    await interop.ConfigureAsync(BuildOptions(null), _token);
                }

                if (_sync.Revision != 0)
                {
                    await PushValueAsync(interop);
                }
            },
            "apply parameters");
    }

    private async Task<IEditorInterop?> CreateInteropAsync(DotNetObjectReference<EditorCallbacks> callbacks)
    {
        try
        {
            var interop = await InteropFactory.CreateAsync(
                _root,
                _content,
                _source,
                _toolbar?.Element ?? default,
                callbacks,
                BuildOptions(_sync.KnownHtml));

            if (interop is not null && !_lifetime.IsCancellationRequested)
            {
                _interop.TrySetResult(interop);
                return interop;
            }

            _interop.TrySetCanceled(_token);
            await (interop?.DisposeAsync() ?? ValueTask.CompletedTask);
        }
        catch (Exception) when (_lifetime.IsCancellationRequested)
        {
            _interop.TrySetCanceled(_token);
        }
        catch (Exception exception)
        {
            Logger.InitializationFailed(exception);
            _interop.TrySetException(exception);
            _toasts?.Show("The editor could not be loaded.", ToastType.Error);
        }

        return null;
    }

    private Task<IEditorInterop> GetInteropAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_lifetime.IsCancellationRequested, this);
        return _interop.Task.WaitAsync(cancellationToken);
    }

    private Task PushValueAsync(IEditorInterop interop) =>
        interop.ResetHtmlAsync(_sync.KnownHtml ?? string.Empty, _sync.Revision, _token);

    private async Task ApplyContentAsync(string? html, ChangeSource source, int revision)
    {
        if (ReadOnly || !_sync.IsCurrent(revision))
        {
            return;
        }

        if (html is null)
        {
            Logger.ContentNotDelivered(Settings.MaxContentBytes);
            _toasts?.Show(TooLargeMessage, ToastType.Warning);
            return;
        }

        if (!_sync.Accept(revision, html))
        {
            return;
        }

        _sync.BeginEcho(html);
        try
        {
            await ValueChanged.InvokeAsync(html);
        }
        finally
        {
            _sync.EndEcho(html);
        }

        if (ValueExpression is not null)
        {
            EditContext?.NotifyFieldChanged(FieldIdentifier.Create(ValueExpression));
        }

        await OnChange.InvokeAsync(new(html, source));
    }

    private Task TrackContentWork(Task work)
    {
        _contentWork = _contentWork.IsCompleted ? work : Task.WhenAll(_contentWork, work);
        return work;
    }

    private Task ExecuteItemAsync(IToolbarItem item) => RunCommandAsync(item.Id, () => item.IsEnabled(_state), item.ExecuteAsync);

    private Task ExecuteShortcutAsync(EditorShortcut shortcut) => RunCommandAsync(shortcut.Keys, static () => true, shortcut.Handler);

    private Task RunCommandAsync(string id, Func<bool> isEnabled, Func<IEditorApi, CancellationToken, Task> execute) => RunGuardedAsync(
        async () =>
        {
            await _contentWork;
            if (!isEnabled())
            {
                return;
            }

            await execute(Api, _token);
            await OnCommandExecuted.InvokeAsync(new(id));
        },
        id,
        exception => OnCommandFailed.InvokeAsync(new(id, exception)));

    private Task FocusFromToolbarAsync() => RunGuardedAsync(() => Api.FocusAsync(_token), "focus");

    private Task ClosePanelFromUserAsync() => RunGuardedAsync(() => Api.ClosePanelAsync(_token), "close panel");

    private async Task RunGuardedAsync(Func<Task> action, string operation, Func<Exception, Task>? failed = null)
    {
        try
        {
            await action();
        }
        catch (Exception exception) when (IsTeardown(exception))
        {
            return;
        }
        catch (Exception exception)
        {
            Logger.OperationFailed(operation, exception);
            _toasts?.Show(FailureMessage, ToastType.Error);
            if (failed is not null)
            {
                await RunGuardedAsync(() => failed(exception), "command failed");
            }
        }
    }

    private bool IsTeardown(Exception exception) =>
        exception is JSDisconnectedException
        || (exception is OperationCanceledException or ObjectDisposedException && _lifetime.IsCancellationRequested);

    private Task SetStateAsync(EditorState state)
    {
        if (state == _state)
        {
            return Task.CompletedTask;
        }

        _state = state;
        var notified = RunGuardedAsync(() => OnStateChanged.InvokeAsync(state), "state change");
        if (!ReferenceEquals(_renderedState, state))
        {
            StateHasChanged();
        }

        return notified;
    }

    private void RefreshPanelTitle()
    {
        if (_panelRequest?.Instance?.Title is { } title && title != _panelTitle)
        {
            _panelTitle = title;
            StateHasChanged();
        }
    }

    private EditorInitOptions BuildOptions(string? html) =>
        EditorInitOptions.Create(html, ReadOnly, Placeholder, Settings, _arrangement.Items, _shortcuts.Bindings);

    private static string? BuildDimensionStyle(EditorDimensions dimensions)
    {
        string[] declarations =
        [
            .. new[]
            {
                ("--rte-width", dimensions.Width),
                ("--rte-height", dimensions.Height),
                ("--rte-min-height", dimensions.MinHeight),
                ("--rte-max-height", dimensions.MaxHeight)
            }
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Item2))
            .Select(pair => $"{pair.Item1}:{pair.Item2}")
        ];
        return declarations.Length == 0 ? null : string.Join(';', declarations);
    }
}
