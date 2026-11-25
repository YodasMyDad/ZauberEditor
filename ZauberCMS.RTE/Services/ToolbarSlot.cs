using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Services;

internal sealed record ToolbarSlot(int Index, IToolbarItem Item, IReadOnlyList<IToolbarItem> Children);
