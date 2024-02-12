using System.Collections;
using System.Collections.Immutable;
using ECS.Extensions;

namespace ECS;

public sealed class ArchetypeBuilder
{
	public ArchetypeBuilder Add<TComponent>() where TComponent : struct
	{
		var componentType = Component<TComponent>.Type;
		_componentTypes.Add(componentType);
		return this;
	}

	public Archetype Build(World world)
	{
		ArchetypeDescription archetypeDescription = new(_componentTypes);
		Archetype archetype = new(Lists);
		world.AddArchetype(archetypeDescription, archetype);
		return archetype;
	}
	
	private readonly List<ComponentType> _componentTypes = new();
	
	private ImmutableArray<IList?> Lists
	{
		get
		{
			var requiredSize = _componentTypes.GetMaxId() + 1;
			var builder = ImmutableArray.CreateBuilder<IList?>(requiredSize);
			builder.Count = requiredSize;
			foreach (var componentType in _componentTypes)
				builder[componentType.Id] = componentType.List;
			return builder.ToImmutable();
		}
	}
}