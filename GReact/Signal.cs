using System;
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
		private static void CheckTarget(object target) {
			if (target != null && Attribute.IsDefined(target.GetType(), typeof(CompilerGeneratedAttribute))) {
				Godot.GD.PushWarning("This GReact signal was created with a lambda expression. This will cause performance issues, as the lambda will be recreated every frame, and thus always compare as unequal with the lambda from the previous frame. Please use a static function instead, and pass anything you would close over using the props struct.");
			}
		}
	}

	public abstract class Signal : Godot.Object, IEquatable<Signal> {
		private struct CallbackHolder {
			public Action callback;
		}

		private class SpecializedSignal<PropT> : Signal where PropT : notnull {
			private PropT props;
			private Action<PropT> callback;

			public SpecializedSignal(PropT props, Action<PropT> callback) {
				this.props = props;
				this.callback = callback;
			}

			public override void Call() {
				callback(props);
			}

			public override bool Equals(Signal other) {
				if (other is SpecializedSignal<PropT> castSignal) {
					return props.Equals(castSignal.props) && callback == castSignal.callback;
				}
				return false;
			}
		}

		public abstract void Call();
		public abstract bool Equals(Signal other);

		public static Signal New<PropT>(PropT props, Action<PropT> callback) where PropT : notnull {
			LambdaChecker.Check(callback);
			return new SpecializedSignal<PropT>(props, callback);
		}
		public static Signal New(Action callback) {
			LambdaChecker.Check(callback);
			return new SpecializedSignal<CallbackHolder>(new CallbackHolder { callback = callback }, CallCallback);
		}

		private static void CallCallback(CallbackHolder holder) => holder.callback();
	}

	public abstract class Signal<Arg1T> : Godot.Object, IEquatable<Signal> {
		private struct CallbackHolder {
			public Action<Arg1T> callback;
		}

		private class SpecializedSignal<PropT> : Signal<Arg1T> where PropT : notnull {
			private PropT props;
			private Action<PropT, Arg1T> callback;

			public SpecializedSignal(PropT props, Action<PropT, Arg1T> callback) {
				this.props = props;
				this.callback = callback;
			}

			public override void Call(Arg1T arg) {
				callback(props, arg);
			}

			public override bool Equals(Signal other) {
				if (other is SpecializedSignal<PropT> castSignal) {
					return props.Equals(castSignal.props) && callback == castSignal.callback;
				}
				return false;
			}
		}

		public abstract void Call(Arg1T arg);
		public abstract bool Equals(Signal other);

		public static Signal<Arg1T> New<PropT>(PropT props, Action<PropT, Arg1T> callback) where PropT : notnull {
			LambdaChecker.Check(callback);
			return new SpecializedSignal<PropT>(props, callback);
		}
		public static Signal<Arg1T> New(Action<Arg1T> callback) {
			LambdaChecker.Check(callback);
			return new SpecializedSignal<CallbackHolder>(new CallbackHolder { callback = callback }, CallCallback);
		}

		private static void CallCallback(CallbackHolder holder, Arg1T arg) => holder.callback(arg);
	}
}