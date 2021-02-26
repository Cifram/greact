namespace GReact {
	public interface IControlProps : INodeProps {
		UIDim vert { get; set; }
		UIDim horiz { get; set; }
		Signal? onReady { get; set; }
	}

	public struct ControlProps : IControlProps {
		public int? id { get; set; }
		public UIDim vert { get; set; }
		public UIDim horiz { get; set; }
		public Signal? onReady { get; set; }
	}

	public static class ControlComponent {
		public static Element New(ControlProps props) => Element<ControlProps, Godot.Control>.New(props, CreateNode, ModifyNode);

		public static void CopyToNode(Godot.Control control, IControlProps? oldProps, IControlProps props) {
			if (oldProps == null || !oldProps.vert.Equals(props.vert)) {
				if (props.vert.containerMode) {
					control.RectMinSize = new Godot.Vector2(control.RectMinSize.x, props.vert.minSize);
					control.SizeFlagsVertical = (int)props.vert.sizeFlags;
				} else {
					control.AnchorTop = props.vert.anchorStart;
					control.AnchorBottom = props.vert.anchorEnd;
					control.MarginTop = props.vert.marginStart;
					control.MarginBottom = props.vert.marginEnd;
				}
			}
			if (oldProps == null || !oldProps.horiz.Equals(props.horiz)) {
				if (props.horiz.containerMode) {
					control.RectMinSize = new Godot.Vector2(props.horiz.minSize, control.RectMinSize.y);
					control.SizeFlagsHorizontal = (int)props.horiz.sizeFlags;
				} else {
					control.AnchorLeft = props.horiz.anchorStart;
					control.AnchorRight = props.horiz.anchorEnd;
					control.MarginLeft = props.horiz.marginStart;
					control.MarginRight = props.horiz.marginEnd;
				}
			}
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