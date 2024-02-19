using System.Runtime.InteropServices;
using ECS.Components;

namespace ECS.Entities;

public sealed class Archetype
{
	public int EntitiesCount => _entities.Count;

	public Span<Entity> GetEntities()
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

	internal World World { get; }
	internal ArchetypeDescription Description { get; }

	internal Archetype(World world, ArchetypeDescription archetypeDescription)
	{
		World = world;
		Description = archetypeDescription;
		_components = new ComponentsList(archetypeDescription.ComponentTypes);
	}
	
	internal ref Entity CreateEntity(IEnumerable<ComponentFactory> componentFactories)
	{
		_components.AddComponents(componentFactories);
		return ref CreateEntity();
	}

	internal ref TComponent Get<TComponent>(int index) where TComponent : struct
	{
		return ref _components.Get<TComponent>(index);
	}

	internal ref Entity GetEntity(int index)
	{
		return ref GetEntities()[index];
	}

	internal void Remove(int index)
	{
		_entities.RemoveAt(index);
		_components.Remove(index);
	}

	internal void EnsureRemainingCapacity(int capacity)
	{
		_entities.EnsureCapacity(_entities.Count + capacity);
		_components.EnsureRemainingCapacity(capacity);
	}

	internal void AddComponentsToBuilder(EntityBuilder builder, int index)
	{
		_components.AddToBuilder(builder, index);
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
}