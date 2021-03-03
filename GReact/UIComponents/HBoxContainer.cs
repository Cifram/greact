namespace GReact {
	public struct HBoxContainerProps : IBoxContainerProps {
		public int? id { get; set; }
		public UIDim vert { get; set; }
		public UIDim horiz { get; set; }
		public Signal? onReady { get; set; }
		public Godot.BoxContainer.AlignMode alignment { get; set; }
	}

	public static class HBoxContainerComponent {
		public static Element New(HBoxContainerProps props) =>
			Element<HBoxContainerProps, Godot.HBoxContainer>.New(props, CreateNode, ModifyNode);

		public static void CopyToNode(Godot.HBoxContainer control, HBoxContainerProps? oldProps, HBoxContainerProps props) {
			BoxContainerComponent.CopyToNode(control, oldProps, props);
		}

		private static Godot.Node CreateNode(HBoxContainerProps props) {
			var control = new Godot.HBoxContainer();
			CopyToNode(control, null, props);
			return control;
		}

		private static void ModifyNode(Godot.Node node, HBoxContainerProps oldProps, HBoxContainerProps props) {
			if (!oldProps.Equals(props) && node is Godot.HBoxContainer control) {
				CopyToNode(control, oldProps, props);
			}
		}
	}
}