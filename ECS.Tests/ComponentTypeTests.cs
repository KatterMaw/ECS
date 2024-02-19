using ECS.Components;
using ECS.Tests.Components;

namespace ECS.Tests;

public sealed class ComponentTypeTests
{
	[Fact]
	public void ShouldCreateComponentTypesWithDifferentIds()
	{
		Component<Health>.Type.Should().NotBe(Component<Velocity>.Type);
	}
}