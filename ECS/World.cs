using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ECS;

public sealed class World
{
	internal IObservable<Archetype> ArchetypeAdded => _archetypeAdded.AsObservable();
	internal IReadOnlyCollection<Archetype> Archetypes => _archetypes.Values;
	
	internal void AddArchetype(ArchetypeDescription archetypeDescription, Archetype archetype)
	{
		_archetypes.Add(archetypeDescription, archetype);
		_archetypeAdded.OnNext(archetype);
	}

	internal bool TryGetArchetype(ArchetypeDescription description, [MaybeNullWhen(false)] out Archetype archetype)
	{
		return _archetypes.TryGetValue(description, out archetype);
	}

	private readonly Subject<Archetype> _archetypeAdded = new();
	private readonly Dictionary<ArchetypeDescription, Archetype> _archetypes = new();
}