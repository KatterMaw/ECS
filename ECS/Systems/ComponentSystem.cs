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
			foreach (var component in components)
				Update(in component);
			PostUpdate();
		}
	}

	protected ComponentSystem(ArchetypeQuery query)
	{
		_query = query;
	}

	protected virtual void PreUpdate() { }
	protected abstract void Update(ref readonly TComponent component);
	protected virtual void PostUpdate() { }
	
	private readonly ArchetypeQuery _query;
}