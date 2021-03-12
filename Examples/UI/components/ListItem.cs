using Godot;
using GReact;

namespace UIExample {
	[Component]
	public static class ListItemComponent {
		public struct Props {
			public string text;
			public Signal onDelete;
			public Signal<string> onChange;
		}

		public static Element New(Props props) =>
			Component.HBoxContainer(
				horiz: UIDim.Container.ExpandFill()
			).Children(
				Component.LineEdit(
					horiz: UIDim.Container.ExpandFill(),
					text: props.text,
					onTextChanged: props.onChange,
					onReady: Signal.New(OnLineEditReady)
				),
				Component.Button(
					text: "X",
					onPressed: props.onDelete
				)
			);

		public static void OnLineEditReady(Node node) {
			if (node is LineEdit control) {
				control.GrabFocus();
				control.SelectAll();
			}
		}
	}
}