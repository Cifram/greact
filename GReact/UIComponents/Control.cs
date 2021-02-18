namespace GReact {
	public interface IControlProps {
		UIDim vert { get; set; }
		UIDim horiz { get; set; }
	}

	public struct ControlProps : IControlProps {
		public UIDim vert { get; set; }
		public UIDim horiz { get; set; }
	}

	public static class ControlComponent {
		public static Element New(string key, ControlProps props) =>
			Element<ControlProps>.New(key, props,
				(props) => {
					var control = new Godot.Control();
					CopyToNode(control, props);
					return control;
				},
				(node, oldProps, props) => {
					if (!oldProps.Equals(props) && node is Godot.Control control) {
						CopyToNode(control, props);
					}
				}
			);

		public static void CopyToNode(Godot.Control control, IControlProps props) {
			control.AnchorTop = props.vert.anchorStart;
			control.AnchorBottom = props.vert.anchorEnd;
			control.AnchorLeft = props.horiz.anchorStart;
			control.AnchorRight = props.horiz.anchorEnd;
			control.MarginTop = props.vert.marginStart;
			control.MarginBottom = props.vert.marginEnd;
			control.MarginLeft = props.horiz.marginStart;
			control.MarginRight = props.horiz.marginEnd;
		}
	}
}