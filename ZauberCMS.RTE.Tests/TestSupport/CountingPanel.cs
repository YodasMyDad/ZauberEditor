using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Tests.TestSupport;

internal sealed class CountingPanel : PanelBase
{
    private int _count;

    public override string Title => string.Create(CultureInfo.InvariantCulture, $"Count {_count}");

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "button");
        builder.AddAttribute(1, "class", "counting-panel-next");
        builder.AddAttribute(2, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => _count++));
        builder.CloseElement();
    }
}
