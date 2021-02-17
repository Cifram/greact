using Godot;

namespace GReact {
	public struct VBoxContainerProps : IBoxContainerProps {
		public UIDim vert { get; set; }
		public UIDim horiz { get; set; }
		public BoxContainer.AlignMode alignment { get; set; }
	}

	public static class VBoxContainerComponent {
		public static IElement New(string key, VBoxContainerProps props) =>
			Element<VBoxContainerProps>.New(key, props,
				(props) => {
					var control = new Godot.VBoxContainer();
					CopyToNode(control, props);
					return control;
				},
				(node, oldProps, props) => {
					if (!oldProps.Equals(props) && node is Godot.VBoxContainer control) {
						CopyToNode(control, props);
					}
				}
			);

		public static void CopyToNode(Godot.VBoxContainer control, VBoxContainerProps props) {
			BoxContainerComponent.CopyToNode(control, props);
		}
	}
}