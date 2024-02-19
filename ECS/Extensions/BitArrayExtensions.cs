using ECS.Components;

namespace ECS.Extensions;

internal static class BitArrayExtensions
{
	public static BitArray ToBitArray(this IReadOnlyCollection<ComponentType> types)
	{
		BitArray bitArray = new(types.GetMaxId() + 1);
		foreach (var type in types)
			bitArray[type.Id] = true;
		return bitArray;
	}
}