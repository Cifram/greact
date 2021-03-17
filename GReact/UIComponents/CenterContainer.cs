namespace GReact {
	[Component]
	public static class CenterContainer {
		public struct Props : IControlProps {
			[Optional] public int? id { get; set; }
			[Optional] public UIDim vert { get; set; }
			[Optional] public UIDim horiz { get; set; }
			[Optional] public Signal? onReady { get; set; }
		}

		public static Element New(Props props) =>
			Element<Props, Godot.CenterContainer>.New(props, CreateNode, ModifyNode);

		public static void CopyToNode(Godot.CenterContainer control, Props? oldProps, Props props) {
			ControlComponent.CopyToNode(control, oldProps, props);
		}

		private static Godot.Node CreateNode(Props props) {
			var control = new Godot.CenterContainer();
			CopyToNode(control, null, props);
			return control;
		}

		private static void ModifyNode(Godot.Node node, Props oldProps, Props props) {
			if (!oldProps.Equals(props) && node is Godot.CenterContainer control) {
				CopyToNode(control, oldProps, props);
			}
		}
	}
}