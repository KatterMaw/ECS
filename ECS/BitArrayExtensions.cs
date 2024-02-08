namespace ECS;

internal static class BitArrayExtensions
{
	public static BitArray ToBitArray(this IReadOnlyCollection<ComponentType> types)
	{
		BitArray bitArray = new(types.Max(type => type.Id) + 1);
		foreach (var type in types)
			bitArray[type.Id] = true;
		return bitArray;
	}
}