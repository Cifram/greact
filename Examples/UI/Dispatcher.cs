namespace UIExample {
	public class Dispatcher : Godot.Node {
		private State state = new();
		private GReact.Renderer renderer = new();

		public override void _Ready() {
			renderer.trackNodeChurn = true;
			base._Ready();
		}

		public override void _Process(float delta) {
			renderer.Render(this, GReact.Component.Root(lists: state.lists, apply: state.Apply));
			if (renderer.nodesCreated != 0 || renderer.nodesDestroyed != 0) {
				Godot.GD.Print($"{renderer.nodesCreated} nodes created and {renderer.nodesDestroyed} destroyed");
			}
			base._Process(delta);
		}
	}
}