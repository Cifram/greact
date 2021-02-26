namespace GReact {
	public struct UIDim {
		public bool containerMode;

		// Values used for manual layout
		public float anchorStart;
		public float anchorEnd;
		public float marginStart;
		public float marginEnd;

		// Values used for container layout
		public float minSize;
		public Godot.Control.SizeFlags sizeFlags;

		/// <summary>Layout directly with anchors and margins, for when the control is not in a container.</summary>
		public static class Manual {
			/// <summary>Place the control at the top or left of the space, with the specified size.</summary>
			public static UIDim Start(float size) =>
				new UIDim {
					containerMode = false,
					anchorStart = 0,
					anchorEnd = 0,
					marginStart = 0,
					marginEnd = size,
				};

			/// <summary>Place the control at the bottom or right of the space, with the specified size.</summary>
			public static UIDim End(float size) =>
				new UIDim {
					containerMode = false,
					anchorStart = 1,
					anchorEnd = 1,
					marginStart = -size,
					marginEnd = 0,
				};

			/// <summary>Center the control, with the specified size.</summary>
			public static UIDim Center(float size) =>
				new UIDim {
					containerMode = false,
					anchorStart = 0.5f,
					anchorEnd = 0.5f,
					marginStart = -size / 2,
					marginEnd = size / 2,
				};

			/// <summary>Expand to fit the entire space, minus the margins at the start and end.</summary>
			public static UIDim Expand(float marginStart, float marginEnd) =>
				new UIDim {
					containerMode = false,
					anchorStart = 0,
					anchorEnd = 1,
					marginStart = marginStart,
					marginEnd = -marginEnd,
				};

			/// <summary>Directly specify the anchors and margins of the control.</summary>
			public static UIDim Custom(float anchorStart, float anchorEnd, float marginStart, float marginEnd) =>
				new UIDim {
					containerMode = false,
					anchorStart = anchorStart,
					anchorEnd = anchorEnd,
					marginStart = marginStart,
					marginEnd = -marginEnd,
				};
		}

		/// <summary>Layout options for use when the control is inside a container, which takes over assignment of anchors and margins.</summary>
		public static class Container {
			/// <summary>Causes the control to fill the entire space allocated to it, regardless of the size of it's contents.
			/// It will be a minumum of the specified minSize.</summary>
			public static UIDim Fill(float minSize = 0) =>
				new UIDim {
					containerMode = true,
					minSize = minSize,
					sizeFlags = Godot.Control.SizeFlags.Fill,
				};

			/// <summary>Causes the container to allocate as much space as it can to this control, but does not expand it to fill that space.
			/// It will be a minumum of the specified minSize.</summary>
			public static UIDim Expand(float minSize = 0) =>
				new UIDim {
					containerMode = true,
					minSize = minSize,
					sizeFlags = Godot.Control.SizeFlags.Expand,
				};

			/// <summary>Causes the container to allocate as much space as it can to this control, and expands the control to fill that space.
			/// It will be a minumum of the specified minSize.</summary>
			public static UIDim ExpandFill(float minSize = 0) =>
				new UIDim {
					containerMode = true,
					minSize = minSize,
					sizeFlags = Godot.Control.SizeFlags.ExpandFill,
				};

			/// <summary>Causes the container to shrink to the size of it's contents, or the specified minSize, whichever is larger, and be
			/// positioned at the top or left of it's allocated space.</summary>
			public static UIDim ShrinkStart(float minSize = 0) =>
				new UIDim {
					containerMode = true,
					minSize = minSize,
					sizeFlags = 0,
				};

			/// <summary>Causes the container to shrink to the size of it's contents, or the specified minSize, whichever is larger, and be
			/// positioned at the center of it's allocated space.</summary>
			public static UIDim ShrinkCenter(float minSize = 0) =>
				new UIDim {
					containerMode = true,
					minSize = minSize,
					sizeFlags = Godot.Control.SizeFlags.ShrinkCenter,
				};

			/// <summary>Causes the container to shrink to the size of it's contents, or the specified minSize, whichever is larger, and be
			/// positioned at the bottom or right of it's allocated space.</summary>
			public static UIDim ShrinkEnd(float minSize = 0) =>
				new UIDim {
					containerMode = true,
					minSize = minSize,
					sizeFlags = Godot.Control.SizeFlags.ShrinkEnd,
				};
		}
	}
}