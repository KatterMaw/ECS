namespace ECS;

// ReSharper disable once UnusedTypeParameter
internal static class Component<T> where T : struct
{
	// ReSharper disable once StaticMemberInGenericType
	public static ComponentType Type { get; private set; }
	
	static Component()
	{
		Type = ComponentType.Create();
	}
}

internal sealed class ComponentType
{
	internal static ComponentType Create()
	{
		var newId = Interlocked.Increment(ref _lastId);
		return new ComponentType(newId);
	}

	private static int _lastId = -1;
	
	public int Id { get; }

	public override int GetHashCode() => Id;

	private ComponentType(int id)
	{
		Id = id;
	}
}