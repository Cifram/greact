using Godot;

namespace GReact {
	public struct LineEditProps : IControlProps {
		public int? id { get; set; }
		public UIDim vert { get; set; }
		public UIDim horiz { get; set; }
		public Vector2 minSize { get; set; }
		public Control.SizeFlags sizeFlagsHoriz { get; set; }
		public Control.SizeFlags sizeFlagsVert { get; set; }
		public Signal? onReady { get; set; }
		public string text;
		public Signal<string>? onTextChanged;
		public Signal<string>? onTextEntered;
	}

	public static class LineEditComponent {
		public static Element New(LineEditProps props) => Element<LineEditProps, Godot.LineEdit>.New(props, CreateNode, ModifyNode);

		public static void CopyToNode(Godot.LineEdit control, LineEditProps? oldProps, LineEditProps props) {
			ControlComponent.CopyToNode(control, oldProps, props);
			if (control.Text != props.text) {
				control.Text = props.text;
			}
			props.onTextChanged?.Connect(control, "text_changed", oldProps?.onTextChanged);
			props.onTextEntered?.Connect(control, "text_entered", oldProps?.onTextEntered);
		}

		private static Godot.Node CreateNode(LineEditProps props) {
			var control = new Godot.LineEdit();
			CopyToNode(control, null, props);
			return control;
		}

		private static void ModifyNode(Godot.Node node, LineEditProps oldProps, LineEditProps props) {
			if (!oldProps.Equals(props) && node is Godot.LineEdit control) {
				CopyToNode(control, oldProps, props);
			}
		}
	}
}