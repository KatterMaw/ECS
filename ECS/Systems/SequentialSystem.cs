namespace ECS.Systems;

public sealed class SequentialSystem : ISystem
{
	public SequentialSystem(params ISystem[] systems)
	{
		_systems = systems;
	}
	
	public void Update()
	{
		foreach (var system in _systems)
			system.Update();
	}
	
	private readonly ISystem[] _systems;
}