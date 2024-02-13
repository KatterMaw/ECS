namespace ECS;

public sealed class ArchetypeBuilder
{
	internal static Archetype Build(World world, IReadOnlyCollection<ComponentType> componentTypes)
	{
		ArchetypeDescription archetypeDescription = new(componentTypes);
		Archetype archetype = new(componentTypes);
		world.AddArchetype(archetypeDescription, archetype);
		return archetype;
	}
	
	public ArchetypeBuilder Add<TComponent>() where TComponent : struct
	{
		var componentType = Component<TComponent>.Type;
		_componentTypes.Add(componentType);
		return this;
	}

	public Archetype Build(World world)
	{
		return Build(world, _componentTypes);
	}
	
	private readonly List<ComponentType> _componentTypes = new();
}