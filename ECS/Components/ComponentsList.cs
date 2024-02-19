using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using CommunityToolkit.Diagnostics;
using ECS.Entities;
using ECS.Extensions;

namespace ECS.Components;

internal sealed class ComponentsList
{
	public int Count => _componentTypes.Length;
	
	public bool Has<TComponent>() where TComponent : struct
	{
		var typeId = Component<TComponent>.Type.Id;
		return _componentsLookup.Length > typeId && _componentsLookup[typeId] != null;
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
	
	internal ComponentsList(IReadOnlyCollection<ComponentType> componentTypes)
	{
		var requiredSize = componentTypes.GetMaxId() + 1;
		var listsBuilder = ImmutableArray.CreateBuilder<IList>(componentTypes.Count);
		var lookupBuilder = ImmutableArray.CreateBuilder<IList?>(requiredSize);
		lookupBuilder.Count = requiredSize;
		foreach (var componentType in componentTypes)
		{
			var list = componentType.CreateList();
			listsBuilder.Add(list);
			lookupBuilder[componentType.Id] = list;
		}
		_componentTypes = componentTypes.ToImmutableArray();
		_componentsLists = listsBuilder.ToImmutable();
		_componentsLookup = lookupBuilder.ToImmutable();
	}

	internal void AddComponents(IEnumerable<ComponentFactory> componentFactories)
	{
		foreach (var componentFactory in componentFactories)
		{
			var list = _componentsLookup[componentFactory.ComponentType.Id];
			Guard.IsNotNull(list);
			componentFactory.AddComponent(list);
		}
	}

	internal ref TComponent Get<TComponent>(int index) where TComponent : struct
	{
		return ref GetSpan<TComponent>()[index];
	}

	internal void Remove(int index)
	{
		foreach (var list in _componentsLists)
			list.RemoveAt(index);
	}

	internal void EnsureRemainingCapacity(int capacity)
	{
		foreach (var type in _componentTypes)
		{
			var list = _componentsLookup[type.Id];
			Guard.IsNotNull(list);
			type.EnsureRemainingCapacity(list, capacity);
		}
	}

	internal void AddToBuilder(EntityBuilder builder, int index)
	{
		for (int i = 0; i < Count; i++)
		{
			var type = _componentTypes[i];
			var list = _componentsLists[i];
			type.AddToBuilder(builder, list, index);
		}
	}

	private readonly ImmutableArray<ComponentType> _componentTypes;
	private readonly ImmutableArray<IList> _componentsLists;
	private readonly ImmutableArray<IList?> _componentsLookup;

	private List<TComponent> GetList<TComponent>() where TComponent : struct
	{
		var list = _componentsLookup[Component<TComponent>.Type.Id];
		Guard.IsNotNull(list);
		return (List<TComponent>)list;
	}

	private bool TryGetList<TComponent>([NotNullWhen(true)] out List<TComponent>? list) where TComponent : struct
	{
		list = null;
		var index = Component<TComponent>.Type.Id;
		if (_componentsLookup.Length <= index)
			return false;
		list = _componentsLookup[index] as List<TComponent>;
		return list != null;
	}
}