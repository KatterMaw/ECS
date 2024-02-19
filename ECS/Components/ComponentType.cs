using System.Collections;
using ECS.Entities;

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
	public abstract void AddToBuilder(EntityBuilder builder, IList list, int index);

	public bool Equals(ComponentType other)
	{
		return Id == other.Id;
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(null, obj))
			return false;
		if (ReferenceEquals(this, obj))
			return true;
		if (obj.GetType() != GetType())
			return false;
		return Equals((ComponentType)obj);
	}

	public override int GetHashCode()
	{
		return Id;
	}

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

	public override void AddToBuilder(EntityBuilder builder, IList list, int index)
	{
		AddToBuilder(builder, (List<TComponent>)list, index);
	}

	private static void EnsureRemainingCapacity(List<TComponent> list, int capacity)
	{
		list.EnsureCapacity(capacity);
	}

	private static void AddToBuilder(EntityBuilder builder, List<TComponent> list, int index)
	{
		var component = list[index];
		builder.Add(component);
	}
}