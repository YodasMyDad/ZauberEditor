using System.Collections.Frozen;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Tests.TestSupport;

internal static class States
{
    public static EditorState Ready => EditorState.Empty;

    public static EditorState Marks(params string[] tags) =>
        EditorState.Empty with { ActiveMarks = tags.ToFrozenSet(StringComparer.OrdinalIgnoreCase) };

    public static EditorState Block(string blockType) => EditorState.Empty with { BlockType = blockType };
}
