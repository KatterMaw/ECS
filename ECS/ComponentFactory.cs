using System.Collections;

namespace ECS;

internal abstract class ComponentFactory
{
	internal static ComponentFactory Create<TComponent>(TComponent component) where TComponent : struct
	{
		return new ComponentFactory<TComponent>(component);
	}

	public ComponentType ComponentType { get; }
	public abstract void AddComponent(IList list);

	protected ComponentFactory(ComponentType componentType)
	{
		ComponentType = componentType;
	}
}

internal sealed class ComponentFactory<TComponent> : ComponentFactory where TComponent : struct
{
	public ComponentFactory(TComponent value) : base(Component<TComponent>.Type)
	{
		_value = value;
	}

	public override void AddComponent(IList list)
	{
		AddComponent((List<TComponent>)list);
	}

	private void AddComponent(List<TComponent> list)
	{
		list.Add(_value);
	}
	
	private readonly TComponent _value;
}