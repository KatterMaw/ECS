using CommunityToolkit.Diagnostics;
using ECS.Components;

namespace ECS.Entities;

public sealed class EntityBuilder
{
	public EntityBuilder Add<TComponent>(TComponent component) where TComponent : struct
	{
		var isAdded = _componentFactories.Add(ComponentFactory.Create(component));
		Guard.IsTrue(isAdded);
		_cachedArchetype = null;
		return this;
	}
	
	public EntityBuilder Add<TComponent>() where TComponent : struct
	{
		var isAdded = _componentFactories.Add(ComponentFactory<TComponent>.Default);
		Guard.IsTrue(isAdded);
		_cachedArchetype = null;
		return this;
	}

	public EntityBuilder Remove<TComponent>() where TComponent : struct
	{
		var isRemoved = _componentFactories.Remove(ComponentFactory<TComponent>.Default);
		Guard.IsTrue(isRemoved);
		_cachedArchetype = null;
		return this;
	}

	public ref Entity Build(World world)
	{
		var archetype = GetOrCreateArchetype(world);
		return ref archetype.CreateEntity(_componentFactories);
	}

	public void Build(World world, int count)
	{
		var archetype = GetOrCreateArchetype(world);
		archetype.EnsureRemainingCapacity(count);
		for (int i = 0; i < count; i++)
			archetype.CreateEntity(_componentFactories);
	}

	private IReadOnlyCollection<ComponentType> ComponentTypes =>
		_componentFactories.Select(factory => factory.ComponentType).ToArray();
	private readonly HashSet<ComponentFactory> _componentFactories = new(ComponentFactoryTypeEqualityComparer.Instance);
	private Archetype? _cachedArchetype;

	private Archetype GetOrCreateArchetype(World world)
	{
		if (_cachedArchetype != null)
			return _cachedArchetype;
		ArchetypeDescription archetypeDescription = new(ComponentTypes);
		_cachedArchetype = world.GetOrCreateArchetype(archetypeDescription);
		return _cachedArchetype;
	}
}