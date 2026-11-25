using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Interop;

internal sealed record ImageOptions(
    bool MaintainAspectRatio,
    int? MaxWidth,
    int? MaxHeight,
    bool AllowBase64Upload,
    long MaxFileSizeBytes,
    IReadOnlyList<string> AllowedTypes)
{
    public static ImageOptions From(ImageConstraints constraints) => new(
        constraints.MaintainAspectRatio,
        constraints.MaxWidth,
        constraints.MaxHeight,
        constraints.AllowBase64ImageUpload,
        constraints.MaxFileSizeBytes,
        NameList.Normalise(constraints.AllowedImageTypes));
}
