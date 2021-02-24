using System;
using Godot;
using GReact;

namespace UIExample {
	public struct ListItemProps {
		public string text;
		public Signal onDelete;
	}

	public static class ListItemComponent {
		public static Element New(ListItemProps props) =>
			HBoxContainerComponent.New(new HBoxContainerProps {
				sizeFlagsHoriz = Control.SizeFlags.ExpandFill,
			}).Child(
				LabelComponent.New(new LabelProps {
					sizeFlagsHoriz = Control.SizeFlags.ExpandFill,
					text = props.text,
				})
			).Child(
				ButtonComponent.New(new ButtonProps {
					text = "X",
					pressed = props.onDelete,
				})
			);
	}
}