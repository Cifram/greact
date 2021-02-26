using System;
using System.Collections.Generic;
using Godot;
using GReact;

namespace UIExample {
	public struct RootProps {
		public Dictionary<int, List<string>> lists;
		public Action<Action<State>> apply;
	}

	public static class RootComponent {
		public static Element New(RootProps props) {
			var root = HBoxContainerComponent.New(new HBoxContainerProps {
				vert = UIDim.Expand(0, 0),
				horiz = UIDim.Expand(0, 0),
			});

			foreach (var listId in props.lists.Keys) {
				root.Child(
					ListComponent.New(new ListProps {
						id = listId,
						list = props.lists[listId],
						apply = props.apply,
					})
				);
			}

			root.Child(
				ButtonComponent.New(new ButtonProps {
					text = "New Column",
					onPressed = Signal.New(OnAddList, props),
				})
			);

			return root;
		}

		public static void OnAddList(Node node, RootProps props) {
			props.apply(State.AddList());
		}
	}
}