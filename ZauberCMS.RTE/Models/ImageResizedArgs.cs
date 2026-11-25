namespace ZauberCMS.RTE.Models;

/// <summary>Raised after the user resizes an image.</summary>
/// <param name="Width">The new width in pixels.</param>
/// <param name="Height">The new height in pixels.</param>
public sealed record ImageResizedArgs(int Width, int Height);
