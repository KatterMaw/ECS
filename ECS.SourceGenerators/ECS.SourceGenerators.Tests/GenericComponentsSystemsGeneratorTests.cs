using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace ECS.SourceGenerators.Tests;

public sealed class GenericComponentsSystemsGeneratorTests
{
	private const string ExpectedTwoAndThreeComponentsSystemClassesCode =
		"""
		using ECS.Systems.Queries;

		namespace ECS.Systems;

		public abstract class ComponentsSystem<TComponent1, TComponent2> : ISystem
			where TComponent1 : struct
			where TComponent2 : struct
		{
			public void Update()
			{
				PreUpdate();
				for (var index = 0; index < _query.Archetypes.Count; index++)
				{
					var archetype = _query.Archetypes[index];
					var components1 = archetype.GetSpan<TComponent1>();
					var components2 = archetype.GetSpan<TComponent2>();
					for (int i = 0; i < archetype.EntitiesCount; i++)
						Update(ref components1[i], ref components2[i]);
				}
				PostUpdate();
			}
		
			protected ComponentsSystem(World world)
			{
				_query = new PredicateArchetypeQuery(world, archetype => archetype.Has<TComponent1>() && archetype.Has<TComponent2>());
			}
		
			protected virtual void PreUpdate() { }
			protected abstract void Update(ref TComponent1 component1, ref TComponent2 component2);
			protected virtual void PostUpdate() { }
		
			private readonly ArchetypeQuery _query;
		}

		public abstract class ComponentsSystem<TComponent1, TComponent2, TComponent3> : ISystem
			where TComponent1 : struct
			where TComponent2 : struct
			where TComponent3 : struct
		{
			public void Update()
			{
				PreUpdate();
				for (var index = 0; index < _query.Archetypes.Count; index++)
				{
					var archetype = _query.Archetypes[index];
					var components1 = archetype.GetSpan<TComponent1>();
					var components2 = archetype.GetSpan<TComponent2>();
					var components3 = archetype.GetSpan<TComponent3>();
					for (int i = 0; i < archetype.EntitiesCount; i++)
						Update(ref components1[i], ref components2[i], ref components3[i]);
				}
				PostUpdate();
			}
		
			protected ComponentsSystem(World world)
			{
				_query = new PredicateArchetypeQuery(world, archetype => archetype.Has<TComponent1>() && archetype.Has<TComponent2>() && archetype.Has<TComponent3>());
			}
		
			protected virtual void PreUpdate() { }
			protected abstract void Update(ref TComponent1 component1, ref TComponent2 component2, ref TComponent3 component3);
			protected virtual void PostUpdate() { }
		
			private readonly ArchetypeQuery _query;
		}
		""";

	private const string TwoComponentsSystemClass =
		"""
		using Benchmarks.Components;
		using ECS;
		using ECS.Systems;

		namespace Benchmarks;

		public sealed class MovementSystem : ComponentsSystem<Position, Velocity>
		{
			public MovementSystem(World world) : base(world)
			{
			}
		
			protected override void Update(ref Position position, ref Velocity velocity)
			{
				position.X += velocity.X;
				position.Y += velocity.Y;
			}
		}
		""";

	private const string ThreeComponentsSystemClass =
		"""
		using Benchmarks.Components;
		using ECS;
		using ECS.Systems;

		namespace Benchmarks;

		public sealed class AnotherSystem : ComponentsSystem<Position, Velocity, Health>
		{
			public MovementSystem(World world) : base(world)
			{
			}
		
			protected override void Update(ref Position position, ref Velocity velocity, ref Health health)
			{
			}
		}
		""";

	[Fact]
	public void OutputShouldBeAsExpected()
	{
		// Create an instance of the source generator.
		GenericComponentsSystemsGenerator generator = new();

		// Source generators should be tested using 'GeneratorDriver'.
		var driver = CSharpGeneratorDriver.Create(generator);

		// We need to create a compilation with the required source code.
		var compilation = CSharpCompilation.Create(nameof(GenericComponentsSystemsGeneratorTests),
			new[]
			{
				CSharpSyntaxTree.ParseText(TwoComponentsSystemClass),
				CSharpSyntaxTree.ParseText(ThreeComponentsSystemClass)
			},
			Array.Empty<PortableExecutableReference>());

		// Run generators and retrieve all results.
		var runResult = driver.RunGenerators(compilation).GetRunResult();

		// All generated files can be found in 'RunResults.GeneratedTrees'.
		var generatedFileSyntax = runResult.GeneratedTrees.Single(t => Path.GetFileName(t.FilePath) == "ComponentsSystem.g.cs");

		// Complex generators should be tested using text comparison.
		Assert.Equal(
			ExpectedTwoAndThreeComponentsSystemClassesCode,
			generatedFileSyntax.GetText().ToString(),
			ignoreLineEndingDifferences: true);
	}
}