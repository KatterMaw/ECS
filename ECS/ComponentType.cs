using System.Collections;

namespace ECS;

// ReSharper disable once UnusedTypeParameter
internal sealed class ComponentType
{
	internal static ComponentType Create<T>()
	{
		var newId = Interlocked.Increment(ref _lastId);
		return new ComponentType(newId, () => new List<T>());
	}

	private static int _lastId = -1;
	
	public int Id { get; }
	public IList List => _listFactory();

	public override int GetHashCode() => Id;

	private readonly Func<IList> _listFactory;

	private ComponentType(int id, Func<IList> listFactory)
	{
		Id = id;
		_listFactory = listFactory;
	}
}