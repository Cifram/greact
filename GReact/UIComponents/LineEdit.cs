namespace GReact {
	[Component]
	public static class LineEditComponent {
		public struct Props : IControlProps {
			public int? id { get; set; }
			public UIDim vert { get; set; }
			public UIDim horiz { get; set; }
			public Signal? onReady { get; set; }
			public string text;
			public Signal<string>? onTextChanged;
			public Signal<string>? onTextEntered;
		}

		public static Element New(Props props) => Element<Props, Godot.LineEdit>.New(props, CreateNode, ModifyNode);

		public static void CopyToNode(Godot.LineEdit control, Props? oldProps, Props props) {
			ControlComponent.CopyToNode(control, oldProps, props);
			if (control.Text != props.text) {
				control.Text = props.text;
			}
			props.onTextChanged?.Connect(control, "text_changed", oldProps?.onTextChanged);
			props.onTextEntered?.Connect(control, "text_entered", oldProps?.onTextEntered);
		}

		private static Godot.Node CreateNode(Props props) {
			var control = new Godot.LineEdit();
			CopyToNode(control, null, props);
			return control;
		}

		private static void ModifyNode(Godot.Node node, Props oldProps, Props props) {
			if (!oldProps.Equals(props) && node is Godot.LineEdit control) {
				CopyToNode(control, oldProps, props);
			}
		}
	}
}