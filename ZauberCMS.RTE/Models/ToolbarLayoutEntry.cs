using System.Text.Json.Serialization;

namespace ZauberCMS.RTE.Models;

/// <summary>One entry in a <see cref="ToolbarLayout"/>.</summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(ToolbarGroup), "group")]
[JsonDerivedType(typeof(ToolbarItemRef), "item")]
[JsonDerivedType(typeof(ToolbarSeparator), "separator")]
public abstract record ToolbarLayoutEntry
{
    /// <summary>Extra CSS class for the entry's element.</summary>
    public string? CssClass { get; init; }
}
