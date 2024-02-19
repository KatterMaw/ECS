using CommunityToolkit.Diagnostics;
using ECS.Components;

namespace ECS.Entities;

public sealed class EntityBuilder
{
	public EntityBuilder Add<TComponent>(TComponent component = default) where TComponent : struct
	{
		var componentFactory = ComponentFactory.Create(component);
		Add(componentFactory);
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

	internal void Add(ComponentFactory componentFactory)
	{
		var isAdded = _componentFactories.Add(componentFactory);
		Guard.IsTrue(isAdded);
		_cachedArchetype = null;
	}

	internal void Remove(ComponentType componentType)
	{
		var isRemoved = _componentFactories.Remove(componentType.DefaultFactory);
		Guard.IsTrue(isRemoved);
		_cachedArchetype = null;
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