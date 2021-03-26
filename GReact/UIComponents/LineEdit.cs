using System;

namespace GReact {
	[Component]
	public static class LineEditComponent {
		public struct Props : IControlProps {
			[Optional] public int? id { get; set; }
			[Optional] public UIDim vert { get; set; }
			[Optional] public UIDim horiz { get; set; }
			[Optional] [Signal("ready")] public Action<Godot.Node>? onReady { get; set; }
			[Optional] public string? text;
			[Optional] [Signal("text_changed")] public Action<Godot.Node, string>? onTextChanged;
			[Optional] [Signal("text_entered")] public Action<Godot.Node, string>? onTextEntered;
		}

		public static Element New(Props props) => Element<Props, Godot.LineEdit>.New(props, CreateNode, ModifyNode);

		public static void CopyToNode(Godot.LineEdit control, Props? oldProps, Props props) {
			ControlComponent.CopyToNode(control, oldProps, props);
			if (control.Text != props.text) {
				control.Text = props.text;
			}
		}

		private static Godot.Node CreateNode(Props props) {
			var control = new Godot.LineEdit();
			CopyToNode(control, null, props);
			Component.RegisterLineEditSignals(control, props);
			return control;
		}

		private static void ModifyNode(Godot.Node node, Props oldProps, Props props) {
			if (!oldProps.Equals(props) && node is Godot.LineEdit control) {
				CopyToNode(control, oldProps, props);
				Component.RegisterLineEditSignals(control, props);
			}
		}
	}
}