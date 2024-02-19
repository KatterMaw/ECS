using System.Runtime.InteropServices;

namespace ECS;

public sealed class Archetype
{
	public int EntitiesCount => _entities.Count;

	public Span<Entity> GetEntitiesSpan()
	{
		return CollectionsMarshal.AsSpan(_entities);
	}
	
	public Span<TComponent> GetSpan<TComponent>() where TComponent : struct
	{
		return _components.GetSpan<TComponent>();
	}

	public Span<TComponent> GetOptionalSpan<TComponent>() where TComponent : struct
	{
		return _components.GetOptionalSpan<TComponent>();
	}

	public bool Has<TComponent>() where TComponent : struct
	{
		return _components.Has<TComponent>();
	}

	internal ArchetypeDescription Description { get; }

	internal Archetype(ArchetypeDescription archetypeDescription)
	{
		Description = archetypeDescription;
		_components = new ComponentsList(archetypeDescription.ComponentTypes);
	}
	
	internal ref Entity CreateEntity(List<ComponentFactory> componentFactories)
	{
		_components.AddComponents(componentFactories);
		return ref CreateEntity();
	}

	internal ref TComponent Get<TComponent>(int index) where TComponent : struct
	{
		return ref _components.Get<TComponent>(index);
	}

	internal void EnsureRemainingCapacity(int capacity)
	{
		_entities.EnsureCapacity(_entities.Count + capacity);
		_components.EnsureRemainingCapacity(capacity);
	}

	private readonly List<Entity> _entities = new();
	private readonly ComponentsList _components;

	private ref Entity CreateEntity()
	{
		var newEntityIndex = _entities.Count;
		Entity entity = new(this, newEntityIndex);
		_entities.Add(entity);
		return ref GetEntity(newEntityIndex);
	}

	private ref Entity GetEntity(int index)
	{
		return ref GetEntities()[index];
	}
}