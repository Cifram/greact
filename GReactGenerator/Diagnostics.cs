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

	private static DiagnosticDescriptor? DiagSignalPropIsNotAction;
	public static DiagnosticDescriptor SignalPropIsNotAction {
		get {
			if (DiagSignalPropIsNotAction == null) {
				DiagSignalPropIsNotAction = new DiagnosticDescriptor(
					id: "GR0004",
					title: "Signal props must be of type Action",
					messageFormat: "Signal prop {0} for GReact Component class {1} is a {2}, not an Action<Godot.Node> or Action<Godot.Node, T>.",
					category: "GReact.Generation",
					defaultSeverity: DiagnosticSeverity.Error,
					isEnabledByDefault: true
				);
			}
			return DiagSignalPropIsNotAction;
		}
	}

	private static DiagnosticDescriptor? DiagSignalPropMustHaveArityZeroOrOne;
	public static DiagnosticDescriptor SignalPropMustHaveArityOneOrTwo {
		get {
			if (DiagSignalPropMustHaveArityZeroOrOne == null) {
				DiagSignalPropMustHaveArityZeroOrOne = new DiagnosticDescriptor(
					id: "GR0004",
					title: "Signal props must be an Action of arity 0 or 1",
					messageFormat: "Signal prop {0} for GReact Component class {1} is a {2} (with {3} type arguments), but must have 1 or 2 type arguments.",
					category: "GReact.Generation",
					defaultSeverity: DiagnosticSeverity.Error,
					isEnabledByDefault: true
				);
			}
			return DiagSignalPropMustHaveArityZeroOrOne;
		}
	}

	private static DiagnosticDescriptor? DiagSignalPropMustHaveNodeArg;
	public static DiagnosticDescriptor SignalPropMustHaveNodeArg {
		get {
			if (DiagSignalPropMustHaveNodeArg == null) {
				DiagSignalPropMustHaveNodeArg = new DiagnosticDescriptor(
					id: "GR0004",
					title: "Signal props must be an Action with first type argument of Godot.Node",
					messageFormat: "Signal prop {0} for GReact Component class {1} is a {2}, but the first type argument must be a Godot.Node, not {3}.",
					category: "GReact.Generation",
					defaultSeverity: DiagnosticSeverity.Error,
					isEnabledByDefault: true
				);
			}
			return DiagSignalPropMustHaveNodeArg;
		}
	}

	private static DiagnosticDescriptor? DiagSignalHasAnonymousType;
	public static DiagnosticDescriptor SignalHasAnonymousType {
		get {
			if (DiagSignalHasAnonymousType == null) {
				DiagSignalHasAnonymousType = new DiagnosticDescriptor(
					id: "GR0004",
					title: "Signal props must be a named type",
					messageFormat: "Signal prop {0} for GReact Component class {1} is of anonymous type {2}, but must be a named type of either Action or Action<T>.",
					category: "GReact.Generation",
					defaultSeverity: DiagnosticSeverity.Error,
					isEnabledByDefault: true
				);
			}
			return DiagSignalHasAnonymousType;
		}
	}

	public static void Report(GeneratorExecutionContext context, DiagnosticDescriptor desc, SyntaxReference? syntax, bool throwException, params object[] args) {
		context.ReportDiagnostic(Diagnostic.Create(desc, syntax == null ? Location.None : Location.Create(syntax.SyntaxTree, syntax.Span), args));
		var message = String.Format(desc.MessageFormat.ToString(), args);
		System.Console.WriteLine(message);
		if (throwException) {
			throw new Exception(message);
		}
	}
}