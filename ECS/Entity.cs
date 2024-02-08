namespace ECS;

public readonly struct Entity
{
	internal Entity(Archetype archetype, int index)
	{
		_archetype = archetype;
		_index = index;
	}

	public ref TComponent Get<TComponent>() where TComponent : struct
	{
		return ref _archetype.Get<TComponent>(_index);
	}
	
	private readonly Archetype _archetype;
	private readonly int _index;
}