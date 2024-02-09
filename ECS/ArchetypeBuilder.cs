using System.Collections;
using System.Collections.Immutable;

namespace ECS;

public sealed class ArchetypeBuilder
{
	public ArchetypeBuilder Add<TComponent>() where TComponent : struct
	{
		return Add<TComponent>(ComponentType<TComponent>.Value);
	}

	internal ArchetypeBuilder Add<TComponent>(ComponentType componentType)
	{
		_componentTypes.Add(componentType);
		_listsBuilder.Add(componentType, new List<TComponent>());
		return this;
	}

	public void Build(World world)
	{
		BuildAndReturn(world);
	}

	internal Archetype BuildAndReturn(World world)
	{
		ArchetypeDescription archetypeDescription = new(_componentTypes);
		Archetype archetype = new(_listsBuilder.ToImmutable());
		world.Archetypes.Add(archetypeDescription, archetype);
		return archetype;
	}

	private readonly List<ComponentType> _componentTypes = new();
	private readonly ImmutableDictionary<ComponentType, IList>.Builder _listsBuilder =
		ImmutableDictionary.CreateBuilder<ComponentType, IList>();
}