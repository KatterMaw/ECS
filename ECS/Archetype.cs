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

	internal ref Entity CreateEntity(List<ComponentType> componentTypes, List<Action<IList>> componentFactories)
	{
		Guard.IsEqualTo(componentTypes.Count, _components.Count);
		for (var i = 0; i < componentTypes.Count; i++)
		{
			var type = componentTypes[i];
			var factory = componentFactories[i];
			factory(_components[type]);
		}
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
		return (List<TComponent>)_components[ComponentType<TComponent>.Value];
	}
}