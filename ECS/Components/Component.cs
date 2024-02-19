namespace ECS.Components;

internal static class Component<T> where T : struct
{
	// ReSharper disable once StaticMemberInGenericType
	internal static ComponentType Type { get; }
	
	static Component()
	{
		Type = ComponentType.Create<T>();
	}
}