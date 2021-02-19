namespace GReact {
	public struct UIDim {
		public float anchorStart;
		public float anchorEnd;
		public float marginStart;
		public float marginEnd;

		public static UIDim Start(float size) =>
			new UIDim {
				anchorStart = 0,
				anchorEnd = 0,
				marginStart = 0,
				marginEnd = size,
			};
		public static UIDim End(float size) =>
			new UIDim {
				anchorStart = 1,
				anchorEnd = 1,
				marginStart = -size,
				marginEnd = 0,
			};
		public static UIDim Center(float size) =>
			new UIDim {
				anchorStart = 0.5f,
				anchorEnd = 0.5f,
				marginStart = -size/2,
				marginEnd = size/2,
			};
		public static UIDim Expand(float marginStart, float marginEnd) =>
			new UIDim {
				anchorStart = 0,
				anchorEnd = 1,
				marginStart = marginStart,
				marginEnd = -marginEnd,
			};
		public static UIDim Custom(float anchorStart, float anchorEnd, float marginStart, float marginEnd) =>
			new UIDim {
				anchorStart = anchorStart,
				anchorEnd = anchorEnd,
				marginStart = marginStart,
				marginEnd = -marginEnd,
			};
	}
}