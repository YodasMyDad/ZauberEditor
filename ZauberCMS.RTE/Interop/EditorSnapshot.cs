using System.Collections.Frozen;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Interop;

internal sealed record EditorSnapshot(
    IReadOnlyList<string>? Marks,
    string? Block,
    string? Alignment,
    bool HasSelection,
    bool HasFocus,
    bool CanUndo,
    bool CanRedo,
    bool InLink,
    bool InImage,
    bool InTable,
    bool PrefersDark)
{
    public EditorState ApplyTo(EditorState state) => state with
    {
        ActiveMarks = (Marks ?? []).ToFrozenSet(StringComparer.OrdinalIgnoreCase),
        BlockType = string.IsNullOrEmpty(Block) ? "p" : Block.ToLowerInvariant(),
        Alignment = Alignment switch
        {
            "center" => TextAlignment.Center,
            "right" => TextAlignment.Right,
            "justify" => TextAlignment.Justify,
            _ => TextAlignment.Left
        },
        HasSelection = HasSelection,
        HasFocus = HasFocus,
        CanUndo = CanUndo,
        CanRedo = CanRedo,
        InLink = InLink,
        InImage = InImage,
        InTable = InTable,
        PrefersDarkScheme = PrefersDark
    };
}
