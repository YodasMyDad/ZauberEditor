using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ZauberCMS.RTE.Models;

namespace ZauberCMS.RTE.Services;

/// <summary>Registers toolbar items. Built-in items come first; a later item with the same id replaces an earlier one.</summary>
public sealed class ZauberRteOptions
{
    private readonly List<ToolbarItemRegistration> _registrations = [];

    internal IReadOnlyList<ToolbarItemRegistration> Registrations => _registrations;

    /// <summary>
    /// Loads the editor stylesheet automatically on the first editor in a page. Defaults to <see langword="true"/>; turn it off
    /// when the page links or bundles <c>zauber-editor.css</c> itself.
    /// </summary>
    public bool AutoLoadStyles { get; set; } = true;

    /// <summary>Adds an item created per editor through dependency injection.</summary>
    /// <typeparam name="TItem">The item type.</typeparam>
    public ZauberRteOptions AddToolbarItem<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TItem>()
        where TItem : class, IToolbarItem
    {
        _registrations.Add(ToolbarItemRegistration.ForFactory(typeof(TItem), static services => ActivatorUtilities.CreateInstance<TItem>(services)));
        return this;
    }

    /// <summary>Adds an item instance shared by every editor; it must be stateless.</summary>
    /// <param name="item">The item.</param>
    public ZauberRteOptions AddToolbarItem(IToolbarItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _registrations.Add(ToolbarItemRegistration.ForInstance(item));
        return this;
    }

    /// <summary>Removes an item registered earlier, including a built-in one.</summary>
    /// <param name="id">The item id.</param>
    public ZauberRteOptions RemoveToolbarItem(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        _registrations.Add(ToolbarItemRegistration.ForRemoval(id));
        return this;
    }

    /// <summary>Adds every concrete <see cref="IToolbarItem"/> type in an assembly.</summary>
    /// <param name="assembly">The assembly to scan.</param>
    [RequiresUnreferencedCode("Assembly scanning finds item types by reflection, which trimming can remove. Use AddToolbarItem<TItem>() instead.")]
    public ZauberRteOptions AddToolbarItemsFromAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        foreach (var type in assembly.GetTypes())
        {
            if (type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false } && type.IsAssignableTo(typeof(IToolbarItem)))
            {
                _registrations.Add(ToolbarItemRegistration.ForFactory(type, services => (IToolbarItem)ActivatorUtilities.CreateInstance(services, type)));
            }
        }

        return this;
    }
}
