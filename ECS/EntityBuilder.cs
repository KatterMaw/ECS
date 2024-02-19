namespace ECS;

public sealed class EntityBuilder
{
	public EntityBuilder Add<TComponent>(TComponent component) where TComponent : struct
	{
		AddInternal<TComponent>();
		_componentFactories.Add(ComponentFactory.Create(component));
		return this;
	}
	
	public EntityBuilder Add<TComponent>() where TComponent : struct
	{
		AddInternal<TComponent>();
		_componentFactories.Add(ComponentFactory<TComponent>.Default);
		return this;
	}

	private void AddInternal<TComponent>() where TComponent : struct
	{
		var componentType = Component<TComponent>.Type;
		_componentTypes.Add(componentType);
		_cachedArchetype = null;
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

	private readonly List<ComponentType> _componentTypes = new();
	private readonly List<ComponentFactory> _componentFactories = new();
	private Archetype? _cachedArchetype;

	private Archetype GetOrCreateArchetype(World world)
	{
		if (_cachedArchetype != null)
			return _cachedArchetype;
		ArchetypeDescription archetypeDescription = new(_componentTypes);
		_cachedArchetype = world.GetOrCreateArchetype(archetypeDescription);
		return _cachedArchetype;
	}
}