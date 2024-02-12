namespace ECS.Systems;

public sealed class ParallelSystem : ISystem
{
	public ParallelSystem(params ISystem[] systems)
	{
		_actions = systems.Select<ISystem, Action>(system => system.Update).ToArray();
	}
	
	public void Update()
	{
		Parallel.Invoke(_actions);
	}
	
	private readonly Action[] _actions;
}