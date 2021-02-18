using Godot;

namespace GReact {
	public struct HBoxContainerProps : IBoxContainerProps {
		public UIDim vert { get; set; }
		public UIDim horiz { get; set; }
		public BoxContainer.AlignMode alignment { get; set; }
	}

	public static class HBoxContainerComponent {
		public static Element New(string key, HBoxContainerProps props) => Element<HBoxContainerProps>.New(key, props, CreateNode, ModifyNode);

		public static void CopyToNode(Godot.HBoxContainer control, HBoxContainerProps props) {
			BoxContainerComponent.CopyToNode(control, props);
		}

		private static Godot.Node CreateNode(HBoxContainerProps props) {
			var control = new Godot.HBoxContainer();
			CopyToNode(control, props);
			return control;
		}

		private static void ModifyNode(Godot.Node node, HBoxContainerProps oldProps, HBoxContainerProps props) {
			if (!oldProps.Equals(props) && node is Godot.HBoxContainer control) {
				CopyToNode(control, props);
			}
		}
	}
}