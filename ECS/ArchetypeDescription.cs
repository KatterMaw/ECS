﻿namespace ECS;

internal readonly struct ArchetypeDescription
{
	public ArchetypeDescription(IReadOnlyCollection<ComponentType> componentTypes)
	{
		_typesBits = componentTypes.ToBitArray();
	}

	public override bool Equals(object? obj)
	{
		return obj is ArchetypeDescription other && Equals(other);
	}

	public bool Equals(ArchetypeDescription other)
	{
		return _typesBits.Equals(other._typesBits);
	}

	public override int GetHashCode()
	{
		return _typesBits.GetHashCode();
	}

	private readonly BitArray _typesBits;
}