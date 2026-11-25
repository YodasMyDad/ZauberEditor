using System.Collections.Frozen;

namespace ZauberCMS.RTE.Models;

/// <summary>Rules for inserting and resizing images.</summary>
public sealed record ImageConstraints
{
    private static readonly FrozenSet<string> DefaultImageTypes = FrozenSet.Create(
        StringComparer.OrdinalIgnoreCase,
        ".jpg", ".jpeg", ".png", ".gif", ".webp");

    /// <summary>Largest width, in pixels, an image can be resized to; null is unlimited.</summary>
    public int? MaxWidth { get; init; }

    /// <summary>Largest height, in pixels, an image can be resized to; null is unlimited.</summary>
    public int? MaxHeight { get; init; }

    /// <summary>Whether resizing keeps the image's aspect ratio.</summary>
    public bool MaintainAspectRatio { get; init; } = true;

    /// <summary>File extensions accepted for upload, including the dot (case-insensitive).</summary>
    public IReadOnlySet<string> AllowedImageTypes { get; init; } = DefaultImageTypes;

    /// <summary>Whether images can be uploaded from disk and embedded as data URLs.</summary>
    public bool AllowBase64ImageUpload { get; init; }

    /// <summary>Largest file, in bytes, that can be uploaded.</summary>
    public long MaxFileSizeBytes { get; init; } = 5 * 1024 * 1024;

    /// <summary>Whether both have the same limits and allowed image types.</summary>
    /// <param name="other">The constraints to compare with.</param>
    public bool Equals(ImageConstraints? other) =>
        other is not null
        && MaxWidth == other.MaxWidth
        && MaxHeight == other.MaxHeight
        && MaintainAspectRatio == other.MaintainAspectRatio
        && AllowBase64ImageUpload == other.AllowBase64ImageUpload
        && MaxFileSizeBytes == other.MaxFileSizeBytes
        && ValueEquality.Set(AllowedImageTypes, other.AllowedImageTypes);

    /// <inheritdoc />
    public override int GetHashCode() =>
        HashCode.Combine(MaxWidth, MaxHeight, MaintainAspectRatio, AllowBase64ImageUpload, MaxFileSizeBytes);
}
