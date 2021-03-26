using System;
using System.Collections.Generic;
using System.Linq;
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
						onPressed: node => props.apply(State.AddItemToList(props.id))
					),
					Component.Button(
						text: "X",
						onPressed: node => props.apply(State.RemoveList(props.id))
					)
				)
			).Children(
				props.list.Select((name, i) => Component.ListItem(
					text: name,
					onDelete: node => props.apply(State.RemoveItemFromList(props.id, i)),
					onChange: (node, str) => props.apply(State.ChangeItem(props.id, i, str))
				)).ToArray()
			);
	}
}