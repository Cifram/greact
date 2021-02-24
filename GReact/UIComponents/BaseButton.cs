namespace GReact {
	public interface IBaseButtonProps : IControlProps {
		bool disabled { get; set; }
		Signal pressed { get; set; }
	}

	public static class BaseButtonComponent {
		public static void CopyToNode(Godot.BaseButton control, IBaseButtonProps? oldProps, IBaseButtonProps props) {
			ControlComponent.CopyToNode(control, oldProps, props);
			control.Disabled = props.disabled;
			props.pressed.Connect(control, "pressed", oldProps?.pressed);
		}
	}
}