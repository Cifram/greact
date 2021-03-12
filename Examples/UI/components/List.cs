using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GReact;

namespace UIExample {
	[Component]
	public static class ListComponent {
		public struct Props {
			public int id;
			public List<string> list;
			public Action<Action<State>> apply;
		}

		public static Element New(Props props) =>
			Component.VBoxContainer(
				id: props.id,
				horiz: UIDim.Container.ShrinkStart(200)
			).Children(
				Component.HBoxContainer(
					horiz: UIDim.Container.ExpandFill()
				).Children(
					Component.Button(
						horiz: UIDim.Container.ExpandFill(),
						text: "Add Item",
						onPressed: Signal.New(OnAddItem, props)
					),
					Component.Button(text: "X", onPressed: Signal.New(OnRemoveList, props))
				),
				Component.VBoxContainer(
					vert: UIDim.Container.ExpandFill(),
					horiz: UIDim.Container.ExpandFill()
				).Children(
					props.list.Select((name, i) => Component.ListItem(
						text: name,
						onDelete: Signal.New(OnRemoveItem, (i, props)),
						onChange: Signal<string>.New(OnChangeItem, (i, props))
					)).ToArray()
				)
			);

		private static void OnRemoveList(Node node, Props props) {
			props.apply(State.RemoveList(props.id));
		}

		private static void OnRemoveItem(Node node, (int, Props) args) {
			var (itemIndex, props) = args;
			props.apply(State.RemoveItemFromList(props.id, itemIndex));
		}

		private static void OnAddItem(Node node, Props props) {
			props.apply(State.AddItemToList(props.id));
		}

		private static void OnChangeItem(Node node, (int, Props) args, string newValue) {
			var (itemIndex, props) = args;
			props.apply(State.ChangeItem(props.id, itemIndex, newValue));
		}
	}
}