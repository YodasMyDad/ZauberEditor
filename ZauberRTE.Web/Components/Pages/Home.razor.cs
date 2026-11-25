using ZauberCMS.RTE.Models;
using ZauberRTE.Web.Client.Components;

namespace ZauberRTE.Web.Components.Pages;

public sealed partial class Home
{
    private const int LogLength = 8;

    private static readonly EditorSettings UploadSettings = new() { ImageConstraints = new() { AllowBase64ImageUpload = true } };

    private readonly List<ChangeEntry> _changes = [];
    private string? _html = """
        <h2>Welcome to Zauber RTE 3.0</h2>
        <p>Try <strong>bold</strong>, <em>italic</em>, <a href="https://example.com">links</a>, lists, tables and images.</p>
        <ul><li>Undo and redo with Ctrl+Z / Ctrl+Y</li><li>View the HTML source from the toolbar</li></ul>
        """;
    private EditorState _state = EditorState.Empty;
    private int _changeCount;

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

    private static string YesNo(bool value) => value ? "yes" : "no";

    private sealed record ChangeEntry(int Number, ChangeSource Source, string Size);
}
