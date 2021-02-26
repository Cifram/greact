namespace GReact {
	public struct ButtonProps : IBaseButtonProps {
		public int? id { get; set; }
		public UIDim vert { get; set; }
		public UIDim horiz { get; set; }
		public Godot.Vector2 minSize { get; set; }
		public Godot.Control.SizeFlags sizeFlagsHoriz { get; set; }
		public Godot.Control.SizeFlags sizeFlagsVert { get; set; }
		public Signal? onReady { get; set; }
		public bool disabled { get; set; }
		public Signal? onPressed { get; set; }
		public string text;
	}

	public static class ButtonComponent {
		public static Element New(ButtonProps props) => Element<ButtonProps, Godot.Button>.New(props, CreateNode, ModifyNode);

		public static void CopyToNode(Godot.Button control, ButtonProps? oldProps, ButtonProps props) {
			BaseButtonComponent.CopyToNode(control, oldProps, props);
			control.Text = props.text;
		}

		private static Godot.Node CreateNode(ButtonProps props) {
			var control = new Godot.Button();
			CopyToNode(control, null, props);
			return control;
		}

		private static void ModifyNode(Godot.Node node, ButtonProps oldProps, ButtonProps props) {
			if (!oldProps.Equals(props) && node is Godot.Button control) {
				CopyToNode(control, oldProps, props);
			}
		}
	}
}