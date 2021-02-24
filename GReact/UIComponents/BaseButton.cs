namespace GReact {
	public interface IBaseButtonProps : IControlProps {
		bool disabled { get; set; }
		Signal? onPressed { get; set; }
	}

	public static class BaseButtonComponent {
		public static void CopyToNode(Godot.BaseButton control, IBaseButtonProps? oldProps, IBaseButtonProps props) {
			ControlComponent.CopyToNode(control, oldProps, props);
			control.Disabled = props.disabled;
			props.onPressed?.Connect(control, "pressed", oldProps?.onPressed);
		}
	}
}