# Declarative Programming

A core principle of GReact is that it's declarative. Declarative programming is a big topic which you can read about elsewhere, but in this context it means the focus is on declaring how the world looks, right now, based on the provided state, and NOT on how the world is updating or changing.

This doesn't mean the world can't change or update, just that the state and presentation are broken into entirely separate layers. The state updates each frame, and the presentation is then constructed based on the current state. The presentation in this case is the Godot scene graph composed of `Node`s, and GReact is the system for building that presentation based on the current state.

The advantage of this is that it greatly simplifies what you have to think about, as the programmer. When building your presentation, you don't have to care about all the transitions and changes. You don't have to think about all the steps required to update each thing in the scene graph that cares about a given change. On one side, you just change the state. And on the other side, you just build a thing based on the current state.

In order to accomplish this, GReact creates the illusion that you're rebuilding the entire scene graph from scratch each frame. Under the hood, this isn't quite what's happening. Rather, you build up a lightweight representation of the scene graph each frame, and then GReact considers the difference between what you built last frame, and what you built this frame, and only updates the parts of the scene graph that actually changed. But when using it, you rarely have to think about this.

# High Level Structure

Core to GReact is the relationship between components, `Element`s and `Node`s.

A component, really, is just a function, which builds a portion of the scene graph, a `Node` and its children, given some current state. It does this by taking the state as arguments, and returning an `Element`, where an `Element` is a lightweight representation of a `Node`. That `Element` can, of course, have child `Element`s, which can be defined directly by this component, or by other components called by this one.

The class that pulls this all together is the `Renderer`. Create a `GReact.Renderer` to represent a scene graph you want to manage with GReact, and call `Render` on it, passing in an `Element` for the root node, and the `Node` that you want it to be parented to. It will build the actual Godot scene graph from that. When you call `Render` on this same renderer again, it will build up the updated scene graph of `Element`s, compare it to the one from the last time it was called, and use that comparison to update the actual Godot scene graph, only where required.

# Components

By convention, each component is a static function called `New`, on a static class whose name ends with `Component`. For instance, `CharacterComponent.New` or `ChatBoxComponent.New` would be reasonable components. This function needs to take the relevant state as arguments, and return an `Element`.

Components can be divided into two categories: those that directly represent a specific `Node` class, called node components, and those that represent one or more node components combined into a hierarchy, called composite components. These follow slightly different conventions.

When using GReact, you will almost exclusively be writing composite components. GReact has a collection of node components to represent Godot's built-in `Node` classes (though that collection is currently woefully incomplete, and contributions are welcome to help address this), and there's rarely a reason to write your own custom `Node` class to be managed by GReact. But, you will need to use node components all the time.

By convention, node components are named as the `Node` class name followed by `Component`, such as `ButtonComponent` for the `Button` `Node` class. Also by convention, they take two arguments: a string key, and a a props struct. The props struct is a struct containing all the properties needed by that `Node` class, and by convention is named by the node class name followed by `Props`, such as `ButtonProps`. So to make a button, you'd do something like:

```c#
ButtonComponent.New("MyButton", new ButtonProps {
  vert = UIDim.JustifyCenter(20),
  horiz = UIDim.JustifyCenter(100),
  text = "Press Me!",
  pressed = Signal.New(OnButtonPress),
})
```

How the `horiz` and `vert` props work with `UIDim`, and how `Signal` works, will be covered in later sections. The important part is the general structure of how you instantiate a standard node component. The reason for the props struct, instead of just taking all the properties directly as function arguments, is so the props can be stored in the newly created `Element` and saved, which allows them to be compared with the props from the matching `Element` from the last frame to understand what has changed.

Composite components do not technically need to use a props struct because they're not directly constructing an `Element`. They could just take whatever state they need directly as function arguments. However, by convention and for consistency, they still use props structs. Using a composite component should feel indistinguishable from using a node component.

So let's see an example of a simple composite component:

```c#
using GReact;

public struct ThingyProps {
  public int id;
  public string label1;
  public string label2;
}

public static class ThingComponent {
  public static Element New(ThingyProps props) {
    return HBoxContainerComponent.New($"Thingy-{props.id}", new HBoxContainerProps {
      vert = UIDim.JustifyExpand(0, 0),
      horiz = UIDim.JustifyExpand(0, 0),
    }).Child(
      LabelComponent.New($"Thingy-{props.id}-Label1", new LabelProps {
        text = props.label1,
      })
    ).Child(
      LabelComponent.New($"Thingy-{props.id}-Label2", new LabelProps {
        text = props.label2,
      })
    );
  }
}
```

This creates an `HBoxContainer` with two `Label`s as children. Again, the usage `UIDim` will be explained later, suffice it to say this will cause the `HBoxContainer` to expand to fill its entire parent region. Note the calls to the `Child` function on `Element`. After an element is returned by calling another component, be it a node or composite component, you can call `Child` on it, and pass in another `Element`, to add that `Element` as a child, and the `Child` method returns the original element, so you can chain these calls.

A note about the key passed in to each node component: this key needs to be globally unique. This is how GReact figures out that which `Element` this frame corresponds to the same `Element` in the previous frame, even if it's in a different part of the scene graph. If two `Element`s are given the same key, it can't maintain proper consistency between frames, and thus will throw an exception.

# Renderer and Dispatcher

The renderer is pretty simple: Create and store a `GReact.Renderer`, and every frame call `Render` on it, passing in the `Element` for the root `Node`.

By convention, this is managed by what we call a dispatcher. This is a custom `Node` which initializes our global state, and creates the renderer, on initialization, and updates that state, and calls `Render` on the renderer, every frame. It's called a dispatcher because it dispatches these events, initialization and updating of the scene, to any other code that needs it. In a large, complicated program, it's not expected that the dispatcher manages all of these things directly, but rather that it's the entry point that delegates to everything else that needs to do initialization or updates.

A simple dispatcher might look something like this:

```c#
public class Dispatcher : Godot.Node {
  private StateStore state;
  private GReact.Renderer renderer = new GReact.Renderer();

  void Start() {
    state = new StateStore();
  }

  public override void _Process(float delta) {
    state.Update(delta);

    renderer.Render(this, RootNode.New(new RootProps {
      state = state,
    }));

    base._Process(delta);
  }
}
```

Where `StateStore` is a stand-in for whatever class you create for managing your global state.

It's quite possible and sometimes sensible to maintain more than one renderer. Each one will maintain its own section of the Godot scene graph, parented to whatever parent `Node` you provide to the `Render` function. An example of where this might make sense is having one renderer for the game geometry and another one for the UI. Or, if you need to run two scenes in parallel on split screen, giving each one its own renderer might make sense. It's up to you to decide what makes the most sense for your circumstances.

# Signals

It's often the case that you want to pass some kind of callback to be activated when Godot fires a signal on a `Node`. For instance, you might want to respond to a `Button` being pressed. There is some trickiness here. Due to the way Godot registers callbacks on signals, and the fact that we want to be able to compare the callbacks from one frame to the next to avoid replacing them unnecessarily, your callbacks should always be static functions, not lambda expressions.

However, GReact provides the convenient `Signal` class to manage this for you. `Signal.New()` will take a static function as a callback, and optionally take a props struct that it will pass into that function when it's called. If no props struct is supplied, then the callback provided must also be one that doesn't take a props struct. If the underlying Godot signal requires an argument, the function must take that argument as well. So callback may or may not take a props struct, depending on whether you supply one, and may or may not take an additional argument, depending on whether the underlying Godot signal provides one. If it takes both, the props argument comes first.

Here's an example of a component wrapping a button, showing how it uses a GReact `Signal` to respond to button presses.

```c#
public struct MyButtonProps {
  int id;
  string response;
}

public static class MyButtonComponent {
  public static Element New(MyButtonProps props) {
    return ButtonComponent.New($"MyButton-{id}", new ButtonProps {
      vert = UIDim.JustifyCenter(20),
      horiz = UIDim.JustifyCenter(100),
      text = "Press Me!",
      pressed = Signal.New(props, OnButtonPress),
    })
  }

  private static void OnButtonPress(MyButtonProps props) {
    Godot.GD.Print($"Button response: {props.response}");
  }
}
```

As you see here, you can generally just reuse the component's props struct to pass on to the callback, and declare the callback as a `private static` function on the same class as your component's `New` function. So it's all much simpler in practice than it might sound in the description.

# Rules of Good GReact

Now that you've got the structure of GReact down, there are some very important rules to understand for any code that interacts with GReact. Breaking these rules breaks the GReact paradigm, and will tend to cause things to not function properly.

## 1. Components should never read from the scene graph

The GReact paradigm means that components are given all the data they need to create their portion of the scene graph from scratch, and should always pretend they're doing that. And if you're always making an entirely new scene graph from scratch, why would you look at the old one?

## 2. Nodes managed by node components must not be modified by anything else

If a node component is managing a `Node`, it needs to know it has total and sole control over that `Node`. The component is given some properties that determine the state of the `Node` it manages, and when it's done, the `Node` needs to entirely match what those properties indicate. But, it's comparing against the properties from the last frame to avoid changing things it doesn't need to. If something else has been messing with the `Node` in the meantime, some of those values may no longer be what's indicated by the old properties, and the component may not overwrite things it needs to.
