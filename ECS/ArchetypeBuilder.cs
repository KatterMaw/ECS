using System.Collections;
using System.Collections.Immutable;

namespace ECS;

internal sealed class ArchetypeBuilder
{
	public ArchetypeBuilder Add<TComponent>() where TComponent : struct
	{
		var componentType = ComponentType.Get<TComponent>();
		_componentTypes.Add(componentType);
		_listsBuilder.Add(componentType, new List<TComponent>());
		return this;
	}

	public Archetype Build(World world)
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