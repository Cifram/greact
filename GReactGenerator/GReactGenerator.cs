using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

[Generator]
public class GReactGenerator : ISourceGenerator {
	public void Execute(GeneratorExecutionContext context) {
		var componentAttr = context.Compilation.GetTypeByMetadataName("GReact.ComponentAttribute");
		if (componentAttr == null) {
			throw new Exception("GReact.ComponentAttribute type not found");
		}

		var optionalAttr = context.Compilation.GetTypeByMetadataName("GReact.OptionalAttribute");
		if (optionalAttr == null) {
			throw new Exception("GReact.OptionalAttribute type not found");
		}

		var src = @"
using System.Collections.Generic;
using System.Linq;

namespace GReact {
	public static partial class Component {";

		var components = FindComponents(context.Compilation.GlobalNamespace, componentAttr);
		foreach (var component in components) {
			if (!component.IsStatic) {
				Diagnostics.Report(context, Diagnostics.ComponentNotStatic, component.DeclaringSyntaxReferences[0], component.Name);
				continue;
			}
			var propsCandidates = component.GetTypeMembers("Props");
			if (propsCandidates.Length == 0) {
				Diagnostics.Report(context, Diagnostics.ComponentLacksPropsStruct, component.DeclaringSyntaxReferences[0], component.Name);
				continue;
			}
			var props = propsCandidates.FirstOrDefault(candidate => candidate.Arity == 0);
			if (props == null) {
				Diagnostics.Report(context, Diagnostics.ComponentPropsStructIsGeneric, component.DeclaringSyntaxReferences[0], component.Name);
				continue;
			}
			if (props.TypeKind != TypeKind.Struct) {
				Diagnostics.Report(context, Diagnostics.ComponentPropsNotAStruct, component.DeclaringSyntaxReferences[0], component.Name, component.TypeKind);
				continue;
			}

			var propList = GetProps(props, optionalAttr);
			var fullComponentName = component.ToString();
			var constructorName = component.Name.EndsWith("Component") ? component.Name.Substring(0, component.Name.Length - 9) : component.Name;

			src += $@"
		public static GReact.Element {constructorName}(
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
		public bool optional;

		public PropInfo(string type, string name, bool optional) =>
			(this.type, this.name, this.optional) = (type, name, optional);
	}

	private bool HasAttribute(ISymbol symbol, INamedTypeSymbol attribute) =>
		symbol.GetAttributes().Any(
			ad => ad.AttributeClass?.Equals(attribute, SymbolEqualityComparer.Default) ?? false
		);

	private List<PropInfo> GetProps(INamedTypeSymbol propsStruct, INamedTypeSymbol optionalAttr) {
		var props = new List<PropInfo>();
		foreach (var member in propsStruct.GetMembers()) {
			if (member is IFieldSymbol field && !field.IsImplicitlyDeclared) {
				props.Add(new PropInfo(field.Type.ToString(), field.Name, HasAttribute(field, optionalAttr)));
			} else if (member is IPropertySymbol property) {
				props.Add(new PropInfo(property.Type.ToString(), property.Name, HasAttribute(property, optionalAttr)));
			}
		}
		return props;
	}

	private List<INamedTypeSymbol> FindComponents(INamespaceSymbol ns, INamedTypeSymbol componentAttr) {
		var list = new List<INamedTypeSymbol>();

		foreach (var type in ns.GetTypeMembers()) {
			if (type.TypeKind != TypeKind.Class || type.Name == "") {
				continue;
			}
			if (!HasAttribute(type, componentAttr)) {
				continue;
			}
			list.Add(type);
		}

		foreach (var subNs in ns.GetNamespaceMembers()) {
			list.AddRange(FindComponents(subNs, componentAttr));
		}

		return list;
	}
}