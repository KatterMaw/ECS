using ECS.Systems.Queries;

namespace ECS.Systems;

public abstract class ComponentSystem<TComponent> : ISystem where TComponent : struct
{
	public void Update()
	{
		foreach (var archetype in _query.Archetypes)
		{
			PreUpdate();
			var components = archetype.GetSpan<TComponent>();
			for (var index = 0; index < components.Length; index++)
			{
				ref var component = ref components[index];
				Update(ref component);
			}
			PostUpdate();
		}
	}

	protected ComponentSystem(World world)
	{
		_query = new PredicateArchetypeQuery(world, archetype => archetype.Has<TComponent>());
	}

	protected virtual void PreUpdate() { }
	protected abstract void Update(ref TComponent component);
	protected virtual void PostUpdate() { }
	
	private readonly ArchetypeQuery _query;
}