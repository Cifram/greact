using Godot;

namespace GReact {
	public struct HBoxContainerProps : IBoxContainerProps {
		public UIDim vert { get; set; }
		public UIDim horiz { get; set; }
		public BoxContainer.AlignMode alignment { get; set; }
	}

	public static class HBoxContainerComponent {
		public static Element New(string key, HBoxContainerProps props) =>
			Element<HBoxContainerProps>.New(key, props,
				(props) => {
					var control = new Godot.HBoxContainer();
					CopyToNode(control, props);
					return control;
				},
				(node, oldProps, props) => {
					if (!oldProps.Equals(props) && node is Godot.HBoxContainer control) {
						CopyToNode(control, props);
					}
				}
			);

		public static void CopyToNode(Godot.HBoxContainer control, HBoxContainerProps props) {
			BoxContainerComponent.CopyToNode(control, props);
		}
	}
}