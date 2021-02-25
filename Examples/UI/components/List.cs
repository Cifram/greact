using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GReact;

namespace UIExample {
	public struct ListProps {
		public int id;
		public List<string> list;
		public Action<Action<State>> apply;
	}

	public static class ListComponent {
		public static Element New(ListProps props) {
			var mainList = VBoxContainerComponent.New(new VBoxContainerProps {
				sizeFlagsVert = Control.SizeFlags.ExpandFill,
				sizeFlagsHoriz = Control.SizeFlags.ExpandFill,
			});

			foreach (var (name, i) in props.list.Select((name, i) => (name, i))) {
				mainList.Child(
					ListItemComponent.New(new ListItemProps {
						text = name,
						onDelete = Signal.New(OnRemoveItem, (i, props)),
						onChange = Signal<string>.New(OnChangeItem, (i, props)),
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
						onPressed = Signal.New(OnRemoveList, props),
					})
				)
			).Child(
				mainList
			);
		}

		private static void OnRemoveList(ListProps props) {
			props.apply(State.RemoveList(props.id));
		}

		private static void OnRemoveItem((int, ListProps) args) {
			var (itemIndex, props) = args;
			props.apply(State.RemoveItemFromList(props.id, itemIndex));
		}

		private static void OnAddItem(ListProps props) {
			props.apply(State.AddItemToList(props.id));
		}

		private static void OnChangeItem((int, ListProps) args, string newValue) {
			var (itemIndex, props) = args;
			props.apply(State.ChangeItem(props.id, itemIndex, newValue));
		}
	}
}