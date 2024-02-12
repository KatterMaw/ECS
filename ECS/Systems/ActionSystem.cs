namespace ECS.Systems;

public sealed class ActionSystem : ISystem
{
	public ActionSystem(Action action)
	{
		_action = action;
	}
	
	public void Update()
	{
		_action();
	}
	
	private readonly Action _action;
}