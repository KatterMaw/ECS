using System.Reactive.Disposables;

namespace ECS.Systems.Queries;

public abstract class ArchetypeQuery : IDisposable
{
	internal IReadOnlyList<Archetype> Archetypes
	{
		get
		{
			if (!_isInitialized)
				Initialize();
			return _archetypes;
		}
	}

	public void Dispose()
	{
		_disposable.Dispose();
		GC.SuppressFinalize(this);
	}
	
	protected internal ArchetypeQuery(World world)
	{
		_world = world;
	}

	private void Initialize()
	{
		foreach (var archetype in _world.Archetypes)
		{
			if (IsSuitable(archetype))
				_archetypes.Add(archetype);
		}
		_world.ArchetypeAdded.Subscribe(archetype =>
		{
			if (IsSuitable(archetype))
				_archetypes.Add(archetype);
		});
		_isInitialized = true;
	}

	protected abstract bool IsSuitable(Archetype archetype);

	private readonly World _world;
	private readonly List<Archetype> _archetypes = new();
	private readonly CompositeDisposable _disposable = new();
	private bool _isInitialized;
}