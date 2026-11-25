namespace ZauberCMS.RTE.Models;

/// <summary>Raised when a toolbar item or shortcut throws.</summary>
/// <param name="ItemId">The id of the item, or the <see cref="EditorShortcut.Keys"/> of an editor shortcut.</param>
/// <param name="Exception">The exception it threw.</param>
public sealed record CommandFailedArgs(string ItemId, Exception Exception);
