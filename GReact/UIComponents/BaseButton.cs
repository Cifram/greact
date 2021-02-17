namespace GReact {
	public interface IBaseButtonProps : IControlProps {
		bool disabled { get; set; }
		Signal pressed { get; set; }
	}

	public static class BaseButtonComponent {
		public static void CopyToNode(Godot.BaseButton control, IBaseButtonProps? oldProps, IBaseButtonProps props) {
			ControlComponent.CopyToNode(control, props);
			control.Disabled = props.disabled;
			if (oldProps == null || !oldProps.pressed.Equals(props.pressed)) {
				control.Connect("pressed", props.pressed, nameof(props.pressed.Call));
			}
		}
	}
}