namespace GReact {
	public interface IBoxContainerProps : IControlProps {
		Godot.BoxContainer.AlignMode alignment { get; set; }
	}

	public static class BoxContainerComponent {
		public static void CopyToNode(Godot.BoxContainer control, IBoxContainerProps? oldProps, IBoxContainerProps props) {
			ControlComponent.CopyToNode(control, oldProps, props);
			control.Alignment = props.alignment;
		}
	}
}