using System.Collections;

namespace ECS;

internal readonly struct ComponentFactory
{
	internal static ComponentFactory Create<TComponent>() where TComponent : struct
	{
		return new ComponentFactory(Component<TComponent>.Type, list => ((IList<TComponent>)list).Add(new TComponent()));
	}
	
	internal static ComponentFactory Create<TComponent>(TComponent component) where TComponent : struct
	{
		return new ComponentFactory(Component<TComponent>.Type, list => ((IList<TComponent>)list).Add(component));
	}

	public readonly ComponentType ComponentType;
	public readonly Action<IList> AddComponent;

	private ComponentFactory(ComponentType componentType, Action<IList> addComponent)
	{
		ComponentType = componentType;
		AddComponent = addComponent;
	}
}