using System;
using System.Collections.Generic;

namespace UIExample {
	public class State {
		public Dictionary<int, List<string>> lists = new Dictionary<int, List<string>>();
		public int maxId = 0;

		public void Apply(Action<State> action) {
			action(this);
		}

		public static Action<State> AddList() => state => {
			state.lists[state.maxId] = new List<string>();
			state.maxId++;
		};

		public static Action<State> RemoveList(int listId) => state => {
			state.lists.Remove(listId);
		};

		public static Action<State> AddItemToList(int listId) => state => {
			state.lists[listId].Add("New Item");
		};

		public static Action<State> RemoveItemFromList(int listId, int itemIndex) => state => {
			state.lists[listId].RemoveAt(itemIndex);
		};

		public static Action<State> ChangeItem(int listId, int itemIndex, string newValue) => state => {
			state.lists[listId][itemIndex] = newValue;
		};
	}
}