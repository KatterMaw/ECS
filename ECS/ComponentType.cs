using System.Collections;

namespace ECS;

// ReSharper disable once UnusedTypeParameter
internal sealed class ComponentType
{
	internal static ComponentType Create<T>()
	{
		var newId = Interlocked.Increment(ref _lastId);
		return new ComponentType(newId, () => new List<T>(), (list, i) => ((List<T>)list).EnsureCapacity(list.Count + i));
	}

	private static int _lastId = -1;
	
	public int Id { get; }
	public IList List => _listFactory();

	public void EnsureListRemainingCapacity(IList list, int value)
	{
		_ensureListRemainingCapacity(list, value);
	}
	
	public override int GetHashCode() => Id;

	private readonly Func<IList> _listFactory;
	private readonly Action<IList, int> _ensureListRemainingCapacity;

	private ComponentType(int id, Func<IList> listFactory, Action<IList, int> ensureListRemainingCapacity)
	{
		Id = id;
		_listFactory = listFactory;
		_ensureListRemainingCapacity = ensureListRemainingCapacity;
	}
}