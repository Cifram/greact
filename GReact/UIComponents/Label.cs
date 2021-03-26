using System;

namespace GReact {
	[Component]
	public static class LabelComponent {
		public struct Props : IControlProps {
			[Optional] public int? id { get; set; }
			[Optional] public UIDim vert { get; set; }
			[Optional] public UIDim horiz { get; set; }
			[Optional] [Signal("ready")] public Action<Godot.Node>? onReady { get; set; }
			[Optional] public string? text;
		}

		public static Element New(Props props) => Element<Props, Godot.Label>.New(props, CreateNode, ModifyNode);

		public static void CopyToNode(Godot.Label control, Props? oldProps, Props props) {
			ControlComponent.CopyToNode(control, oldProps, props);
			control.Text = props.text;
		}

		public static Godot.Node CreateNode(Props props) {
			var control = new Godot.Label();
			CopyToNode(control, null, props);
			Component.RegisterLabelSignals(control, props);
			return control;
		}

		public static void ModifyNode(Godot.Node node, Props oldProps, Props props) {
			if (!oldProps.Equals(props) && node is Godot.Label control) {
				CopyToNode(control, oldProps, props);
				Component.RegisterLabelSignals(control, props);
			}
		}
	}
}