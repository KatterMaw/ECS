using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ECS;

public sealed class World
{
	internal IObservable<Archetype> ArchetypeAdded => _archetypeAdded.AsObservable();
	internal IReadOnlyCollection<Archetype> Archetypes => _archetypes.Values;

	internal Archetype GetOrCreateArchetype(ArchetypeDescription archetypeDescription)
	{
		if (TryGetArchetype(archetypeDescription, out var archetype))
			return archetype;
		archetype = new Archetype(archetypeDescription);
		AddArchetype(archetype);
		return archetype;
	}

	private readonly Subject<Archetype> _archetypeAdded = new();
	private readonly Dictionary<ArchetypeDescription, Archetype> _archetypes = new();
	
	private void AddArchetype(Archetype archetype)
	{
		_archetypes.Add(archetype.Description, archetype);
		_archetypeAdded.OnNext(archetype);
	}

	private bool TryGetArchetype(ArchetypeDescription description, [MaybeNullWhen(false)] out Archetype archetype)
	{
		return _archetypes.TryGetValue(description, out archetype);
	}
}