using Godot;
using GReact;

namespace UIExample {
	public struct ListProps {
		public int id;
		public Column column;
		public Signal onDelete;
	}

	public static class ListComponent {
		public static Element New(ListProps props) {
			var mainList = VBoxContainerComponent.New(new VBoxContainerProps {
				sizeFlagsVert = Control.SizeFlags.ExpandFill,
				sizeFlagsHoriz = Control.SizeFlags.ExpandFill,
			});

			foreach (var name in props.column.items) {
				mainList.Child(
					ListItemComponent.New(new ListItemProps {
						text = name,
						onDelete = Signal.New(OnRemoveItem, (props.column, name)),
					})
				);
			}

			return VBoxContainerComponent.New(new VBoxContainerProps {
				id = props.id,
				minSize = new Vector2(200, 0),
			}).Child(
				HBoxContainerComponent.New(new HBoxContainerProps {
					sizeFlagsHoriz = Control.SizeFlags.ExpandFill,
				}).Child(
					ButtonComponent.New(new ButtonProps {
						text = "Add Item",
						onPressed = Signal.New(OnAddItem, props),
						sizeFlagsHoriz = Control.SizeFlags.ExpandFill,
					})
				).Child(
					ButtonComponent.New(new ButtonProps {
						text = "X",
						onPressed = props.onDelete,
					})
				)
			).Child(
				mainList
			);
		}

		private static void OnRemoveItem((Column, string) props) {
			props.Item1.items.Remove(props.Item2);
		}

		private static void OnAddItem(ListProps props) {
			props.column.items.Add("New Item");
		}
	}
}