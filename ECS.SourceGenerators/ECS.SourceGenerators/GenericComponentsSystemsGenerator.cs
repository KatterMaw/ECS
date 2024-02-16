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
		componentsCounts = componentsCounts.Distinct().ToImmutableArray();
		StringBuilder stringBuilder = new();
		stringBuilder.AppendLine("using ECS.Systems.Queries;")
			.AppendLine()
			.AppendLine("namespace ECS.Systems;")
			.AppendLine();
		foreach (var componentsCount in componentsCounts)
		{
			GenerateComponentsSystem(stringBuilder, componentsCount);
			if (componentsCount != componentsCounts[componentsCounts.Length - 1])
				stringBuilder.AppendLine().AppendLine();
		}

		context.AddSource("ComponentsSystem.g.cs", SourceText.From(stringBuilder.ToString(), Encoding.UTF8));
	}

	private static void GenerateComponentsSystem(StringBuilder stringBuilder, int componentsCount)
	{
		ImmutableArray<int> componentIndexes = Enumerable.Range(1, componentsCount).ToImmutableArray();
		
		void AppendTypeParameters()
		{
			foreach (var componentIndex in componentIndexes)
			{
				stringBuilder.Append("TComponent").Append(componentIndex);
				if (componentIndex != componentIndexes[componentIndexes.Length - 1])
					stringBuilder.Append(", ");
			}
		}

		void AppendTypeConstraints()
		{
			foreach (var componentIndex in componentIndexes)
				stringBuilder.Append("\twhere TComponent").Append(componentIndex).AppendLine(" : struct");
		}

		void AppendSpansObtaining()
		{
			foreach (var componentIndex in componentIndexes)
				stringBuilder
					.Append("\t\t\tvar components")
					.Append(componentIndex)
					.Append(" = archetype.GetSpan<TComponent")
					.Append(componentIndex)
					.AppendLine(">();");
		}

		void AppendUpdateMethodArguments()
		{
			foreach (var componentIndex in componentIndexes)
			{
				stringBuilder.Append("ref components").Append(componentIndex).Append("[i]");
				if (componentIndex != componentIndexes[componentIndexes.Length - 1])
					stringBuilder.Append(", ");
			}
		}

		void AppendQueryBody()
		{
			foreach (var componentIndex in componentIndexes)
			{
				stringBuilder.Append("archetype.Has<TComponent").Append(componentIndex).Append(">()");
				if (componentIndex != componentIndexes[componentIndexes.Length - 1])
					stringBuilder.Append(" && ");
			}
		}

		void AppendUpdateMethodParameters()
		{
			foreach (var componentIndex in componentIndexes)
			{
				stringBuilder
					.Append("ref TComponent")
					.Append(componentIndex)
					.Append(" component")
					.Append(componentIndex);
				if (componentIndex != componentIndexes[componentIndexes.Length - 1])
					stringBuilder.Append(", ");
			}
		}

		stringBuilder.Append("public abstract class ComponentsSystem<");
		AppendTypeParameters();
		stringBuilder.AppendLine("> : ISystem");
		AppendTypeConstraints();
		stringBuilder
			.AppendLine("{")
			.AppendLine("\tpublic void Update()")
			.AppendLine("\t{")
			.AppendLine("\t\tPreUpdate();")
			.AppendLine("\t\tfor (var index = 0; index < _query.Archetypes.Count; index++)")
			.AppendLine("\t\t{")
			.AppendLine("\t\t\tvar archetype = _query.Archetypes[index];");
		AppendSpansObtaining();
		stringBuilder
			.AppendLine("\t\t\tfor (int i = 0; i < archetype.EntitiesCount; i++)")
			.Append("\t\t\t\tUpdate(");
		AppendUpdateMethodArguments();
		stringBuilder
			.AppendLine(");")
			.AppendLine("\t\t}")
			.AppendLine("\t\tPostUpdate();")
			.AppendLine("\t}")
			.AppendLine()
			.AppendLine("\tprotected ComponentsSystem(World world)")
			.AppendLine("\t{")
			.Append("\t\t_query = new PredicateArchetypeQuery(world, archetype => ");
		AppendQueryBody();
		stringBuilder
			.AppendLine(");")
			.AppendLine("\t}")
			.AppendLine()
			.AppendLine("\tprotected virtual void PreUpdate() { }")
			.Append("\tprotected abstract void Update(");
		AppendUpdateMethodParameters();
		stringBuilder.AppendLine(");")
			.AppendLine("\tprotected virtual void PostUpdate() { }")
			.AppendLine()
			.AppendLine("\tprivate readonly ArchetypeQuery _query;")
			.Append("}");
	}
}