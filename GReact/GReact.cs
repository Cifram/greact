using System;
using System.Collections.Generic;
using System.Linq;

namespace GReact {
	public class Renderer {
		private PopulatedElement? oldRootElem;
		public int nodesCreated { get; private set; }
		public int nodesDestroyed { get; private set; }
		public bool trackNodeChurn = false;

		private PopulatedElement Render(PopulatedElement? parent, int id, PopulatedElement? oldPopElem, Element elem) {
			var popElem = elem.Render(oldPopElem);

			var orderedChildren = new List<(Type, int)>();
			foreach (var childElem in elem.children) {
				var childKey = (childElem.nodeType, popElem.GetNextId(childElem.nodeType, childElem.id));
				if (popElem.children.ContainsKey(childKey)) {
					throw new Exception($"Duplicate key {childKey.Item1}-{childKey.Item2} between two GReact elements that are siblings");
				}
				orderedChildren.Add(childKey);
				popElem.children[childKey] = oldPopElem?.children.ContainsKey(childKey) ?? false ?
					Render(popElem, childKey.Item2, oldPopElem.Value.children[childKey], childElem) :
					Render(popElem, childKey.Item2, null, childElem);
			}

			if (oldPopElem == null) {
				if (trackNodeChurn) {
					nodesCreated++;
				}
				parent?.node.AddChild(popElem.node);
				popElem.node.Name = $"{popElem.node.GetType()}-{id}";
			} else {
				var keysToRemove = oldPopElem.Value.children.Keys.Where(key => !popElem.children.ContainsKey(key)).ToArray();
				foreach (var key in keysToRemove) {
					if (trackNodeChurn) {
						nodesDestroyed += GetNodeHierarchySize(oldPopElem.Value.children[key].node);
					}
					oldPopElem.Value.children[key].node.QueueFree();
				}

				for (int i = 0; i < orderedChildren.Count; i++) {
					var nodeInPosition = popElem.children[orderedChildren[i]].node;
					if (nodeInPosition.GetPositionInParent() != i) {
						popElem.node.MoveChild(nodeInPosition, i);
					}
				}
			}
			return popElem;
		}

		public void Render(Godot.Node parent, Element elem) {
			nodesCreated = 0;
			nodesDestroyed = 0;

			var popElem = Render(null, 0, oldRootElem, elem);
			if (oldRootElem == null) {
				parent.AddChild(popElem.node);
			}
			oldRootElem = popElem;
		}

		private int GetNodeHierarchySize(Godot.Node node) {
			var size = 1;
			for (int i = 0; i < node.GetChildCount(); i++) {
				size += GetNodeHierarchySize(node.GetChild(i));
			}
			return size;
		}
	}

	public interface INodeProps {
		int? id { get; }
	}

	public delegate Godot.Node CreateNode<PropT>(PropT props) where PropT : struct;
	public delegate void ModifyNode<PropT>(Godot.Node node, PropT oldProps, PropT props) where PropT : struct;

	public interface Element {
		Element Child(Element child);
		int? id { get; }
		Type nodeType { get; }
		PopulatedElement Render(PopulatedElement? old);
		List<Element> children { get; }
	}

	public struct Element<PropT, NodeT> : Element where PropT : struct, INodeProps where NodeT : Godot.Node {
		public List<Element> children { get; set; }
		public int? id { get; set; }
		public Type nodeType { get => typeof(NodeT); }
		public PropT props;
		public CreateNode<PropT> createNode;
		public ModifyNode<PropT> modifyNode;

		public static Element New(PropT props, CreateNode<PropT> createNode, ModifyNode<PropT> modifyNode) =>
			new Element<PropT, NodeT> {
				props = props,
				id = props.id,
				createNode = createNode,
				modifyNode = modifyNode,
				children = new List<Element>(),
			};

		public Element Child(Element child) {
			children.Add(child);
			return this;
		}

		public PopulatedElement Render(PopulatedElement? old) {
			if (old == null) {
				var node = createNode(props);
				return new PopulatedElement(this, node);
			} else {
				if (old.Value.elem is Element<PropT, NodeT> oldElem) {
					modifyNode(old.Value.node, oldElem.props, props);
					return new PopulatedElement(this, old.Value.node);
				} else {
					throw new Exception("Node somehow changed type while maintaining the same key");
				}
			}
		}
	}

	public struct PopulatedElement {
		public Element elem;
		public Godot.Node node;
		public Dictionary<(Type, int), PopulatedElement> children;
		public Dictionary<Type, int> maxChildIds;

		public PopulatedElement(Element element, Godot.Node node) {
			this.elem = element;
			this.node = node;
			this.children = new Dictionary<(Type, int), PopulatedElement>();
			this.maxChildIds = new Dictionary<Type, int>();
		}

		public int GetNextId(Type type, int? explicitId) {
			if (explicitId == null) {
				if (maxChildIds.ContainsKey(type)) {
					maxChildIds[type]++;
					return maxChildIds[type];
				}
				maxChildIds[type] = 0;
				return 0;
			} else {
				if (!maxChildIds.ContainsKey(type) || maxChildIds[type] < explicitId.Value) {
					maxChildIds[type] = explicitId.Value;
				}
				return explicitId.Value;
			}
		}
	}
}