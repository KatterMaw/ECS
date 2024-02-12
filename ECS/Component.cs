namespace ECS;

internal static class Component<T> where T : struct
{
	// ReSharper disable StaticMemberInGenericType
	internal static ComponentType Type { get; }
	internal static ComponentFactory DefaultFactory { get; }
	// ReSharper restore StaticMemberInGenericType
	
	static Component()
	{
		Type = ComponentType.Create<T>();
		DefaultFactory = ComponentFactory.Create<T>();
	}
}