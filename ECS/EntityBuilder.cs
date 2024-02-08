using System.Collections;
using CommunityToolkit.Diagnostics;

namespace ECS;

public sealed class EntityBuilder
{
	internal sealed class ComponentFactory
	{
		public static ComponentFactory Create<TComponent>(TComponent component) where TComponent : struct
		{
			var componentType = ComponentType.Get<TComponent>();
			return new ComponentFactory(componentType, AddComponent);
			void AddComponent(IList list) => ((IList<TComponent>)list).Add(component);
		}
		
		public ComponentType Type { get; }
		public Action<IList> AddComponent { get; }

		private ComponentFactory(ComponentType type, Action<IList> addComponent)
		{
			Type = type;
			AddComponent = addComponent;
		}
	}

	public EntityBuilder(bool buildArchetype = true)
	{
		if (buildArchetype)
			_archetypeBuilder = new ArchetypeBuilder();
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
	private readonly ArchetypeBuilder? _archetypeBuilder;

	private Archetype GetOrCreateArchetype(World world, ArchetypeDescription archetypeDescription)
	{
		if (world.Archetypes.TryGetValue(archetypeDescription, out var archetype))
			return archetype;
		Guard.IsNotNull(_archetypeBuilder);
		return _archetypeBuilder.Build(world);
	}
}