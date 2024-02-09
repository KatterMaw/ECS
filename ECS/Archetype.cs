using System.Collections;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace ECS;

internal sealed class Archetype
{
	internal Archetype(ImmutableDictionary<ComponentType, IList> componentLists)
	{
		_components = componentLists;
	}
	
	internal ref Entity CreateEntity(IEnumerable<ComponentFactory> componentFactories)
	{
		foreach (var componentFactory in componentFactories)
			componentFactory.AddComponent(_components[componentFactory.ComponentType]);
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
		return (List<TComponent>)_components[Component<TComponent>.Type];
	}
}