using System.Collections;

namespace ECS.Components;

internal abstract class ComponentType
{
	internal static ComponentType Create<T>() where T : struct
	{
		return new ComponentType<T>();
	}

	private static int _lastId = -1;
	
	public int Id { get; }
	public abstract ComponentFactory DefaultFactory { get; }
	
	public abstract IList CreateList();
	public abstract void EnsureRemainingCapacity(IList list, int capacity);
	public override int GetHashCode() => Id;

	protected ComponentType()
	{
		Id = Interlocked.Increment(ref _lastId);;
	}
}

internal sealed class ComponentType<TComponent> : ComponentType where TComponent : struct
{
	public override ComponentFactory DefaultFactory => ComponentFactory<TComponent>.Default;

	public override IList CreateList()
	{
		return new List<TComponent>();
	}

	public override void EnsureRemainingCapacity(IList list, int capacity)
	{
		EnsureRemainingCapacity((List<TComponent>)list, capacity);
	}

	private static void EnsureRemainingCapacity(List<TComponent> list, int capacity)
	{
		list.EnsureCapacity(capacity);
	}
}