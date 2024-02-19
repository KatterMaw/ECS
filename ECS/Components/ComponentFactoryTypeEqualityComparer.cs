namespace ECS.Components;

internal sealed class ComponentFactoryTypeEqualityComparer : IEqualityComparer<ComponentFactory>
{
	public static ComponentFactoryTypeEqualityComparer Instance { get; } = new();
	
	public bool Equals(ComponentFactory? x, ComponentFactory? y)
	{
		if (ReferenceEquals(x, y))
			return true;
		if (ReferenceEquals(x, null))
			return false;
		if (ReferenceEquals(y, null))
			return false;
		return x.ComponentType.Equals(y.ComponentType);
	}

	public int GetHashCode(ComponentFactory obj)
	{
		return obj.ComponentType.GetHashCode();
	}
}