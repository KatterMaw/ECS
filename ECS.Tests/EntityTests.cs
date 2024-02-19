using ECS.Entities;
using ECS.Systems;
using ECS.Systems.Queries;
using ECS.Tests.Components;

namespace ECS.Tests;

public sealed class EntityTests
{
	[Fact]
	public void ShouldAddComponentToExistingEntity()
	{
		World world = new();
		ref var entity = ref new EntityBuilder()
			.Add<Position>()
			.Build(world);
		entity.Add(new Velocity(10, 0)).Mutate();
		entity.Get<Velocity>().X.Should().Be(10);
	}

	[Fact]
	public void ShouldRemoveComponentFromExistingEntity()
	{
		World world = new();
		ref var entity = ref new EntityBuilder()
			.Add<Position>()
			.Add(new Velocity(10, 0))
			.Build(world);
		entity.Remove<Velocity>().Mutate();
		entity.Has<Velocity>().Should().BeFalse();
	}

	[Fact]
	public void ShouldIterateOnceAfterAdding()
	{
		World world = new();
		ref var entity = ref new EntityBuilder()
			.Add<Position>()
			.Build(world);
		entity.Add(new Velocity(10, 0)).Mutate();
		IncreasingVelocitySystem increasingVelocitySystem = new(world);
		increasingVelocitySystem.Update();
		entity.Get<Velocity>().X.Should().Be(11);
	}

	[Fact]
	public void ShouldNotIterateEntityAfterComponentRemoving()
	{
		World world = new();
		ref var entity = ref new EntityBuilder()
			.Add<Position>()
			.Add(new Velocity(10, 0))
			.Build(world);
		entity.Remove<Velocity>().Mutate();
		NonExecutableSystem<Velocity> nonExecutableSystem = new(world);
		nonExecutableSystem.Update();
	}

	private sealed class IncreasingVelocitySystem : ComponentSystem<Velocity>
	{
		public IncreasingVelocitySystem(World world) : base(world)
		{
		}

		protected override void Update(ref Velocity velocity)
		{
			velocity.X++;
		}
	}

	private sealed class NonExecutableSystem<TComponent> : ComponentSystem<TComponent> where TComponent : struct
	{
		public NonExecutableSystem(World world) : base(world)
		{
		}

		protected override void Update(ref TComponent component)
		{
			throw new InvalidOperationException("Should not be executed");	
		}
	}
}

