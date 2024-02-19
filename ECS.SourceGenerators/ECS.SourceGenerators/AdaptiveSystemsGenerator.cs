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
public sealed class AdaptiveSystemsGenerator : IIncrementalGenerator
{
	private sealed class SystemData
	{
		public INamespaceSymbol Namespace { get; }
		public string Name { get; }
		public ImmutableArray<ComponentData> Components { get; }
		public bool PassEntity { get; }
		public bool HasPreUpdate { get; }
		public bool HasPostUpdate { get; }

		public SystemData(
			INamespaceSymbol namespaceSymbol,
			string name,
			ImmutableArray<ComponentData> components,
			bool passEntity,
			bool hasPreUpdate,
			bool hasPostUpdate)
		{
			Namespace = namespaceSymbol;
			Name = name;
			PassEntity = passEntity;
			Components = components;
			HasPreUpdate = hasPreUpdate;
			HasPostUpdate = hasPostUpdate;
		}
	}

	private sealed class ComponentData
	{
		public INamespaceSymbol Namespace { get; }
		public string Name { get; }
		public bool IsOptional { get; }

		public ComponentData(INamespaceSymbol namespaceSymbol, string name, bool isOptional)
		{
			Namespace = namespaceSymbol;
			Name = name;
			IsOptional = isOptional;
		}
	}
	
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var provider = context.SyntaxProvider
			.CreateSyntaxProvider(IsTargetSyntaxNode, Transform)
			.Where(x => x != null)
			.Select((data, _) => data!);
		context.RegisterSourceOutput(provider, GenerateSystemBackend);
	}

	private static bool IsTargetSyntaxNode(SyntaxNode node, CancellationToken cancellationToken)
	{
		return node is MethodDeclarationSyntax { Identifier.ValueText: "Update", ParameterList.Parameters.Count: > 0 };
	}

	private static SystemData? Transform(GeneratorSyntaxContext syntaxContext, CancellationToken cancellationToken)
	{
		var updateMethodNode = (MethodDeclarationSyntax)syntaxContext.Node;
		var declaredMethodSymbol = syntaxContext.SemanticModel.GetDeclaredSymbol(updateMethodNode);
		if (declaredMethodSymbol == null)
			return null;
		var declaredClass = updateMethodNode.Parent as ClassDeclarationSyntax;
		if (declaredClass == null)
			return null;
		var declaredClassSymbol = syntaxContext.SemanticModel.GetDeclaredSymbol(declaredClass);
		if (declaredClassSymbol == null)
			return null;
		if (declaredClassSymbol.Interfaces.All(i => i.Name != "ISystem"))
			return null;
		var componentParameters = declaredMethodSymbol.Parameters.AsEnumerable();
		var passEntity = declaredMethodSymbol.Parameters.First().Type.Name == "Entity";
		if (passEntity)
			componentParameters = componentParameters.Skip(1);
		var components = componentParameters.Select(parameter =>
			{
				var isNullable = parameter.NullableAnnotation == NullableAnnotation.Annotated;
				var type = isNullable ? ((INamedTypeSymbol)parameter.Type).TypeArguments.Single() : parameter.Type;
				return new ComponentData(
					type.ContainingNamespace,
					type.Name,
					isNullable);
			})
			.ToImmutableArray();
		var methods = declaredClassSymbol.GetMembers().OfType<IMethodSymbol>().ToImmutableArray();
		var hasPreUpdate = methods.Any(method => method.Name == "PreUpdate");
		var hasPostUpdate = methods.Any(method => method.Name == "PostUpdate");
		
		return new SystemData(
			declaredClassSymbol.ContainingNamespace,
			declaredClassSymbol.Name,
			components,
			passEntity,
			hasPreUpdate,
			hasPostUpdate);
	}

	private static void GenerateSystemBackend(SourceProductionContext context, SystemData data)
	{
		StringBuilder stringBuilder = new();

		void AppendComponentsNamespaceUsings()
		{
			var namespaces = data.Components.Select(component => component.Namespace.ToDisplayString()).Distinct();
			foreach (var componentNamespace in namespaces)
				stringBuilder.Append("using ").Append(componentNamespace).AppendLine(";");
		}

		void AppendQueryBody()
		{
			foreach (var component in data.Components)
			{
				stringBuilder.Append("archetype.Has<").Append(component.Name).Append(">()");
				if (component != data.Components[data.Components.Length - 1])
					stringBuilder.Append(" && ");
			}
		}

		void AppendComponentSpansObtaining()
		{
			foreach (var component in data.Components)
			{
				var method = component.IsOptional ? "GetOptionalSpan" : "GetSpan";
				stringBuilder.Append("\t\t\tSpan<")
					.Append(component.Name)
					.Append("> ")
					.Append(component.Name.ToLower())
					.Append("Span = archetype.")
					.Append(method)
					.Append("<")
					.Append(component.Name)
					.AppendLine(">();");
			}
		}

		void AppendComponentsObtaining()
		{
			foreach (var component in data.Components)
			{
				if (component.IsOptional)
					stringBuilder
						.Append("\t\t\t\t").Append(component.Name).Append("? ").Append(component.Name.ToLower()).AppendLine("DefaultValue = null;")
						.Append("\t\t\t\tref ").Append(component.Name).Append("? ").Append(component.Name.ToLower()).Append(" = ref ").Append(component.Name.ToLower()).AppendLine("DefaultValue;")
						.Append("\t\t\t\tif (").Append(component.Name.ToLower()).AppendLine("Span.Length != 0)")
						.AppendLine("\t\t\t\t{")
						.Append("\t\t\t\t\tref ").Append(component.Name).Append(" ").Append(component.Name.ToLower()).Append("Temp = ref ").Append(component.Name.ToLower()).AppendLine("Span[i];")
						.Append("\t\t\t\t\t").Append(component.Name.ToLower()).Append(" = ").Append(component.Name.ToLower()).AppendLine("Temp;")
						.AppendLine("\t\t\t\t}");
				else
					stringBuilder
						.Append("\t\t\t\tref ")
						.Append(component.Name)
						.Append(" ")
						.Append(component.Name.ToLower())
						.Append(" = ref ")
						.Append(component.Name.ToLower())
						.AppendLine("Span[i];");
			}
		}

		void AppendUpdateMethodArguments()
		{
			if (data.PassEntity)
			{
				stringBuilder.Append("ref entity");
				if (data.Components.Any())
					stringBuilder.Append(", ");
			}

			foreach (var component in data.Components)
			{
				stringBuilder.Append("ref ").Append(component.Name.ToLower());
				if (component != data.Components[data.Components.Length - 1])
					stringBuilder.Append(", ");
			}
		}

		stringBuilder
			.AppendLine("using System;")
			.AppendLine("using ECS;")
			.AppendLine("using ECS.Entities;")
			.AppendLine("using ECS.Systems.Queries;");
		AppendComponentsNamespaceUsings();
		stringBuilder.AppendLine();
		if (!data.Namespace.IsGlobalNamespace)
			stringBuilder.Append("namespace ").Append(data.Namespace.ToDisplayString()).AppendLine(";").AppendLine();
		stringBuilder.Append("partial class ").AppendLine(data.Name);
		stringBuilder.AppendLine("{");
		stringBuilder
			.Append("\tpublic ")
			.Append(data.Name)
			.AppendLine("(World world)")
			.AppendLine("\t{")
			.Append("\t\t_query = new PredicateArchetypeQuery(world, archetype => ");
		AppendQueryBody();
		stringBuilder
			.AppendLine(");")
			.AppendLine("\t}")
			.AppendLine()
			.AppendLine("\tpublic void Update()")
			.AppendLine("\t{");
		if (data.HasPreUpdate)
			stringBuilder.AppendLine("\t\tPreUpdate();");
		stringBuilder
			.AppendLine("\t\tfor (var archetypeIndex = 0; archetypeIndex < _query.Archetypes.Count; archetypeIndex++)")
			.AppendLine("\t\t{")
			.AppendLine("\t\t\tArchetype archetype = _query.Archetypes[archetypeIndex];");
		if (data.PassEntity)
			stringBuilder.AppendLine("\t\t\tSpan<Entity> entities = archetype.GetEntitiesSpan();");
		AppendComponentSpansObtaining();
		stringBuilder
			.AppendLine("\t\t\tfor (int i = 0; i < archetype.EntitiesCount; i++)")
			.AppendLine("\t\t\t{");
		if (data.PassEntity)
			stringBuilder.AppendLine("\t\t\t\tref Entity entity = ref entities[i];");
		AppendComponentsObtaining();
		stringBuilder.Append("\t\t\t\tUpdate(");
		AppendUpdateMethodArguments();
		stringBuilder
			.AppendLine(");")
			.AppendLine("\t\t\t}")
			.AppendLine("\t\t}");
		if (data.HasPostUpdate)
			stringBuilder.AppendLine("\t\tPostUpdate();");
		stringBuilder
			.AppendLine("\t}")
			.AppendLine()
			.AppendLine("\tprivate readonly ArchetypeQuery _query;")
			.Append("}");
		
		context.AddSource($"{data.Name}.g.cs", SourceText.From(stringBuilder.ToString(), Encoding.UTF8));
	}
}