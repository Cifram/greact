using Godot;

namespace GReact {
	public struct VBoxContainerProps : IBoxContainerProps {
		public UIDim vert { get; set; }
		public UIDim horiz { get; set; }
		public BoxContainer.AlignMode alignment { get; set; }
	}

	public static class VBoxContainerComponent {
		public static Element New(string key, VBoxContainerProps props) => Element<VBoxContainerProps>.New(key, props, CreateNode, ModifyNode);

		public static void CopyToNode(Godot.VBoxContainer control, VBoxContainerProps props) {
			BoxContainerComponent.CopyToNode(control, props);
		}

		private static Godot.Node CreateNode(VBoxContainerProps props) {
			var control = new Godot.VBoxContainer();
			CopyToNode(control, props);
			return control;
		}

		private static void ModifyNode(Godot.Node node, VBoxContainerProps oldProps, VBoxContainerProps props) {
			if (!oldProps.Equals(props) && node is Godot.VBoxContainer control) {
				CopyToNode(control, props);
			}
		}
	}
}