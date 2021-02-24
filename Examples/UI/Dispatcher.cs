namespace UIExample {
	public class Dispatcher : Godot.Node {
		private State state = new State();
		private GReact.Renderer renderer = new GReact.Renderer();

		public override void _Ready() {
			renderer.trackNodeChurn = true;
			base._Ready();
		}

		public override void _Process(float delta) {
			renderer.Render(this, RootComponent.New(new RootProps { state = state }));
			if (renderer.nodesCreated != 0 || renderer.nodesDestroyed != 0) {
				Godot.GD.Print($"{renderer.nodesCreated} nodes created and {renderer.nodesDestroyed} destroyed");
			}
			base._Process(delta);
		}
	}
}