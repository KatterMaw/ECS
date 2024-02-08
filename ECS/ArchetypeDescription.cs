namespace ECS;

internal sealed class ArchetypeDescription
{
	public ArchetypeDescription(IReadOnlyCollection<ComponentType> componentTypes)
	{
		_typesBits = componentTypes.ToBitArray();
	}

	public override bool Equals(object? obj)
	{
		return ReferenceEquals(this, obj) || obj is ArchetypeDescription other && Equals(other);
	}

	public override int GetHashCode() => _typesBits.GetHashCode();
	public bool Equals(ArchetypeDescription other) => _typesBits.Equals(other._typesBits);

	private readonly BitArray _typesBits;
}