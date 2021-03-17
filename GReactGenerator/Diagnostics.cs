using System;
using Microsoft.CodeAnalysis;

public static class Diagnostics {
	private static DiagnosticDescriptor? DiagComponentNotStatic;
	public static DiagnosticDescriptor ComponentNotStatic {
		get {
			if (DiagComponentNotStatic == null) {
				DiagComponentNotStatic = new DiagnosticDescriptor(
					id: "GR0001",
					title: "GReact Components must be static",
					messageFormat: "GReact Component class {0} is not static. Components are never instantiated and thus must be static.",
					category: "GReact.Generation",
					defaultSeverity: DiagnosticSeverity.Error,
					isEnabledByDefault: true
				);
			}
			return DiagComponentNotStatic;
		}
	}

	private static DiagnosticDescriptor? DiagComponentLacksPropsStruct;
	public static DiagnosticDescriptor ComponentLacksPropsStruct {
		get {
			if (DiagComponentLacksPropsStruct == null) {
				DiagComponentLacksPropsStruct = new DiagnosticDescriptor(
					id: "GR0002",
					title: "GReact Components must contain a struct called Props",
					messageFormat: "GReact Component class {0} does not contain a struct called Props.",
					category: "GReact.Generation",
					defaultSeverity: DiagnosticSeverity.Error,
					isEnabledByDefault: true
				);
			}
			return DiagComponentLacksPropsStruct;
		}
	}

	private static DiagnosticDescriptor? DiagComponentPropsStructIsGeneric;
	public static DiagnosticDescriptor ComponentPropsStructIsGeneric {
		get {
			if (DiagComponentPropsStructIsGeneric == null) {
				DiagComponentPropsStructIsGeneric = new DiagnosticDescriptor(
					id: "GR0003",
					title: "Props structs for GReact Components must not be generic",
					messageFormat: "Props Struct for GReact Component class {0} is generic.",
					category: "GReact.Generation",
					defaultSeverity: DiagnosticSeverity.Error,
					isEnabledByDefault: true
				);
			}
			return DiagComponentPropsStructIsGeneric;
		}
	}

	private static DiagnosticDescriptor? DiagComponentPropsNotAStruct;
	public static DiagnosticDescriptor ComponentPropsNotAStruct {
		get {
			if (DiagComponentPropsNotAStruct == null) {
				DiagComponentPropsNotAStruct = new DiagnosticDescriptor(
					id: "GR0004",
					title: "Props for GReact Components must not be a struct",
					messageFormat: "Props for GReact Component class {0} is a {1}, not a Struct.",
					category: "GReact.Generation",
					defaultSeverity: DiagnosticSeverity.Error,
					isEnabledByDefault: true
				);
			}
			return DiagComponentPropsNotAStruct;
		}
	}

	public static void Report(GeneratorExecutionContext context, DiagnosticDescriptor desc, SyntaxReference? syntax, params object[] args) {
		System.Console.WriteLine(String.Format(desc.MessageFormat.ToString(), args));
		context.ReportDiagnostic(Diagnostic.Create(desc, syntax == null ? Location.None : Location.Create(syntax.SyntaxTree, syntax.Span), args));
	}
}