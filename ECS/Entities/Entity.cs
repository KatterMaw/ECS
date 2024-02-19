using ECS.Entities.Mutation;

namespace ECS.Entities;

public struct Entity
{
	public bool Has<TComponent>() where TComponent : struct
	{
		return _archetype.Has<TComponent>();
	}
	
	public ref TComponent Get<TComponent>() where TComponent : struct
	{
		return ref _archetype.Get<TComponent>(_index);
	}

	public EntityAdditionBuilder Add<TComponent>(TComponent component = default) where TComponent : struct
	{
		EntityAdditionBuilder additionBuilder = new(this);
		additionBuilder.Add(component);
		return additionBuilder;
	}

	public EntityRemovalBuilder Remove<TComponent>() where TComponent : struct
	{
		EntityRemovalBuilder removalBuilder = new(this);
		removalBuilder.Remove<TComponent>();
		return removalBuilder;
	}
	
	internal Entity(Archetype archetype, int index)
	{
		_archetype = archetype;
		_index = index;
	}

	internal readonly EntityBuilder CreateBuilder()
	{
		EntityBuilder builder = new();
		_archetype.AddComponentsToBuilder(builder, _index);
		return builder;
	}

	internal readonly ref Entity Get()
	{
		return ref _archetype.GetEntity(_index);
	}

	internal void Mutate(EntityBuilder builder)
	{
		ref var newEntity = ref builder.Build(_archetype.World);
		_archetype.Remove(_index);
		_archetype = newEntity._archetype;
		_index = newEntity._index;
		newEntity = this;
	}
	
	private Archetype _archetype;
	private int _index;
}