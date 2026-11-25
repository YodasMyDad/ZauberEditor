namespace ZauberCMS.RTE.Models;

/// <summary>An image in the editor.</summary>
/// <param name="Src">The image source.</param>
/// <param name="Alt">The alternative text.</param>
/// <param name="Title">The title attribute.</param>
/// <param name="Width">The width in pixels, if set.</param>
/// <param name="Height">The height in pixels, if set.</param>
public sealed record ImageInfo(string Src, string? Alt, string? Title, int? Width, int? Height);
