using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using CommunityToolkit.Diagnostics;
using ECS.Extensions;

namespace ECS;

public sealed class Archetype
{
	public int EntitiesCount => _entities.Count;

	public Span<Entity> GetEntitiesSpan()
	{
		return CollectionsMarshal.AsSpan(_entities);
	}
	
	public bool Has<TComponent>() where TComponent : struct
	{
		return _components[Component<TComponent>.Type.Id] != null;
	}

	public Span<TComponent> GetSpan<TComponent>() where TComponent : struct
	{
		return CollectionsMarshal.AsSpan(GetList<TComponent>());
	}

	public bool TryGetSpan<TComponent>(out Span<TComponent> span) where TComponent : struct
	{
		span = Span<TComponent>.Empty;
		if (!TryGetList<TComponent>(out var list))
			return false;
		span = CollectionsMarshal.AsSpan(list);
		return true;
	}

	public Span<TComponent> GetOptionalSpan<TComponent>() where TComponent : struct
	{
		if (TryGetSpan<TComponent>(out var span))
			return span;
		return Span<TComponent>.Empty;
	}

	internal ArchetypeDescription Description { get; }

	internal Archetype(ArchetypeDescription archetypeDescription)
	{
		Description = archetypeDescription;
		var componentTypes = archetypeDescription.ComponentTypes;
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

	private bool TryGetList<TComponent>([NotNullWhen(true)] out List<TComponent>? list) where TComponent : struct
	{
		list = null;
		var index = Component<TComponent>.Type.Id;
		if (_components.Length <= index)
			return false;
		list = _components[index] as List<TComponent>;
		return list != null;
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