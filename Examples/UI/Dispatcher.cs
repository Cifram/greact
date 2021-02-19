namespace UIExample {
	public class Dispatcher : Godot.Node {
		private GReact.Renderer renderer = new GReact.Renderer();

		public override void _Process(float delta) {
			renderer.Render(this, GReact.ButtonComponent.New("button", new GReact.ButtonProps {
				vert = GReact.UIDim.Center(20),
				horiz = GReact.UIDim.Center(100),
				text = "Push Me",
				pressed = GReact.Signal.New(OnButtonPress),
			}));
			base._Process(delta);
		}

		public static void OnButtonPress() {
			Godot.GD.Print("Button Presed!");
		}
	}
}