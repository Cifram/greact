using System;
using GReact;

namespace UIExample {
	[Component]
	public static class ListItemComponent {
		public struct Props {
			public string text;
			public Action<Godot.Node> onDelete;
			public Action<Godot.Node, string> onChange;
		}

		public static Element New(Props props) =>
			Component.HBoxContainer(
				horiz: UIDim.Container.ExpandFill()
			).Children(
				Component.LineEdit(
					horiz: UIDim.Container.ExpandFill(),
					text: props.text,
					onTextChanged: props.onChange,
					onReady: node => {
						if (node is Godot.LineEdit control) {
							control.GrabFocus();
							control.SelectAll();
						}
					}
				),
				Component.Button(
					text: "X",
					onPressed: props.onDelete
				)
			);
	}
}