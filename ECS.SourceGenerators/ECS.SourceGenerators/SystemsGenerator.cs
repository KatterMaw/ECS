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
public sealed class SystemsGenerator : IIncrementalGenerator
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
		var componentNamespaces = string.Join("\n", data.Components.Select(component => $"using {component.Namespace.ToDisplayString()};").Distinct());
		var archetypePredicateBody = string.Join(" && ", data.Components.Select(component => $"archetype.Has<{component.Name}>()"));
		const string entitiesSpanObtaining = "var entities = archetype.GetEntitiesSpan();";
		var spansObtaining = string.Join("\n\t\t\t", data.Components.Select(component =>
		{
			var method = component.IsOptional ? "GetOptionalSpan" : "GetSpan";
			return $"Span<{component.Name}> {component.Name.ToLower()}Span = archetype.{method}<{component.Name}>();";
		}));
		const string entityObtaining = "ref Entity entity = ref entities[i];";
		var componentsObtaining = string.Join("\n\t\t\t\t", data.Components.Select(component =>
		{
			if (component.IsOptional)
				return
					$$"""
					{{component.Name}}? {{component.Name.ToLower()}}DefaultValue = null;
					ref {{component.Name}}? {{component.Name.ToLower()}} = ref {{component.Name.ToLower()}}DefaultValue;
					if ({{component.Name.ToLower()}}Span.Length != 0)
					{
						ref {{component.Name}} {{component.Name.ToLower()}}Temp = ref {{component.Name.ToLower()}}Span[i];
						{{component.Name.ToLower()}} = {{component.Name.ToLower()}}Temp;
					}
					""";
			return $"ref {component.Name} {component.Name.ToLower()} = ref {component.Name.ToLower()}Span[i];";
		}));
		var updateMethodArgumentsRaw = data.Components.Select(component => $"ref {component.Name.ToLower()}");
		if (data.PassEntity)
			updateMethodArgumentsRaw = updateMethodArgumentsRaw.Prepend("ref entity");
		var updateMethodArguments = string.Join(", ", updateMethodArgumentsRaw);
		var source =
			$$"""
			using System;
			using ECS;
			using ECS.Systems.Queries;
			{{componentNamespaces}}
			
			{{(data.Namespace.IsGlobalNamespace ? string.Empty : $"namespace {data.Namespace.ToDisplayString()};")}}

			partial class {{data.Name}}
			{
				public {{data.Name}}(World world)
				{
					_query = new PredicateArchetypeQuery(world, archetype => {{archetypePredicateBody}});
				}
				
				public void Update()
				{
					{{(data.HasPreUpdate ? "PreUpdate();" : string.Empty)}}
					for (var archetypeIndex = 0; archetypeIndex < _query.Archetypes.Count; archetypeIndex++)
					{
						var archetype = _query.Archetypes[archetypeIndex];
						{{(data.PassEntity ? entitiesSpanObtaining : string.Empty)}}
						{{spansObtaining}}
						for (int i = 0; i < archetype.EntitiesCount; i++)
						{
							{{(data.PassEntity ? entityObtaining : string.Empty)}}
							{{componentsObtaining}}
							Update({{updateMethodArguments}});
						}
					}
					{{(data.HasPostUpdate ? "PostUpdate();" : string.Empty)}}
				}
				
				private ArchetypeQuery _query;
			}
			""";
		context.AddSource($"{data.Name}.g.cs", SourceText.From(source, Encoding.UTF8));
	}
}