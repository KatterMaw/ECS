using System.IO;
using System.Linq;
using ECS.Systems;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace ECS.SourceGenerators.Tests.AdaptiveSystemsGenerator;

public sealed class TwoComponentSystemWithEntityTests
{
	private const string ExpectedGeneratorOutput =
		"""
		using System;
		using ECS;
		using ECS.Systems.Queries;
		using Tests.Sample.Components;
		
		namespace Tests.Sample.Systems;
		
		partial class MovementSystem
		{
			public MovementSystem(World world)
			{
				_query = new PredicateArchetypeQuery(world, archetype => archetype.Has<Position>() && archetype.Has<Velocity>());
			}
		
			public void Update()
			{
				for (var archetypeIndex = 0; archetypeIndex < _query.Archetypes.Count; archetypeIndex++)
				{
					Archetype archetype = _query.Archetypes[archetypeIndex];
					Span<Entity> entities = archetype.GetEntitiesSpan();
					Span<Position> positionSpan = archetype.GetSpan<Position>();
					Span<Velocity> velocitySpan = archetype.GetSpan<Velocity>();
					for (int i = 0; i < archetype.EntitiesCount; i++)
					{
						ref Entity entity = ref entities[i];
						ref Position position = ref positionSpan[i];
						ref Velocity velocity = ref velocitySpan[i];
						Update(ref entity, ref position, ref velocity);
					}
				}
			}
		
			private readonly ArchetypeQuery _query;
		}
		""";

	private const string GeneratorInput =
		"""
		using ECS;
		using ECS.Systems;
		using Tests.Sample.Components;
		
		namespace Tests.Sample.Components
		{
			public struct Position
			{
				public int X;
				public int Y;
						
				public Position(int x, int y)
				{
					X = x;
					Y = y;
				}
			}
					
			public struct Velocity
			{
				public int X;
				public int Y;
						
				public Velocity(int x, int y)
				{
					X = x;
					Y = y;
				}
			}
		}
				
		namespace Tests.Sample.Systems
		{
			internal sealed partial class MovementSystem : ISystem
			{
				private void Update(ref Entity entity, ref Position position, ref Velocity velocity)
				{
				}
			}
		}
		""";

	[Fact]
	public void GeneratorOutputShouldBeAsExpected()
	{
		// Create an instance of the source generator.
		SourceGenerators.AdaptiveSystemsGenerator generator = new();

		// Source generators should be tested using 'GeneratorDriver'.
		var driver = CSharpGeneratorDriver.Create(generator);

		// We need to create a compilation with the required source code.
		var compilation = CSharpCompilation.Create(
			nameof(GenericComponentsSystemsGeneratorTests),
			[CSharpSyntaxTree.ParseText(GeneratorInput)],
			[MetadataReference.CreateFromFile(typeof(ISystem).Assembly.Location)]);

		// Run generators and retrieve all results.
		var runResult = driver.RunGenerators(compilation).GetRunResult();

		// All generated files can be found in 'RunResults.GeneratedTrees'.
		var generatedFileSyntax = runResult.GeneratedTrees.Single(t => Path.GetFileName(t.FilePath) == "MovementSystem.g.cs");

		// Complex generators should be tested using text comparison.
		Assert.Equal(ExpectedGeneratorOutput, generatedFileSyntax.GetText().ToString());
	}
}