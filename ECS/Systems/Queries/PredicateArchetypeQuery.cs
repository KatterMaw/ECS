using ECS.Entities;

namespace ECS.Systems.Queries;

public sealed class PredicateArchetypeQuery : ArchetypeQuery
{
	public PredicateArchetypeQuery(World world, Func<Archetype, bool> predicate) : base(world)
	{
		_predicate = predicate;
	}
	
	protected override bool IsSuitable(Archetype archetype)
	{
		return _predicate(archetype);
	}

	private readonly Func<Archetype, bool> _predicate;
}