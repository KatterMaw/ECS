using ECS.Entities;
using ECS.Tests.Components;

namespace ECS.Tests;

public sealed class WorldAccessors
{
	[Fact]
	public void ShouldCreateAndGetComponents()
	{
		World world = new();
		EntityBuilder entityBuilder = new();
		var entity = entityBuilder.Add(new Health(100)).Build(world);
		entity.Get<Health>().Value.Should().Be(100);
	}

	[Fact]
	public void ShouldChangeComponentValue()
	{
		World world = new();
		EntityBuilder entityBuilder = new();
		var entity = entityBuilder.Add(new Health(100)).Build(world);
		entity.Get<Health>().Value = 10;
		entity.Get<Health>().Value.Should().Be(10);
	}

	[Fact]
	public void ShouldAddTwoEntities()
	{
		World world = new();
		EntityBuilder entityBuilder = new();
		entityBuilder.Add(new Health(100));
		entityBuilder.Build(world);
		entityBuilder.Build(world);
	}
}