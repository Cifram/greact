using Godot;

namespace GReact {
	public struct VBoxContainerProps : IBoxContainerProps {
		public int? id { get; set; }
		public UIDim vert { get; set; }
		public UIDim horiz { get; set; }
		public Godot.Vector2 minSize { get; set; }
		public Godot.Control.SizeFlags sizeFlagsHoriz { get; set; }
		public Godot.Control.SizeFlags sizeFlagsVert { get; set; }
		public Signal? onReady { get; set; }
		public BoxContainer.AlignMode alignment { get; set; }
	}

	public static class VBoxContainerComponent {
		public static Element New(VBoxContainerProps props) =>
			Element<VBoxContainerProps, Godot.VBoxContainer>.New(props, CreateNode, ModifyNode);

		public static void CopyToNode(Godot.VBoxContainer control, VBoxContainerProps? oldProps, VBoxContainerProps props) {
			BoxContainerComponent.CopyToNode(control, oldProps, props);
		}

		private static Godot.Node CreateNode(VBoxContainerProps props) {
			var control = new Godot.VBoxContainer();
			CopyToNode(control, null, props);
			return control;
		}

		private static void ModifyNode(Godot.Node node, VBoxContainerProps oldProps, VBoxContainerProps props) {
			if (!oldProps.Equals(props) && node is Godot.VBoxContainer control) {
				CopyToNode(control, oldProps, props);
			}
		}
	}
}