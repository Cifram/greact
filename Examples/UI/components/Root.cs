using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GReact;

namespace UIExample {
	[Component]
	public static class RootComponent {
		public struct Props {
			public Dictionary<int, List<string>> lists;
			public Action<Action<State>> apply;
		}

		public static Element New(Props props) =>
			Component.HBoxContainer(
				vert: UIDim.Manual.Expand(0, 0),
				horiz: UIDim.Manual.Expand(0, 0)
			).Children(
				props.lists.Keys.Select(listId =>
					Component.List(
						id: listId,
						list: props.lists[listId],
						apply: props.apply
					)
				).ToArray()
			).Child(
				Component.Button(
					vert: UIDim.Container.ShrinkStart(),
					text: "New List",
					onPressed: Signal.New(OnAddList, props)
				)
			);

		public static void OnAddList(Node node, Props props) {
			props.apply(State.AddList());
		}
	}
}