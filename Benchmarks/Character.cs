using System.Numerics;

namespace Benchmarks;

internal sealed class Character
{
	public Vector2 Position { get; set; }
	public Vector2 Velocity { get; set; } = new(10, 0);
	public float Health { get; set; } = 100;

	public void Update()
	{
		Position += Velocity;
	}
}