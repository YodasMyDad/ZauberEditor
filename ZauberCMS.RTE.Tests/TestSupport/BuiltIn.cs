using ZauberCMS.RTE.Models;
using ZauberCMS.RTE.Toolbar;

namespace ZauberCMS.RTE.Tests.TestSupport;

internal static class BuiltIn
{
    public static IToolbarItem Item(string id) => BuiltInToolbarItems.All.Single(item => item.Id == id);
}
