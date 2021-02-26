using Godot;

namespace GReact {
	public struct LabelProps : IControlProps {
		public int? id { get; set; }
		public UIDim vert { get; set; }
		public UIDim horiz { get; set; }
		public Vector2 minSize { get; set; }
		public Control.SizeFlags sizeFlagsHoriz { get; set; }
		public Control.SizeFlags sizeFlagsVert { get; set; }
		public Signal? onReady { get; set; }
		public string text;
	}

	public static class LabelComponent {
		public static Element New(LabelProps props) => Element<LabelProps, Godot.Label>.New(props, CreateNode, ModifyNode);

		public static void CopyToNode(Godot.Label control, LabelProps? oldProps, LabelProps props) {
			ControlComponent.CopyToNode(control, oldProps, props);
			control.Text = props.text;
		}

		public static Godot.Node CreateNode(LabelProps props) {
			var control = new Godot.Label();
			CopyToNode(control, null, props);
			return control;
		}

		public static void ModifyNode(Godot.Node node, LabelProps oldProps, LabelProps props) {
			if (!oldProps.Equals(props) && node is Godot.Label control) {
				CopyToNode(control, oldProps, props);
			}
		}
	}
}