using System.Collections;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using CommunityToolkit.Diagnostics;

namespace ECS;

internal sealed class Archetype
{
	internal Archetype(ImmutableDictionary<ComponentType, IList> componentLists)
	{
		_components = componentLists;
	}

	internal ref Entity CreateEntity(IReadOnlyCollection<EntityBuilder.ComponentFactory> componentsData)
	{
		Guard.IsEqualTo(componentsData.Count, _components.Count);
		foreach (var componentData in componentsData)
			componentData.AddComponent(_components[componentData.Type]);
		var newEntityIndex = _entities.Count;
		Entity entity = new(this, newEntityIndex);
		_entities.Add(entity);
		return ref GetEntity(newEntityIndex);
	}

	internal ref TComponent Get<TComponent>(int index) where TComponent : struct
	{
		return ref CollectionsMarshal.AsSpan(GetList<TComponent>())[index];
	}

	private readonly List<Entity> _entities = new();
	private readonly ImmutableDictionary<ComponentType, IList> _components;

	private ref Entity GetEntity(int index)
	{
		return ref CollectionsMarshal.AsSpan(_entities)[index];
	}

	private List<TComponent> GetList<TComponent>() where TComponent : struct
	{
		return (List<TComponent>)_components[ComponentType.Get<TComponent>()];
	}
}