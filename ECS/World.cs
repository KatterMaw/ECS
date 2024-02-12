using System.Diagnostics.CodeAnalysis;
namespace ECS;

public sealed class World
{
	internal Dictionary<ArchetypeDescription, Archetype> Archetypes { get; } = new();
	internal IReadOnlyCollection<Archetype> Archetypes => _archetypes.Values;
	
	internal void AddArchetype(ArchetypeDescription archetypeDescription, Archetype archetype)
	{
		_archetypes.Add(archetypeDescription, archetype);
	}

	internal bool TryGetArchetype(ArchetypeDescription description, [MaybeNullWhen(false)] out Archetype archetype)
	{
		return _archetypes.TryGetValue(description, out archetype);
	}

	private readonly Subject<Archetype> _archetypeAdded = new();
}