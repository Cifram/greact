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
			).Child(
				Component.HBoxContainer(
					horiz: UIDim.Container.ExpandFill()
				).Children(
					Component.Button(
						horiz: UIDim.Container.ExpandFill(),
						text: "Add Item",
						onPressed: Signal.New(OnAddItem, props)
					),
					Component.Button(text: "X", onPressed: Signal.New(OnRemoveList, props))
				)
			).Children(
				props.list.Select((name, i) => Component.ListItem(
					text: name,
					onDelete: Signal.New(OnRemoveItem, (i, props)),
					onChange: Signal<string>.New(OnChangeItem, (i, props))
				)).ToArray()
			);

		private static void OnRemoveList(Node node, Props props) {
			props.apply(State.RemoveList(props.id));
		}

		private static void OnRemoveItem(Node node, (int itemIndex, Props props) args) {
			args.props.apply(State.RemoveItemFromList(args.props.id, args.itemIndex));
		}

		private static void OnAddItem(Node node, Props props) {
			props.apply(State.AddItemToList(props.id));
		}

		private static void OnChangeItem(Node node, (int itemIndex, Props props) args, string newValue) {
			args.props.apply(State.ChangeItem(args.props.id, args.itemIndex, newValue));
		}
	}
}