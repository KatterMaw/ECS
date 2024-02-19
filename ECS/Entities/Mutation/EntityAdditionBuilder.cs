using ECS.Components;

namespace ECS.Entities.Mutation;

public sealed class EntityAdditionBuilder
{
	internal EntityAdditionBuilder(Entity entity)
	{
		_entity = entity;
	}
	
	public EntityAdditionBuilder Add<TComponent>(TComponent component = default) where TComponent : struct
	{
		_componentFactories.Add(ComponentFactory.Create(component));
		return this;
	}

	public void Mutate()
	{
		var entityBuilder = _entity.CreateBuilder();
		foreach (var componentFactory in _componentFactories)
			entityBuilder.Add(componentFactory);
		_entity.Get().Mutate(entityBuilder);
	}
	
	private readonly Entity _entity;
	private readonly HashSet<ComponentFactory> _componentFactories = new(ComponentFactoryTypeEqualityComparer.Instance);
}