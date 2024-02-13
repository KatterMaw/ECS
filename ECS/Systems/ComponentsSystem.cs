using ECS.Systems.Queries;

namespace ECS.Systems;

public abstract class ComponentsSystem<TComponent1, TComponent2> : ISystem
	where TComponent1 : struct
	where TComponent2 : struct
{
	public void Update()
	{
		PreUpdate();
		foreach (var archetype in _query.Archetypes)
		{
			var components1 = archetype.GetSpan<TComponent1>();
			var components2 = archetype.GetSpan<TComponent2>();
			for (int i = 0; i < components1.Length; i++)
			{
				ref var component1 = ref components1[i];
				ref var component2 = ref components2[i];
				Update(ref component1, ref component2);
			}
		}
		PostUpdate();
	}

	protected ComponentsSystem(World world)
	{
		_query = new PredicateArchetypeQuery(world, archetype => archetype.Has<TComponent1>() && archetype.Has<TComponent2>());
	}
	
	protected virtual void PreUpdate() { }
	protected abstract void Update(ref TComponent1 component1, ref TComponent2 component2);
	protected virtual void PostUpdate() { }
	
	private readonly ArchetypeQuery _query;
}