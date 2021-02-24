using System.Collections.Generic;

namespace UIExample {
	public class Column {
		public List<string> items = new List<string>();
	}

	public class State {
		public Dictionary<int, Column> columns = new Dictionary<int, Column>();
		public int maxId = 0;
	}
}