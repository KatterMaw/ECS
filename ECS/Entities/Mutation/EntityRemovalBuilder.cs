using ECS.Components;

namespace ECS.Entities.Mutation;

public sealed class EntityRemovalBuilder
{
	internal EntityRemovalBuilder(Entity entity)
	{
		_entity = entity;
	}

	public EntityRemovalBuilder Remove<TComponent>() where TComponent : struct
	{
		_componentTypes.Add(Component<TComponent>.Type);
		return this;
	}

	public void Mutate()
	{
		var entityBuilder = _entity.CreateBuilder();
		foreach (var componentType in _componentTypes)
			entityBuilder.Remove(componentType);
		_entity.Get().Mutate(entityBuilder);
	}

	private readonly Entity _entity;
	private readonly HashSet<ComponentType> _componentTypes = new();
}