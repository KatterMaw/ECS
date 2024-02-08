using System.Collections;
using System.Collections.Immutable;

namespace ECS;

public sealed class EntityBuilder
{
	internal sealed class ComponentFactory
	{
		public static ComponentFactory Create<TComponent>(TComponent component) where TComponent : struct
		{
			var componentType = ComponentType.Get<TComponent>();
			return new ComponentFactory(componentType, CreateList, AddComponent);
			List<TComponent> CreateList() => new();
			void AddComponent(IList list) => ((IList<TComponent>)list).Add(component);
		}
		
		public ComponentType Type { get; }
		public Func<IList> CreateList { get; }
		public Action<IList> AddComponent { get; }

		private ComponentFactory(ComponentType type, Func<IList> createList, Action<IList> addComponent)
		{
			Type = type;
			CreateList = createList;
			AddComponent = addComponent;
		}
	}
	
	public EntityBuilder Add<TComponent>(TComponent component = default) where TComponent : struct
	{
		_componentFactories.Add(ComponentType.Get<TComponent>(), ComponentFactory.Create(component));
		return this;
	}

	public ref Entity Build(World world)
	{
		ArchetypeDescription archetypeDescription = new(_componentFactories.Keys);
		var archetype = GetOrCreateArchetype(world, archetypeDescription);
		return ref archetype.CreateEntity(_componentFactories.Values);
	}

	private readonly Dictionary<ComponentType, ComponentFactory> _componentFactories = new();

	private Archetype GetOrCreateArchetype(World world, ArchetypeDescription archetypeDescription)
	{
		if (world.Archetypes.TryGetValue(archetypeDescription, out var archetype))
			return archetype;
		return BuildArchetype(world, archetypeDescription);
	}

	private Archetype BuildArchetype(World world, ArchetypeDescription archetypeDescription)
	{
		var componentLists = _componentFactories.ToImmutableDictionary(
			pair => pair.Key,
			pair => pair.Value.CreateList());
		Archetype archetype = new(componentLists);
		world.Archetypes.Add(archetypeDescription, archetype);
		return archetype;
	}
}