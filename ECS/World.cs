namespace ECS;

public sealed class World
{
	internal Dictionary<ArchetypeDescription, Archetype> Archetypes { get; } = new();
}