using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GReact {
	public static class LambdaChecker {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Check(Action callback) {
			if (Godot.OS.IsDebugBuild()) {
				CheckTarget(callback.Target);
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Check<T>(Action<T> callback) {
			if (Godot.OS.IsDebugBuild()) {
				CheckTarget(callback.Target);
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Check<T1, T2>(Action<T1, T2> callback) {
			if (Godot.OS.IsDebugBuild()) {
				CheckTarget(callback.Target);
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Check<T1, T2, T3>(Action<T1, T2, T3> callback) {
			if (Godot.OS.IsDebugBuild()) {
				CheckTarget(callback.Target);
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void CheckTarget(object? target) {
			if (target != null && Attribute.IsDefined(target.GetType(), typeof(CompilerGeneratedAttribute))) {
				Godot.GD.PushWarning("This GReact signal was created with a lambda expression. This will cause performance issues, as the lambda will be recreated every frame, and thus always compare as unequal with the lambda from the previous frame, causing it to reconnect the signal every frame. It is recommended to use a static function instead, and pass anything you would close over using the props argument.");
			}
		}
	}

	public abstract class Signal : Godot.Object {
		private struct CallbackHolder {
			public Action<Godot.Node> callback;
		}

		private Godot.Node? node = null;

		private class SpecializedSignal<PropT> : Signal where PropT : notnull {
			private PropT props;
			private Action<Godot.Node, PropT> callback;

			public SpecializedSignal(Action<Godot.Node, PropT> callback, PropT props) {
				this.props = props;
				this.callback = callback;
			}

			public override void Call() {
				if (node == null) {
					throw new Exception("Signal somehow triggered without a valid node reference");
				}
				callback(node, props);
			}

			public override bool Equals(object? other) {
				if (other is SpecializedSignal<PropT> signal) {
					return props.Equals(signal.props) && callback == signal.callback;
				}
				return false;
			}

			public override int GetHashCode() {
				int hashCode = -16276663;
				hashCode = hashCode * -1521134295 + NativeInstance.GetHashCode();
				hashCode = hashCode * -1521134295 + EqualityComparer<PropT>.Default.GetHashCode(props);
				hashCode = hashCode * -1521134295 + EqualityComparer<Action<Godot.Node, PropT>>.Default.GetHashCode(callback);
				return hashCode;
			}
		}

		public abstract void Call();
		public override abstract bool Equals(object? other);
		public abstract override int GetHashCode();

		public static Signal New<PropT>(Action<Godot.Node, PropT> callback, PropT props) where PropT : notnull {
			LambdaChecker.Check(callback);
			return new SpecializedSignal<PropT>(callback, props);
		}
		public static Signal New(Action<Godot.Node> callback) {
			LambdaChecker.Check(callback);
			return new SpecializedSignal<CallbackHolder>(CallCallback, new CallbackHolder { callback = callback });
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Connect(Godot.Node node, string signalName, Signal? oldSignal) {
			this.node = node;
			if (oldSignal == null || !Equals(oldSignal)) {
				if (oldSignal != null) {
					node.Disconnect(signalName, oldSignal, nameof(oldSignal.Call));
				}
				node.Connect(signalName, this, nameof(this.Call));
			}
		}

		private static void CallCallback(Godot.Node node, CallbackHolder holder) => holder.callback(node);
	}

	public abstract class Signal<Arg1T> : Godot.Object where Arg1T : notnull {
		private struct CallbackHolder {
			public Action<Godot.Node, Arg1T> callback;
		}

		private Godot.Node? node = null;

		private class SpecializedSignal<PropT> : Signal<Arg1T> where PropT : notnull {
			private PropT props;
			private Action<Godot.Node, PropT, Arg1T> callback;

			public SpecializedSignal(Action<Godot.Node, PropT, Arg1T> callback, PropT props) {
				this.props = props;
				this.callback = callback;
			}

			protected override void Call(Arg1T arg) {
				if (node == null) {
					throw new Exception("Signal somehow triggered without a valid node reference");
				}
				callback(node, props, arg);
			}
			public override bool Equals(object? other) {
				if (other is SpecializedSignal<PropT> signal) {
					return props.Equals(signal.props) && callback == signal.callback;
				}
				return false;
			}

			public override int GetHashCode() {
				int hashCode = -16276663;
				hashCode = hashCode * -1521134295 + NativeInstance.GetHashCode();
				hashCode = hashCode * -1521134295 + EqualityComparer<PropT>.Default.GetHashCode(props);
				hashCode = hashCode * -1521134295 + EqualityComparer<Action<Godot.Node, PropT, Arg1T>>.Default.GetHashCode(callback);
				return hashCode;
			}
		}

		protected abstract void Call(Arg1T arg);
		public override abstract bool Equals(object? other);
		public abstract override int GetHashCode();

		public static Signal<Arg1T> New<PropT>(Action<Godot.Node, PropT, Arg1T> callback, PropT props) where PropT : notnull {
			LambdaChecker.Check(callback);
			return new SpecializedSignal<PropT>(callback, props);
		}
		public static Signal<Arg1T> New(Action<Godot.Node, Arg1T> callback) {
			LambdaChecker.Check(callback);
			return new SpecializedSignal<CallbackHolder>(CallCallback, new CallbackHolder { callback = callback });
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Connect(Godot.Node node, string signalName, Signal<Arg1T>? oldSignal) {
			this.node = node;
			if (oldSignal == null || !Equals(oldSignal)) {
				if (oldSignal != null) {
					node.Disconnect(signalName, oldSignal, nameof(oldSignal.Call));
				}
				node.Connect(signalName, this, nameof(this.Call));
			}
		}

		private static void CallCallback(Godot.Node node, CallbackHolder holder, Arg1T arg) => holder.callback(node, arg);
	}
}