using CommunityToolkit.Diagnostics;

namespace ECS;

public sealed class EntityBuilder
{
	public EntityBuilder(bool buildArchetype = true)
	{
		if (buildArchetype)
			_archetypeBuilder = new ArchetypeBuilder();
	}
	
	public EntityBuilder Add<TComponent>(TComponent component) where TComponent : struct
	{
		AddInternal<TComponent>();
		_componentFactories.Add(ComponentFactory.Create(component));
		return this;
	}
	
	public EntityBuilder Add<TComponent>() where TComponent : struct
	{
		AddInternal<TComponent>();
		_componentFactories.Add(Component<TComponent>.DefaultFactory);
		return this;
	}

	private void AddInternal<TComponent>() where TComponent : struct
	{
		var componentType = Component<TComponent>.Type;
		_archetypeBuilder?.Add<TComponent>();
		_componentTypes.Add(componentType);
	}

	public ref Entity Build(World world)
	{
		ArchetypeDescription archetypeDescription = new(_componentTypes);
		var archetype = GetOrCreateArchetype(world, archetypeDescription);
		return ref archetype.CreateEntity(_componentFactories);
	}

	private readonly List<ComponentType> _componentTypes = new();
	private readonly List<ComponentFactory> _componentFactories = new();
	private readonly ArchetypeBuilder? _archetypeBuilder;

	private Archetype GetOrCreateArchetype(World world, ArchetypeDescription archetypeDescription)
	{
		if (world.Archetypes.TryGetValue(archetypeDescription, out var archetype))
			return archetype;
		Guard.IsNotNull(_archetypeBuilder);
		return _archetypeBuilder.BuildAndReturn(world);
	}
}