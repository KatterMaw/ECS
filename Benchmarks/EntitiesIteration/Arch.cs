using Arch.Core;
using Arch.Core.Utils;
using BenchmarkDotNet.Attributes;
using Benchmarks.Components;

namespace Benchmarks;

public partial class EntitiesIteration
{
	[GlobalSetup(Target = nameof(Arch))]
	public void SetupArch()
	{
		_archWorld = World.Create();
		var archetype = new ComponentType[]{ typeof(Position), typeof(Velocity), typeof(Health) };
		_archWorld.Reserve(archetype, EntitiesCount);
		for (int i = 0; i < EntitiesCount; i++)
			_archWorld.Create(new Health(), new Position(), new Velocity(10, 0));
		_archQuery = new QueryDescription().WithAll<Position, Velocity>();
	}

	[Benchmark]
	public void Arch()
	{
		_archWorld.Query(in _archQuery, (ref Position position, ref Velocity velocity) =>
		{
			position.X += velocity.X;
			position.Y += velocity.Y;
		});
	}

	[GlobalCleanup(Target = nameof(Arch))]
	public void CleanupArch()
	{
		_archWorld.Dispose();
	}

	private World _archWorld;
	private QueryDescription _archQuery;
}