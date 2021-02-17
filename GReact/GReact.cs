using System;
using System.Collections.Generic;
using System.Linq;

namespace GReact {
	public class Renderer {
		private PopulatedElement? oldGraph;

		public void Render(Godot.Node parent, IElement newGraph) {
			if (oldGraph == null) {
				oldGraph = BuildFirstTimeGraph(newGraph, parent, null);
			} else {
				// Build dictionaries of new and old elements indexed by key, to diff against
				var oldElemDict = new Dictionary<string, PopulatedElement>();
				var newElemDict = new Dictionary<string, (IElement, string?)>();
				FillOldElemDict(oldElemDict, oldGraph.Value);
				FillNewElemDict(newElemDict, newGraph);

				// Build the creation, move and update queues, by iterating over the new
				// elements and comparing to the old ones
				var creationQueue = new List<(IElement, string?)>();
				var moveQueue = new List<(string, string?)>();
				var updateQueue = new List<string>();
				foreach (var newElem in newElemDict) {
					if (oldElemDict.ContainsKey(newElem.Key)) {
						var (elem, parentKey) = newElem.Value;
						var oldElem = oldElemDict[newElem.Key];
						if (oldElem.parentKey != parentKey) {
							moveQueue.Add((elem.key, parentKey));
						}
						updateQueue.Add(elem.key);
					} else {
						creationQueue.Add(newElem.Value);
					}
				}

				// Build the destruction queue by iterating over the old elements and comparing
				// to the new ones
				var destructionQueue = new List<Godot.Node>();
				foreach (var oldElem in oldElemDict) {
					if (!newElemDict.ContainsKey(oldElem.Key)) {
						destructionQueue.Add(oldElem.Value.node);
					}
				}

				// As we update and create elements, they will be converted to PopulatedElements.
				// The set of elements in the update and create queues should exactly emcompass all
				// the elements in the new graph, so by the time those are processed this dictionary
				// should contain all of the new elements, to allow us to rebuild the graph of
				// populated elements at the end.
				var newPopElemDict = new Dictionary<string, PopulatedElement>();

				// Execute the update queue
				foreach (var updateKey in updateQueue) {
					var oldElem = oldElemDict[updateKey];
					(var newElem, _) = newElemDict[updateKey];
					newPopElemDict[updateKey] = newElem.Render(oldElem);
				}
				// Execute the creation queue
				// Do this after the update queue to make sure that any new node's parent nodes are
				// already in the newPopElemDict.
				foreach (var (newElem, parentKey) in creationQueue) {
					var popElem = newElem.Render(null);
					if (parentKey == null) {
						parent.AddChild(popElem.node);
					} else {
						newPopElemDict[parentKey].node.AddChild(popElem.node);
						popElem.parentKey = parentKey;
					}
					newPopElemDict[popElem.elem.key] = popElem;
				}
				// Execute the move queue
				// Do this after the creation queue, to make sure the parent objects exist.
				foreach (var (src, dst) in moveQueue) {
					var popElem = newPopElemDict[src];
					popElem.parentKey = dst;
					if (dst == null) {
						parent.AddChild(popElem.node);
					} else {
						newPopElemDict[dst].node.AddChild(popElem.node);
					}
					newPopElemDict[src] = popElem;
				}
				// Execute the destruction queue
				// Do this last, so we don't inadvertently destroy children that got unparented from
				// this GameObject.
				foreach (var node in destructionQueue) {
					node.Free();
				}

				// Build the new populated graph
				oldGraph = PopulateGraph(newGraph, newPopElemDict);
			}
		}

		private PopulatedElement PopulateGraph(IElement elem, Dictionary<string, PopulatedElement> popElems) {
			var popElem = popElems[elem.key];
			popElem.children = elem.children.Select(child => PopulateGraph(child, popElems)).ToArray();
			return popElem;
		}

		private void FillOldElemDict(Dictionary<string, PopulatedElement> dict, PopulatedElement elem) {
			dict[elem.elem.key] = elem;
			foreach (var child in elem.children) {
				FillOldElemDict(dict, child);
			}
		}

		private void FillNewElemDict(Dictionary<string, (IElement, string?)> dict, IElement elem, string? parentKey = null) {
			if (dict.ContainsKey(elem.key)) {
				throw new Exception($"Two GReact elements have the key {elem.key}");
			}
			dict[elem.key] = (elem, parentKey);
			foreach (var child in elem.children) {
				FillNewElemDict(dict, child, elem.key);
			}
		}

		private PopulatedElement BuildFirstTimeGraph(IElement elem, Godot.Node parentNode, string? parentKey) {
			var newElem = elem.Render(null);
			parentNode.AddChild(newElem.node);
			newElem.parentKey = parentKey;
			newElem.children = elem.children.Select(child => BuildFirstTimeGraph(child, newElem.node, elem.key)).ToArray();
			return newElem;
		}
	}

	public delegate Godot.Node CreateNode<PropT>(PropT props) where PropT : struct;
	public delegate void ModifyNode<PropT>(Godot.Node node, PropT oldProps, PropT props) where PropT : struct;

	public interface IElement {
		IElement Child(IElement child);
		PopulatedElement Render(PopulatedElement? old);
		string key { get; }
		List<IElement> children { get; }
	}

	public struct Element<PropT> : IElement where PropT : struct {
		public string key { get; set; }
		public List<IElement> children { get; set; }
		public PropT props;
		public CreateNode<PropT> createNode;
		public ModifyNode<PropT> modifyNode;

		public static IElement New(string key, PropT props, CreateNode<PropT> createNode, ModifyNode<PropT> modifyNode) {
			return new Element<PropT> {
				key = key,
				props = props,
				createNode = createNode,
				modifyNode = modifyNode,
				children = new List<IElement>(),
			};
		}

		public IElement Child(IElement child) {
			children.Add(child);
			return this;
		}

		public PopulatedElement Render(PopulatedElement? old) {
			if (old == null) {
				return new PopulatedElement(this, createNode(props));
			} else {
				if (old.Value.elem is Element<PropT> oldElem) {
					modifyNode(old.Value.node, oldElem.props, props);
					return new PopulatedElement(this, old.Value.node);
				} else {
					var newNode = createNode(props);
					var oldNode = old.Value.node;
					var oldParent = oldNode.GetParent();
					if (oldParent != null) {
						oldParent.AddChild(newNode);
					}
					for (var i = 0; i < oldNode.GetChildCount(); i++) {
						newNode.AddChild(oldNode.GetChild(i));
					}
					oldNode.Free();
					return new PopulatedElement(this, newNode);
				}
			}
		}

		public string GetKey() {
			return key;
		}
	}

	public struct PopulatedElement {
		public IElement elem;
		public Godot.Node node;
		public PopulatedElement[] children;
		public string? parentKey;

		public PopulatedElement(IElement element, Godot.Node node) {
			this.elem = element;
			this.node = node;
			this.children = new PopulatedElement[0];
			this.parentKey = null;
		}
	}
}