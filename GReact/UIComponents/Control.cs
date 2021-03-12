namespace GReact {
	public interface IControlProps : INodeProps {
		UIDim vert { get; set; }
		UIDim horiz { get; set; }
		Signal? onReady { get; set; }
	}

	[Component]
	public static class ControlComponent {
		public struct Props : IControlProps {
			[Optional] public int? id { get; set; }
			[Optional] public UIDim vert { get; set; }
			[Optional] public UIDim horiz { get; set; }
			[Optional] public Signal? onReady { get; set; }
		}

		public static Element New(Props props) => Element<Props, Godot.Control>.New(props, CreateNode, ModifyNode);

		public static void CopyToNode(Godot.Control control, IControlProps? oldProps, IControlProps props) {
			if (oldProps == null || !oldProps.vert.Equals(props.vert)) {
				if (props.vert.containerMode) {
					control.RectMinSize = new(control.RectMinSize.x, props.vert.minSize);
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
					control.RectMinSize = new(props.horiz.minSize, control.RectMinSize.y);
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

		private static Godot.Node CreateNode(Props props) {
			var control = new Godot.Control();
			CopyToNode(control, null, props);
			return control;
		}

		private static void ModifyNode(Godot.Node node, Props oldProps, Props props) {
			if (!oldProps.Equals(props) && node is Godot.Control control) {
				CopyToNode(control, oldProps, props);
			}
		}
	}
}