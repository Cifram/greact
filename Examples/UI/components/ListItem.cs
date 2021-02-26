using Godot;
using GReact;

namespace UIExample {
	public struct ListItemProps {
		public string text;
		public Signal onDelete;
		public Signal<string> onChange;
	}

	public static class ListItemComponent {
		public static Element New(ListItemProps props) =>
			HBoxContainerComponent.New(new HBoxContainerProps {
				sizeFlagsHoriz = Control.SizeFlags.ExpandFill,
			}).Child(
				LineEditComponent.New(new LineEditProps {
					sizeFlagsHoriz = Control.SizeFlags.ExpandFill,
					text = props.text,
					onTextChanged = props.onChange,
					onReady = Signal.New(OnLineEditReady),
				})
			).Child(
				ButtonComponent.New(new ButtonProps {
					text = "X",
					onPressed = props.onDelete,
				})
			);

		public static void OnLineEditReady(Node node) {
			if (node is LineEdit control) {
				control.GrabFocus();
				control.SelectAll();
			}
		}
	}
}