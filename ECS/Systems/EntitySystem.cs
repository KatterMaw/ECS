using ECS.Systems.Queries;

namespace ECS.Systems;

public abstract class EntitySystem : ISystem
{
	public void Update()
	{
		foreach (var archetype in _query.Archetypes)
		{
			PreUpdate();
			var entities = archetype.GetEntitiesSpan();
			foreach (var entity in entities)
				Update(in entity);
			PostUpdate();
		}
	}

	protected EntitySystem(ArchetypeQuery query)
	{
		_query = query;
	}

	protected virtual void PreUpdate() { }
	protected abstract void Update(ref readonly Entity entity);
	protected virtual void PostUpdate() { }
	
	private readonly ArchetypeQuery _query;
}