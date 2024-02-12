using System.Collections;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using CommunityToolkit.Diagnostics;

namespace ECS;

public sealed class Archetype
{
	public bool Has<TComponent>() where TComponent : struct
	{
		return _components[Component<TComponent>.Type.Id] != null;
	}
	
	internal Archetype(ImmutableArray<IList?> componentLists)
	{
		_components = componentLists;
	}
	
	internal ref Entity CreateEntity(List<ComponentFactory> componentFactories)
	{
		AddComponents(componentFactories);
		return ref CreateEntity();
	}
	
	internal ref Entity CreateEntity(ImmutableList<ComponentFactory> componentFactories)
	{
		AddComponents(componentFactories);
		return ref CreateEntity();
	}

	internal ref TComponent Get<TComponent>(int index) where TComponent : struct
	{
		return ref GetSpan<TComponent>()[index];
	}

	internal Span<TComponent> GetSpan<TComponent>() where TComponent : struct
	{
		return CollectionsMarshal.AsSpan(GetList<TComponent>());
	}

	internal Span<Entity> GetEntitiesSpan()
	{
		return CollectionsMarshal.AsSpan(_entities);
	}

	private readonly List<Entity> _entities = new();
	private readonly ImmutableArray<IList?> _components;

	private List<TComponent> GetList<TComponent>() where TComponent : struct
	{
		var list = _components[Component<TComponent>.Type.Id];
		Guard.IsNotNull(list);
		return (List<TComponent>)list;
	}

	private void AddComponents(List<ComponentFactory> componentFactories)
	{
		foreach (var componentFactory in componentFactories)
		{
			var list = _components[componentFactory.ComponentType.Id];
			Guard.IsNotNull(list);
			componentFactory.AddComponent(list);
		}
	}

	private void AddComponents(ImmutableList<ComponentFactory> componentFactories)
	{
		foreach (var componentFactory in componentFactories)
		{
			var list = _components[componentFactory.ComponentType.Id];
			Guard.IsNotNull(list);
			componentFactory.AddComponent(list);
		}
	}

	private ref Entity CreateEntity()
	{
		var newEntityIndex = _entities.Count;
		Entity entity = new(this, newEntityIndex);
		_entities.Add(entity);
		return ref GetEntity(newEntityIndex);
	}

	private ref Entity GetEntity(int index)
	{
		return ref GetEntitiesSpan()[index];
	}
}