namespace ECS.Extensions;

internal static class ComponentExtensions
{
	public static int GetMaxId(this IEnumerable<ComponentType> types)
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