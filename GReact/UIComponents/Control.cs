namespace GReact {
	public interface IControlProps : INodeProps {
		UIDim vert { get; set; }
		UIDim horiz { get; set; }
		Godot.Vector2 minSize { get; set; }
		Godot.Control.SizeFlags sizeFlagsHoriz { get; set; }
		Godot.Control.SizeFlags sizeFlagsVert { get; set; }
		Signal? onReady { get; set; }
	}

	public struct ControlProps : IControlProps {
		public int? id { get; set; }
		public UIDim vert { get; set; }
		public UIDim horiz { get; set; }
		public Godot.Vector2 minSize { get; set; }
		public Godot.Control.SizeFlags sizeFlagsHoriz { get; set; }
		public Godot.Control.SizeFlags sizeFlagsVert { get; set; }
		public Signal? onReady { get; set; }
	}

	public static class ControlComponent {
		public static Element New(ControlProps props) => Element<ControlProps, Godot.Control>.New(props, CreateNode, ModifyNode);

		public static void CopyToNode(Godot.Control control, IControlProps? oldProps, IControlProps props) {
			if (oldProps != null && !oldProps.vert.Equals(props.vert)) {
				control.AnchorTop = props.vert.anchorStart;
				control.AnchorBottom = props.vert.anchorEnd;
				control.MarginTop = props.vert.marginStart;
				control.MarginBottom = props.vert.marginEnd;
			}
			if (oldProps != null && !oldProps.horiz.Equals(props.horiz)) {
				control.AnchorLeft = props.horiz.anchorStart;
				control.AnchorRight = props.horiz.anchorEnd;
				control.MarginLeft = props.horiz.marginStart;
				control.MarginRight = props.horiz.marginEnd;
			}
			control.RectMinSize = props.minSize;
			control.SizeFlagsHorizontal = (int)props.sizeFlagsHoriz;
			control.SizeFlagsVertical = (int)props.sizeFlagsVert;
			props.onReady?.Connect(control, "ready", oldProps?.onReady);
		}

		private static Godot.Node CreateNode(ControlProps props) {
			var control = new Godot.Control();
			CopyToNode(control, null, props);
			return control;
		}

		private static void ModifyNode(Godot.Node node, ControlProps oldProps, ControlProps props) {
			if (!oldProps.Equals(props) && node is Godot.Control control) {
				CopyToNode(control, oldProps, props);
			}
		}
	}
}