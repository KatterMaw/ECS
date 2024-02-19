using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using CommunityToolkit.Diagnostics;
using ECS.Extensions;

namespace ECS;

internal sealed class ComponentsList
{
	internal ComponentsList(IReadOnlyCollection<ComponentType> componentTypes)
	{
		_componentTypes = componentTypes.ToImmutableArray();
		var requiredSize = componentTypes.GetMaxId() + 1;
		var builder = ImmutableArray.CreateBuilder<IList?>(requiredSize);
		builder.Count = requiredSize;
		foreach (var componentType in componentTypes)
			builder[componentType.Id] = componentType.CreateList();
		_componentLists = builder.ToImmutable();
	}
	
	public bool Has<TComponent>() where TComponent : struct
	{
		return _componentLists[Component<TComponent>.Type.Id] != null;
	}

	public Span<TComponent> GetSpan<TComponent>() where TComponent : struct
	{
		return CollectionsMarshal.AsSpan(GetList<TComponent>());
	}

	public bool TryGetSpan<TComponent>(out Span<TComponent> span) where TComponent : struct
	{
		span = Span<TComponent>.Empty;
		if (!TryGetList<TComponent>(out var list))
			return false;
		span = CollectionsMarshal.AsSpan(list);
		return true;
	}

	public Span<TComponent> GetOptionalSpan<TComponent>() where TComponent : struct
	{
		if (TryGetSpan<TComponent>(out var span))
			return span;
		return Span<TComponent>.Empty;
	}

	internal void AddComponents(List<ComponentFactory> componentFactories)
	{
		foreach (var componentFactory in componentFactories)
		{
			var list = _componentLists[componentFactory.ComponentType.Id];
			Guard.IsNotNull(list);
			componentFactory.AddComponent(list);
		}
	}

	internal ref TComponent Get<TComponent>(int index) where TComponent : struct
	{
		return ref GetSpan<TComponent>()[index];
	}

	internal void EnsureRemainingCapacity(int capacity)
	{
		foreach (var type in _componentTypes)
		{
			var list = _componentLists[type.Id];
			Guard.IsNotNull(list);
			type.EnsureRemainingCapacity(list, capacity);
		}
	}
	
	private readonly ImmutableArray<IList?> _componentLists;
	private readonly ImmutableArray<ComponentType> _componentTypes;

	private List<TComponent> GetList<TComponent>() where TComponent : struct
	{
		var list = _componentLists[Component<TComponent>.Type.Id];
		Guard.IsNotNull(list);
		return (List<TComponent>)list;
	}

	private bool TryGetList<TComponent>([NotNullWhen(true)] out List<TComponent>? list) where TComponent : struct
	{
		list = null;
		var index = Component<TComponent>.Type.Id;
		if (_componentLists.Length <= index)
			return false;
		list = _componentLists[index] as List<TComponent>;
		return list != null;
	}
}