using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Components.Internal;

internal sealed class PanelRequest
{
    private PanelRequest(Func<PanelRequest, RenderFragment> content) => Content = content(this);

    public RenderFragment Content { get; }

    public PanelBase? Instance { get; private set; }

    public static PanelRequest Create(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type panelType,
        IReadOnlyDictionary<string, object?>? parameters) =>
        new(request => builder =>
        {
            builder.OpenComponent(0, panelType);
            foreach (var (name, value) in parameters ?? FrozenDictionary<string, object?>.Empty)
            {
                builder.AddComponentParameter(1, name, value);
            }

            builder.AddComponentReferenceCapture(2, instance => request.Instance = (PanelBase)instance);
            builder.CloseComponent();
        });
}
