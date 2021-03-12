namespace GReact {
	[Component]
	public static class HBoxContainerComponent {
		public struct Props : IBoxContainerProps {
			[Optional] public int? id { get; set; }
			[Optional] public UIDim vert { get; set; }
			[Optional] public UIDim horiz { get; set; }
			[Optional] public Signal? onReady { get; set; }
			[Optional] public Godot.BoxContainer.AlignMode alignment { get; set; }
		}

		public static Element New(Props props) =>
			Element<Props, Godot.HBoxContainer>.New(props, CreateNode, ModifyNode);

		public static void CopyToNode(Godot.HBoxContainer control, Props? oldProps, Props props) {
			BoxContainerComponent.CopyToNode(control, oldProps, props);
		}

		private static Godot.Node CreateNode(Props props) {
			var control = new Godot.HBoxContainer();
			CopyToNode(control, null, props);
			return control;
		}

		private static void ModifyNode(Godot.Node node, Props oldProps, Props props) {
			if (!oldProps.Equals(props) && node is Godot.HBoxContainer control) {
				CopyToNode(control, oldProps, props);
			}
		}
	}
}