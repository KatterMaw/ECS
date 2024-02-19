using System.Collections;

namespace ECS;

internal abstract class ComponentType
{
	internal static ComponentType Create<T>() where T : struct
	{
		return new ComponentType<T>();
	}

	private static int _lastId = -1;
	
	public int Id { get; }
	
	public abstract IList CreateList();
	public abstract void EnsureRemainingCapacity(IList list, int capacity);
	public override int GetHashCode() => Id;

	protected ComponentType()
	{
		Id = Interlocked.Increment(ref _lastId);;
	}
}

internal sealed class ComponentType<T> : ComponentType where T : struct
{
	public override IList CreateList()
	{
		return new List<T>();
	}

	public override void EnsureRemainingCapacity(IList list, int capacity)
	{
		EnsureRemainingCapacity((List<T>)list, capacity);
	}

	private static void EnsureRemainingCapacity(List<T> list, int capacity)
	{
		list.EnsureCapacity(capacity);
	}
}