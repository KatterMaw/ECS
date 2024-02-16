﻿using BenchmarkDotNet.Attributes;
using Benchmarks.Components;
using ECS;
using ECS.Systems;

namespace Benchmarks;

public partial class EntitiesIteration
{
	[GlobalSetup(Target = nameof(ECS))]
	public void SetupECS()
	{
		_ecsWorld = new();
		EntityBuilder builder = new();
		builder.Add<Position>().Add<Health>().Add(new Velocity(10, 0));
		builder.Build(_ecsWorld, EntitiesCount);
		_ecsSystem = new ECSFastMovementSystem(_ecsWorld);
	}

	[Benchmark]
	public void ECS()
	{
		_ecsSystem.Update();
	}

	private World _ecsWorld;
	private ECSFastMovementSystem _ecsSystem;

	private sealed class ECSFastMovementSystem : ComponentsSystem<Position, Velocity>
	{
		public ECSFastMovementSystem(World world) : base(world)
		{
		}

		protected override void Update(ref Position position, ref Velocity velocity)
		{
			position.X += velocity.X;
			position.Y += velocity.Y;
		}
	}
}