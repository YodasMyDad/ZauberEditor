namespace ZauberCMS.RTE.Models;

/// <summary>Raised when the editor's content changes.</summary>
/// <param name="Html">The new HTML.</param>
/// <param name="Source">What caused the change.</param>
public sealed record EditorChangeArgs(string Html, ChangeSource Source);
