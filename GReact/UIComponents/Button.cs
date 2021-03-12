namespace GReact {

	[Component]
	public static class ButtonComponent {
		public struct Props : IBaseButtonProps {
			[Optional] public int? id { get; set; }
			[Optional] public UIDim vert { get; set; }
			[Optional] public UIDim horiz { get; set; }
			[Optional] public Signal? onReady { get; set; }
			[Optional] public bool disabled { get; set; }
			[Optional] public Signal? onPressed { get; set; }
			[Optional] public string text;
		}

		public static Element New(Props props) => Element<Props, Godot.Button>.New(props, CreateNode, ModifyNode);

		public static void CopyToNode(Godot.Button control, Props? oldProps, Props props) {
			BaseButtonComponent.CopyToNode(control, oldProps, props);
			control.Text = props.text;
		}

		private static Godot.Node CreateNode(Props props) {
			var control = new Godot.Button();
			CopyToNode(control, null, props);
			return control;
		}

		private static void ModifyNode(Godot.Node node, Props oldProps, Props props) {
			if (!oldProps.Equals(props) && node is Godot.Button control) {
				CopyToNode(control, oldProps, props);
			}
		}
	}
}