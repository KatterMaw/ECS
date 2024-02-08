namespace ECS;

internal sealed class ComponentType
{
	public static ComponentType Get<T>() where T : struct
	{
		return ComponentTypes.TryGetValue(typeof(T), out var componentType) ? componentType : Create<T>();
	}

	private static readonly Dictionary<Type, ComponentType> ComponentTypes = new();

	private static ComponentType Create<T>()
	{
		var newId = ComponentTypes.Count;
		ComponentType componentType = new(newId);
		ComponentTypes.Add(typeof(T), componentType);
		return componentType;
	}
	
	public int Id { get; }

	public override int GetHashCode() => Id;

	private ComponentType(int id)
	{
		Id = id;
	}
}