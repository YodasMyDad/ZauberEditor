namespace ZauberCMS.RTE.Models;

/// <summary>Raised after a toolbar item has run.</summary>
/// <param name="ItemId">The id of the item.</param>
public sealed record CommandExecutedArgs(string ItemId);
