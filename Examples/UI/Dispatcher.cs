namespace UIExample {
	public class Dispatcher : Godot.Node {
		private State state = new State();
		private GReact.Renderer renderer = new GReact.Renderer();

		public override void _Ready() {
			base._Ready();
		}

		public override void _Process(float delta) {
			renderer.Render(this, RootComponent.New(new RootProps { state = state }));
			base._Process(delta);
		}
	}
}