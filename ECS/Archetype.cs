using System.Collections;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using CommunityToolkit.Diagnostics;
using ECS.Extensions;

namespace ECS;

public sealed class Archetype
{
	public bool Has<TComponent>() where TComponent : struct
	{
		return _components[Component<TComponent>.Type.Id] != null;
	}

	internal Archetype(IReadOnlyCollection<ComponentType> componentTypes)
	{
		var requiredSize = componentTypes.GetMaxId() + 1;
		var builder = ImmutableArray.CreateBuilder<IList?>(requiredSize);
		builder.Count = requiredSize;
		foreach (var componentType in componentTypes)
			builder[componentType.Id] = componentType.List;
		_components = builder.ToImmutable();
		_types = componentTypes.ToImmutableArray();
	}
	
	internal ref Entity CreateEntity(List<ComponentFactory> componentFactories)
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

	internal void EnsureRemainingCapacity(int capacity)
	{
		_entities.EnsureCapacity(_entities.Count + capacity);
		foreach (var type in _types)
		{
			var list = _components[type.Id];
			Guard.IsNotNull(list);
			type.EnsureListRemainingCapacity(list, capacity);
		}
	}

	private readonly List<Entity> _entities = new();
	private readonly ImmutableArray<IList?> _components;
	private readonly ImmutableArray<ComponentType> _types;

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