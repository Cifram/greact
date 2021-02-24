using GReact;

namespace UIExample {
	public struct RootProps {
		public State state;
	}

	public static class RootComponent {
		public static Element New(RootProps props) {
			var root = HBoxContainerComponent.New(new HBoxContainerProps {
				vert = UIDim.Expand(0, 0),
				horiz = UIDim.Expand(0, 0),
			});

			foreach(var columnKey in props.state.columns.Keys) {
				root.Child(
					ListComponent.New(new ListProps {
						id = columnKey,
						column = props.state.columns[columnKey],
						onDelete = Signal.New((props.state, columnKey), OnRemoveColumn),
					})
				);
			}

			root.Child(
				ButtonComponent.New(new ButtonProps {
					text = "New Column",
					pressed = Signal.New(props, OnNewColumn),
				})
			);

			return root;
		}

		public static void OnNewColumn(RootProps props) {
			props.state.columns[props.state.maxId] = new Column();
			props.state.maxId++;
		}

		private static void OnRemoveColumn((State, int) props) {
			props.Item1.columns.Remove(props.Item2);
		}
	}
}