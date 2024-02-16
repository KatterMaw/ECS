using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace ECS.SourceGenerators;

[Generator]
public sealed class GenericComponentsSystemsGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var genericsCountProvider = context.SyntaxProvider.CreateSyntaxProvider(IsTargetSyntaxNode, Transform).Where(x => x != 0);
		context.RegisterSourceOutput(genericsCountProvider.Collect(), GenerateComponentsSystems);
	}

	private bool IsTargetSyntaxNode(SyntaxNode node, CancellationToken cancellationToken)
	{
		return node is ClassDeclarationSyntax;
	}

	private int Transform(GeneratorSyntaxContext context, CancellationToken cancellationToken)
	{
		var typeSymbol = context.SemanticModel.GetDeclaredSymbol((ClassDeclarationSyntax)context.Node);
		if (typeSymbol == null)
			return 0;
		var baseType = typeSymbol.BaseType;
		if (baseType == null)
			return 0;
		var baseTypeName = baseType.Name;
		if (baseTypeName != "ComponentsSystem")
			return 0;
		return baseType.TypeArguments.Length;
	}

	private static void GenerateComponentsSystems(SourceProductionContext context, ImmutableArray<int> componentsCounts)
	{
		if (componentsCounts.Length == 0)
			return;
		var source = $$"""
		using ECS.Systems.Queries;
		
		namespace ECS.Systems;
		
		{{string.Join("\n", componentsCounts.Distinct().Select(GenerateComponentsSystem))}}
		""";
		context.AddSource("ComponentsSystem.g.cs", SourceText.From(source, Encoding.UTF8));
	}

	private static string GenerateComponentsSystem(int componentsCount)
	{
		var componentsIndexes = Enumerable.Range(1, componentsCount).ToImmutableList();
		var typeParameters = string.Join(", ", componentsIndexes.Select(i => $"TComponent{i}"));
		var typeConstraints = string.Join("\n\t", componentsIndexes.Select(i => $"where TComponent{i} : struct"));
		var spansObtaining = string.Join("\n\t\t\t", componentsIndexes.Select(i => $"var components{i} = archetype.GetSpan<TComponent{i}>();"));
		var updateMethodArguments = string.Join(", ", componentsIndexes.Select(i => $"ref components{i}[i]"));
		var queryBody = string.Join(" && ", componentsIndexes.Select(i => $"archetype.Has<TComponent{i}>()"));
		var updateMethodParameters = string.Join(", ", componentsIndexes.Select(i => $"ref TComponent{i} component{i}"));
		return $$"""
		         public abstract class ComponentsSystem<{{typeParameters}}> : ISystem
		         	{{typeConstraints}}
		         {
		         	public void Update()
		         	{
		         		PreUpdate();
		         		for (var index = 0; index < _query.Archetypes.Count; index++)
		         		{
		         			var archetype = _query.Archetypes[index];
		         			{{spansObtaining}}
		         			for (int i = 0; i < archetype.EntitiesCount; i++)
		         				Update({{updateMethodArguments}});
		         		}
		         		PostUpdate();
		         	}
		         
		         	protected ComponentsSystem(World world)
		         	{
		         		_query = new PredicateArchetypeQuery(world, archetype => {{queryBody}});
		         	}
		         	
		         	protected virtual void PreUpdate() { }
		         	protected abstract void Update({{updateMethodParameters}});
		         	protected virtual void PostUpdate() { }
		         	
		         	private readonly ArchetypeQuery _query;
		         }
		         """;
	}
}