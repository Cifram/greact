using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

[Generator]
public class GReactGenerator : ISourceGenerator {
	public void Execute(GeneratorExecutionContext context) {
		var types = new Types(context);

		var src = @"
using System;

namespace GReact {
	public static partial class Component {";

		var components = FindComponents(context.Compilation.GlobalNamespace, types);
		foreach (var component in components) {
			if (!component.IsStatic) {
				Diagnostics.Report(context, Diagnostics.ComponentNotStatic, component.DeclaringSyntaxReferences[0], false, component.Name);
				continue;
			}
			var propsCandidates = component.GetTypeMembers("Props");
			if (propsCandidates.Length == 0) {
				Diagnostics.Report(context, Diagnostics.ComponentLacksPropsStruct, component.DeclaringSyntaxReferences[0], false, component.Name);
				continue;
			}
			var props = propsCandidates.FirstOrDefault(candidate => candidate.Arity == 0);
			if (props == null) {
				Diagnostics.Report(context, Diagnostics.ComponentPropsStructIsGeneric, component.DeclaringSyntaxReferences[0], false, component.Name);
				continue;
			}
			if (props.TypeKind != TypeKind.Struct) {
				Diagnostics.Report(context, Diagnostics.ComponentPropsNotAStruct, component.DeclaringSyntaxReferences[0], false, component.Name, component.TypeKind);
				continue;
			}

			var fullComponentName = component.ToString();
			var componentBaseName = component.Name.EndsWith("Component") ? component.Name.Substring(0, component.Name.Length - 9) : component.Name;
			var propList = GetProps(context, fullComponentName, props, types);

			var signalList = propList.Where(prop => prop.signalName != null);
			if (signalList.Any()) {
				src += $@"

		private class {componentBaseName}Signals : Godot.Object {{
			public Godot.Node node;
			public {componentBaseName}Signals(Godot.Node node) => this.node = node;
{String.Join("", signalList.Select(prop => prop.signalType == null ?
$@"
			public Action<Godot.Node>? {prop.name};
			public void {Capitalize(prop.name)}() => {prop.name}?.Invoke(node);" :
$@"
			public Action<Godot.Node, {prop.signalType.ToString()}>? {prop.name};
			public void {Capitalize(prop.name)}({prop.signalType.ToString()} val) => {prop.name}?.Invoke(node, val);"
))}
		}}

		public static void Register{componentBaseName}Signals(
			Godot.Node node,
			{fullComponentName}.Props props
		) {{
			var id = node.GetInstanceId();
			if (!Renderer.signalObjects.ContainsKey(id)) {{
				Renderer.signalObjects[id] = new {componentBaseName}Signals(node);
			}}
			if (Renderer.signalObjects[id] is {componentBaseName}Signals signalObject) {{
{String.Join("", signalList.Select(prop => $@"
				if (signalObject.{prop.name} == null && props.{prop.name} != null) {{
					node.Connect(""{prop.signalName ?? ""}"", signalObject, ""{Capitalize(prop.name)}"");
				}} else if (signalObject.{prop.name} != null && props.{prop.name} == null) {{
					node.Disconnect(""{prop.signalName ?? ""}"", signalObject, ""{Capitalize(prop.name)}"");
				}}
				signalObject.{prop.name} = props.{prop.name};"))}
			}}
		}}";
			}

			src += $@"

		public static GReact.Element {componentBaseName}(
{String.Join(",\n", propList.Select(prop => $"\t\t\t{prop.type} {prop.name}{(prop.optional ? $" = default({prop.type})" : "")}"))}
		) {{
			var elem = {fullComponentName}.New(new {fullComponentName}.Props {{
{String.Join("\n", propList.Select(prop => $"\t\t\t\t{prop.name} = {prop.name},"))}
			}});
			return elem;
		}}";
		}

		src += @"
	}
}";

		context.AddSource($"GeneratedComponents", src);
	}

	public void Initialize(GeneratorInitializationContext context) { }

	private struct PropInfo {
		public string type;
		public string name;
		public string? signalName;
		public string? signalType;
		public bool optional;

		public PropInfo(
			GeneratorExecutionContext context, string componentName, Types types,
			ITypeSymbol type, string name, string? signalName, bool optional
		) {
			(this.type, this.name, this.signalName, this.optional) = (type.ToString(), name, signalName, optional);
			if (signalName == null) {
				signalType = null;
			} else {
				if (type is INamedTypeSymbol namedType) {
					if (namedType.Name != "Action") {
						Diagnostics.Report(context, Diagnostics.SignalPropIsNotAction, null, true, name, componentName, type.Name);
						throw new Exception();
					}
					var arity = namedType.TypeArguments.Length;
					if (arity == 1) {
						signalType = null;
					} else if (arity == 2) {
						signalType = namedType.TypeArguments[1].ToString();
					} else {
						Diagnostics.Report(context, Diagnostics.SignalPropMustHaveArityOneOrTwo, null, true, name, componentName, type.ToString(), arity);
						throw new Exception();
					}
					if (!namedType.TypeArguments[0].Equals(types.node, SymbolEqualityComparer.Default)) {
						Diagnostics.Report(context, Diagnostics.SignalPropMustHaveNodeArg, null, true, name, componentName, type.ToString(), namedType.TypeArguments[0].ToString());
					}
				} else {
					Diagnostics.Report(context, Diagnostics.SignalHasAnonymousType, null, true, name, componentName, type.ToString());
					throw new Exception();
				}
			}
		}
	}

	private struct Types {
		public INamedTypeSymbol componentAttr;
		public INamedTypeSymbol optionalAttr;
		public INamedTypeSymbol signalAttr;
		public INamedTypeSymbol node;

		public Types(GeneratorExecutionContext context) {
			componentAttr = GetType(context, "GReact.ComponentAttribute");
			signalAttr = GetType(context, "GReact.SignalAttribute");
			optionalAttr = GetType(context, "GReact.OptionalAttribute");
			node = GetType(context, "Godot.Node");
		}

		private static INamedTypeSymbol GetType(GeneratorExecutionContext context, string name) {
			var maybeType = context.Compilation.GetTypeByMetadataName(name);
			if (maybeType == null) {
				throw new Exception("{name} type not found");
			}
			return maybeType;
		}
	}

	private string Capitalize(string original) {
		return char.ToUpper(original[0]) + original.Substring(1);
	}

	private AttributeData? GetAttribute(ISymbol symbol, INamedTypeSymbol attribute) {
		var attrs = symbol.GetAttributes().Where(
			ad => ad.AttributeClass?.Equals(attribute, SymbolEqualityComparer.Default) ?? false
		).ToArray();
		if (attrs.Length == 0) {
			return null;
		}
		return attrs[0];
	}

	private bool HasAttribute(ISymbol symbol, INamedTypeSymbol attribute) =>
		GetAttribute(symbol, attribute) != null;

	private List<PropInfo> GetProps(
		GeneratorExecutionContext context, string componentName,
		INamedTypeSymbol propsStruct, Types types
	) {
		var props = new List<PropInfo>();
		foreach (var member in propsStruct.GetMembers()) {
			var signalAttrData = GetAttribute(member, types.signalAttr);
			var signalName = signalAttrData?.ConstructorArguments[0].Value as string;
			if (member is IFieldSymbol field && !field.IsImplicitlyDeclared) {
				props.Add(new PropInfo(
					context, componentName, types,
					field.Type, field.Name, signalName, HasAttribute(member, types.optionalAttr)
				));
			} else if (member is IPropertySymbol property) {
				props.Add(new PropInfo(
					context, componentName, types,
					property.Type, property.Name, signalName, HasAttribute(member, types.optionalAttr)
				));
			}
		}
		return props;
	}

	private List<INamedTypeSymbol> FindComponents(INamespaceSymbol ns, Types types) {
		var list = new List<INamedTypeSymbol>();

		foreach (var type in ns.GetTypeMembers()) {
			if (type.TypeKind != TypeKind.Class || type.Name == "") {
				continue;
			}
			if (!HasAttribute(type, types.componentAttr)) {
				continue;
			}
			list.Add(type);
		}

		foreach (var subNs in ns.GetNamespaceMembers()) {
			list.AddRange(FindComponents(subNs, types));
		}

		return list;
	}
}