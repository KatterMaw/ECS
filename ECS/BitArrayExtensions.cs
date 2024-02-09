namespace ECS;

internal static class BitArrayExtensions
{
	public static BitArray ToBitArray(this IReadOnlyCollection<ComponentType> types)
	{
		BitArray bitArray = new(types.GetMaxId() + 1);
		foreach (var type in types)
			bitArray[type.Id] = true;
		return bitArray;
	}

	private static int GetMaxId(this IEnumerable<ComponentType> types)
	{
		var result = 0;
		foreach (var type in types)
		{
			if (type.Id > result)
				result = type.Id;
		}
		return result;
	}
}