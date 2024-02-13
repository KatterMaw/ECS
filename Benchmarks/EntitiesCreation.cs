using Arch.Core.Utils;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Diagnostics.dotTrace;
using Benchmarks.Components;
using ECS;

namespace Benchmarks;

[MemoryDiagnoser]
[DotTraceDiagnoser]
[HardwareCounters(HardwareCounter.CacheMisses)]
public class EntitiesCreation : Benchmark
{
	[Benchmark(Baseline = true)]
	public void NoECS()
	{
		var entities = new Character[EntitiesCount];
		for (int i = 0; i < EntitiesCount; i++)
			entities[i] = new Character();
	}
	
	[Benchmark]
	public void ECS()
	{
		World world = new();
		EntityBuilder entityBuilder = new();
		entityBuilder.Add<Health>().Add<Position>().Add(new Velocity(10, 0));
		entityBuilder.Build(world, EntitiesCount);
	}

	[Benchmark]
	public void Arch()
	{
		var world = global::Arch.Core.World.Create();
		var archetype = new ComponentType[]{ typeof(Position), typeof(Velocity), typeof(Health) };
		world.Reserve(archetype, EntitiesCount);
		for (int i = 0; i < EntitiesCount; i++)
			world.Create(new Health(), new Position(), new Velocity(10, 0));
		world.Dispose();
	}
}