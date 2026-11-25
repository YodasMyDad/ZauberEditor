namespace ZauberCMS.RTE.Models;

/// <summary>Editor size as CSS lengths, for example <c>300px</c> or <c>50vh</c>.</summary>
public sealed record EditorDimensions
{
    /// <summary>Width; null fills the container.</summary>
    public string? Width { get; init; }

    /// <summary>Height of the editing area; null grows with the content.</summary>
    public string? Height { get; init; } = "300px";

    /// <summary>Minimum height of the editing area.</summary>
    public string? MinHeight { get; init; } = "100px";

    /// <summary>Maximum height of the editing area; null is unlimited.</summary>
    public string? MaxHeight { get; init; }
}
