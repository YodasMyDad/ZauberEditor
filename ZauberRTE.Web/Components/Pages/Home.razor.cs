using System.Collections.Frozen;
using ZauberCMS.RTE.Models;
using ZauberRTE.Web.Client.Components;

namespace ZauberRTE.Web.Components.Pages;

public sealed partial class Home
{
    private const int LogLength = 8;

    private static readonly EditorSettings UploadSettings = new()
    {
        ImageConstraints = new() { AllowBase64ImageUpload = true },
        HtmlPolicy = new()
        {
            AllowedAttributes = FrozenSet.Create(StringComparer.OrdinalIgnoreCase, [.. new HtmlPolicy().AllowedAttributes.Where(name => name is not "target")]),
            TagAttributes = new Dictionary<string, IReadOnlySet<string>> { ["a"] = FrozenSet.Create(StringComparer.OrdinalIgnoreCase, "target") }
        }
    };

    private readonly List<ChangeEntry> _changes = [];
    private string? _html = """
        <h2>Welcome to Zauber RTE 3.0</h2>
        <p>Try <strong>bold</strong>, <em>italic</em>, <a href="https://example.com">links</a>, lists, tables and images.</p>
        <ul><li>Undo and redo with Ctrl+Z / Ctrl+Y</li><li>View the HTML source from the toolbar</li></ul>
        """;
    private EditorState _state = EditorState.Empty;
    private int _changeCount;
    private int _saveCount;
    private string _saveStatus = "Not saved yet: press Ctrl+S.";

    private IReadOnlyList<EditorShortcut> Shortcuts => [new("Control+s", SaveAsync)];

    private (string Name, string Value)[] StateRows =>
    [
        ("Block", _state.BlockType),
        ("Marks", _state.ActiveMarks.Count == 0 ? "none" : string.Join(", ", _state.ActiveMarks.Order())),
        ("Alignment", _state.Alignment.ToString()),
        ("Focus / selection", $"{YesNo(_state.HasFocus)} / {YesNo(_state.HasSelection)}"),
        ("Undo / redo", $"{YesNo(_state.CanUndo)} / {YesNo(_state.CanRedo)}"),
        ("In link / image / table", $"{YesNo(_state.InLink)} / {YesNo(_state.InImage)} / {YesNo(_state.InTable)}"),
        ("Source view", YesNo(_state.IsSourceView))
    ];

    private void LogChange(EditorChangeArgs change)
    {
        _changes.Insert(0, new(++_changeCount, change.Source, ByteSize.Format(change.Html)));
        if (_changes.Count > LogLength)
        {
            _changes.RemoveAt(LogLength);
        }
    }

    private void ShowState(EditorState state) => _state = state;

    private async Task SaveAsync(IEditorApi api, CancellationToken cancellationToken)
    {
        var html = await api.GetHtmlAsync(cancellationToken);
        _saveStatus = $"Saved #{++_saveCount}: {ByteSize.Format(html)}";
        StateHasChanged();
        await api.ShowToastAsync("Saved.", ToastType.Success, cancellationToken);
    }

    private void ShowFailure(CommandFailedArgs failure) => _saveStatus = $"'{failure.ItemId}' failed: {failure.Exception.Message}";

    private static string YesNo(bool value) => value ? "yes" : "no";

    private sealed record ChangeEntry(int Number, ChangeSource Source, string Size);
}
