using ECS.Entities;
using ECS.Systems;
using ECS.Systems.Queries;
using ECS.Tests.Components;

namespace ECS.Tests;

public sealed class SystemsTests
{
	[Fact]
	public void ShouldChangePosition()
	{
		World world = new();
		MovementSystem movementSystem = new(world);
		EntityBuilder entityBuilder = new();
		entityBuilder.Add<Position>().Add(new Velocity(10, 0));
		var entity = entityBuilder.Build(world);
		entity.Get<Position>().X.Should().Be(0);
		movementSystem.Update();
		entity.Get<Position>().X.Should().Be(10);
	}
	
	private sealed class MovementSystem : EntitySystem
	{
		public MovementSystem(World world) : base(new PredicateArchetypeQuery(world, archetype => archetype.Has<Position>() && archetype.Has<Velocity>()))
		{
		}

		protected override void Update(ref readonly Entity entity)
		{
			ref var position = ref entity.Get<Position>();
			ref var velocity = ref entity.Get<Velocity>();
			position.X += velocity.X;
			position.Y += velocity.Y;
		}
	}
}