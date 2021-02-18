namespace GReact {
	public struct ButtonProps : IBaseButtonProps {
		public bool disabled { get; set; }
		public Signal pressed { get; set; }
		public UIDim vert { get; set; }
		public UIDim horiz { get; set; }
		public string text;
	}

	public static class ButtonComponent {
		public static Element New(string key, ButtonProps props) =>
			Element<ButtonProps>.New(key, props,
				(props) => {
					var control = new Godot.Button();
					CopyToNode(control, null, props);
					return control;
				},
				(node, oldProps, props) => {
					if (!oldProps.Equals(props) && node is Godot.Button control) {
						CopyToNode(control, oldProps, props);
					}
				}
			);
		
		public static void CopyToNode(Godot.Button control, ButtonProps? oldProps, ButtonProps props) {
			BaseButtonComponent.CopyToNode(control, oldProps, props);
			control.Text = props.text;
		}
	}
}