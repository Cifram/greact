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
		public static Element New(string key, ControlProps props) => Element<ControlProps>.New(key, props, CreateNode, ModifyNode);

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

		private static Godot.Node CreateNode(ControlProps props) {
			var control = new Godot.Control();
			CopyToNode(control, props);
			return control;
		}

		private static void ModifyNode(Godot.Node node, ControlProps oldProps, ControlProps props) {
			if (!oldProps.Equals(props) && node is Godot.Control control) {
				CopyToNode(control, props);
			}
		}
	}
}