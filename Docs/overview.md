# Declarative Programming

A core principle of GReact is that it's declarative. Declarative programming is a big topic which you can read about elsewhere, but in this context it means the focus is on declaring how the world looks, right now, based on the provided state, and NOT on how the world is updating or changing.

This doesn't mean the world can't change or update, just that the state and presentation are broken into entirely separate layers. The state updates each frame, and the presentation is then constructed based on the current state. The presentation in this case is the Godot scene graph composed of `Node`s, and GReact is the system for building that presentation based on the current state.

The advantage of this is that it greatly simplifies what you have to think about, as the programmer. When building your presentation, you don't have to care about all the transitions and changes. You don't have to think about all the steps required to update each thing in the scene graph that cares about a given change. On one side, you just change the state. And on the other side, you just build a thing based on the current state.

In order to accomplish this, GReact creates the illusion that you're rebuilding the entire scene graph from scratch each frame. Under the hood, this isn't quite what's happening. Rather, you build up a lightweight representation of the scene graph each frame, and then GReact considers the difference between what you built last frame, and what you built this frame, and only updates the parts of the scene graph that actually changed. But when using it, you rarely have to think about this.

# High Level Structure

Core to GReact is the relationship between components, `Element`s and `Node`s.

A component, really, is just a function, which builds a portion of the scene graph, a `Node` and its children, given some current state. It does this by taking the state as arguments, and returning an `Element`, where an `Element` is a lightweight representation of a `Node`. That `Element` can have child `Element`s, which can be defined directly by this component, or by other components called by this one.

The class that pulls this all together is the `Renderer`. Create a `GReact.Renderer` to represent a scene graph you want to manage with GReact, and call `Render` on it, passing in an `Element` for the root node, and the `Node` that you want it to be parented to. It will build the actual Godot scene graph from that. When you call `Render` on this same renderer again, it will build up the updated scene graph of `Element`s, compare it to the one from the last time it was called, and use that comparison to update the actual Godot scene graph only where required.

# Components

Each component is a static function called `New`, on a static class with the `[Component]` tag on it. By convention, it's name ends with `Component`. For instance, `CharacterComponent.New` or `ChatBoxComponent.New` would be reasonable components. This function needs to take the relevant state as a struct, and return an `Element`. That struct should be defined internally to the component class, and be called `Props` (short for properties). The broad outline of a component thus looks something like:

```c#
[Component]
public static class FooComponent {
  struct Props {
    public string text;
  }

  public static Element New(Props props) {
    // actual component logic here
  }
}
```

Components can be divided into two categories: those that directly represent a specific `Node` class, called node components, and those that represent one or more components combined into a hierarchy, called composite components. These follow somewhat different conventions internally, but they're called in exactly the same way, so when using a component you don't necessarily have to think about whether it's a node component or a composite component.

The reason for using the `Props` struct, instead of just passing in all the properties directly as function argument, is so that the properties can be stored between frames. This allows it to compare the props from this frame with the props from last frame to make decisions about what needs to be updated. This is particularly useful for node components, which internally compare the old and new props to decide which fields on the node to update.

When using GReact, you will almost exclusively be writing composite components. GReact has a collection of node components to represent Godot's built-in `Node` classes (though that collection is currently woefully incomplete, and contributions are welcome to help address this), and there's rarely a reason to write your own custom `Node` class to be managed by GReact. But, you will need to use node components all the time.

GReact uses a source generator to provide a much cleaner API for calling components. So it can take that component defined above, and allow you to call it like so:

```c#
Component.Foo(text: "Fighters")
```

So it essentially creates a static class called `Component`, which contains a static function for each component in the codebase, with an optional named argument for every field in it's `Props` struct. This is why it's important for components to hold to such a specific, rigid structure. The source generator will give you a compile-time error if this structure is wrong, though sadly some IDEs (notable Visual Studio Code) do not properly display those errors at this time.

Now, if we wanted to call a more useful component, like say `ButtonComponent`, that would look like this:

```c#
Component.Button(
  vert: UIDim.Manual.Center(20),
  horiz: UIDim.Manual.Center(100),
  text: "Press Me!",
  onPressed: Signal.New(OnButtonPress),
)
```

How the `horiz` and `vert` props work with `UIDim` is covered below in the [UI Positioning](#uipos) section, and how `Signal` works is covered in the [Signals](#signals) section.

Composite components do not technically need to use a props struct because they're not directly constructing an `Element`. They could just take whatever state they need directly as function arguments. However, by convention and for consistency, they still use props structs. Using a composite component should feel indistinguishable from using a node component.

So let's see an example of a simple composite component:

```c#
using GReact;

public static class ThingComponent {
  public struct Props {
    public int id;
    public string label1;
    public string label2;
  }

  public static Element New(Props props) =>
    Component.HBoxContainer(
      id: id,
      vert: UIDim.Manual.Expand(0, 0),
      horiz: UIDim.Manual.Expand(0, 0),
    ).Children(
      Component.Label(
        text: props.label1,
      ),
      Component.Label(
        text: props.label2,
      )
    );
}
```

This creates an `HBoxContainer` with two `Label`s as children. Again, the usage `UIDim` will be explained later, suffice it to say this will cause the `HBoxContainer` to expand to fill its entire parent region. Note the call to the `Children` function on `Element`. After an element is returned by calling another component, be it a node or composite component, you can call `Children` on it, and pass in any number of additional `Element`s, to add them as children to the original element. The function will return the original element.

You can also specify children by calling the `Child` function, which takes a single argument. If you're only adding a single child, this is marginally more efficient, because variadic functions require an allocation for the array that stores all the arguments. If you're adding multiple children, then the overhead of the extra function calls will generally overwhelm the savings from the allocation.

The `id` field passed in `HBoxContainerProps` is of special note. This is an optional integer prop available on all node components, and is used to maintain identity of the underlying node between frames. That is, if a component has the same type, in the same place in the scene graph, with the same ID, then GReact assumes it refers to the same node, and updates the existing node instead of creating a new one. If you do not specify an ID, GReact will still assign IDs, sequentially from 0, for each sibling of the same type, and this is fine for most circumstances. However, if you have a list of items that can have elements added or removed in the middle, then when that happens it may not maintain identity correctly, and it's valuable to be able to specify the IDs explicitly.

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

    renderer.Render(this, Component.Root(state: state));

    base._Process(delta);
  }
}
```

Where `StateStore` is a stand-in for whatever class you create for managing your global state.

It's quite possible and sometimes sensible to maintain more than one renderer. Each one will maintain its own section of the Godot scene graph, parented to whatever parent `Node` you provide to the `Render` function. An example of where this might make sense is having one renderer for the game geometry and another one for the UI. Or, if you need to run two scenes in parallel on split screen, giving each one its own renderer might make sense. It's up to you to decide what makes the most sense for your circumstances.

# <a name="signals"></a>Signals

It's often the case that you want to pass some kind of callback to be activated when Godot fires a signal on a `Node`. For instance, you might want to respond to a `Button` being pressed. There is some trickiness here. Due to the way Godot registers callbacks on signals, and the fact that we want to be able to compare the callbacks from one frame to the next to avoid replacing them unnecessarily, your callbacks should always be static functions, not lambda expressions.

However, GReact provides the convenient `Signal` class to manage this for you. `Signal.New()` will take a static function as a callback, and optionally take a props struct that it will pass into that function when it's called. If no props struct is supplied, then the callback provided must also be one that doesn't take a props struct. If the underlying Godot signal requires an argument, the function must take that argument as well. So callback may or may not take a props struct, depending on whether you supply one, and may or may not take an additional argument, depending on whether the underlying Godot signal provides one. If it takes both, the props argument comes first.

Here's an example of a component wrapping a button, showing how it uses a GReact `Signal` to respond to button presses.

```c#
public static class MyButtonComponent {
  public struct Props {
    int id;
    string response;
  }

  public static Element New(Props props) {
    return Component.Button(
      vert: UIDim.Manual.Center(20),
      horiz: UIDim.Manual.Center(100),
      text: "Press Me!",
      onPressed: Signal.New(OnButtonPress, props),
    )
  }

  private static void OnButtonPress(Node node, MyButtonProps props) {
    Godot.GD.Print($"Button response: {props.response}");
  }
}
```

As you see here, you can often just reuse the component's props struct to pass on to the callback, and declare the callback as a `private static` function on the same class as your component's `New` function. Sometimes you also need an ID of some sort, to specify which thing the signal is actually operating on, in which case it's convenient to use a tuple of the component's prop struct and the ID, like so:

```c#
using System.Linq;

public static class MyButtonListComponent {
  public struct Props {
    int id;
    string[] buttonResponses;
  }

  public static Element New(Props props) =>
    Component.VBoxContainer(
      vert: UIDim.Container.Expand(),
      horiz: UIDim.Container.Expand()
    ).Children(
      props.buttonResponses.Select((response, index) =>
        Component.Button(
          horiz: UIDim.Container.ShrinkStart(100),
          text: $"Button {index}",
          onPressed: Signal.new(OnButtonPress, (props, index))
        )
      )
    );

  private static void OnButtonPress(Node node, (ButtonListProps, int) args) {
    var (props, index) = args;
    Godot.GD.Print($"Button response: {props.buttonResponses[index]}");
  }
}
```

You'll notice the signal callback is also passed the `Node` the signal is registered on. Be very careful with this. There are cases where the only way to get the desired behavior is to have a signal directly modify the `Node` it's attached to, but this should only ever be used to interact with the aspects of the `Node` that GReact does not directly manage. A valid use for this might be grabbing focus on the `onReady` signal for a control, so it comes into existence with focus, as focus is something that GReact can't otherwise manage.

# <a name="uipos"></a>UI Positioning

Godot's system for manually positioning UI elements is very flexible, but it requires specifying 8 numbers for each `Node` in the UI (an anchor value and a margin value for top, bottom, left and right), and the correct values for these numbers is sometimes hard to think about. Add to this that when a control is inside a container, it has a completely different set of values to specify how it's formatted, being a set of flags for how it gets scaled and a minimum size. So GReact provides some convenient shortcuts for the common use cases.

The `UIDim` class encompasses the positioning along one axis. It has two modes: manual mode for controls not in containers, and container mode for controls in containers. If in manual mode, it stores a `startAnchor`, `endAnchor`, `startMargin` and `endMargin`, where start is left or top, and end is right or bottom, depending on whether it's horizontal or vertical. If it's in container mode, it stores a `minSize` and `sizeFlags`. Every control component takes a `UIDim vert` property and a `UIDim horiz` property, and will assign the fields from these `UIDim`s to the appropriate fields on the control. Where this saves you time and effort is through a set of static functions that serve as specialized constructors for `UIDim`:

- `UIDim.Manual.Start(size)` - Justifies the control to the left or top, with the specified size. Sets both anchors to 0, along with the start margin, and sets the end margin to the `size`.
- `UIDim.Manual.End(size)` - Justify the control to the right or bottom, with the specified size. Sets both anchors to 1, the start margin to `-size`, and the end margin to 0.
- `UIDim.Manual.Center(size)` - Centers the control, with the specified size. Sets both anchors to 0.5, the start margin to `-size/2` and the end margin to `size/2`.
- `UIDim.Manual.Expand(start, end)` - Makes the control expand to fit the container, with the specified margins at the start and end. Sets the start anchor to 0, the end anchor to 1, the start margin to `start` and the end margin to `-end`.
- `UIDim.Manual.Custom(startAnchor, endAnchor, startMargin, endMargin)` - Sets the values directly, for when you need to do something special.
- `UIDim.Container.Fill(minSize = 0)` - Make the control fill the entire space allocated to it, with a minimum size specified.
- `UIDim.Container.Expand(minSize = 0)` - Makes the container allocate as much space for this control as it can, but does not expand the control to fill that space. The control will have it's size determined by it's contents, or the specified minSize, whichever is larger.
- `UIDim.Container.ExpandFill(minSize = 0)` - Makes the container allocate as much space for this control as it can, and expands the control to fill that space, with a minimum size specified.
- `UIDim.Container.ShrinkStart(minSize = 0)` - Shrink the control to the size of it's contents, or the specified minimum size, whichever is larger, and justifies to the top or left.
- `UIDim.Container.ShrinkCenter(minSize = 0)` - Shrink the control to the size of it's contents, or the specified minimum size, whichever is larger, and centers it.
- `UIDim.Container.ShrinkEnd(minSize = 0)` - Shrinks the control to the size of it's contents, or the specified minimum size, whichever is larger, and justifies it to the bottom or right.

# Rules of Good GReact

Now that you've got the structure of GReact down, there are some very important rules to understand for any code that interacts with GReact. Breaking these rules breaks the GReact paradigm, and will tend to cause things to not function properly.

## 1. Components must not read from the scene graph

The GReact paradigm means that components are given all the data they need to create their portion of the scene graph from scratch, and should always pretend they're doing that. And if you're always making an entirely new scene graph from scratch, why would you look at the old one?

Note that node components do sometimes need to read some data off of the `Node` they manage. They should only ever read data off the `Node` they directly manage, only for the sake of deciding which fields to update, and only in cases where that `Node` might change in a way outside of their control. As an example, a `LineEdit` control can be directly altered by the user's keyboard input, so the `LineEditComponent` needs to check the values on the `Node` itself when deciding what to update.

## 2. Nodes managed by node components must not be modified by anything else

If a node component is managing a `Node`, it needs to know it has total and sole control over that `Node`. The component is given some properties that determine the state of the `Node` it manages, and when it's done, the `Node` needs to entirely match what those properties indicate. But, it's comparing against the properties from the last frame to avoid changing things it doesn't need to. If something else has been messing with the `Node` in the meantime, some of those values may no longer be what's indicated by the old properties, and the component may not overwrite things it needs to.

Note that the exception on the first rule is specifically about handling cases where something outside GReact modifies a `Node` being managed by a node component, and should help illustrate why this causes problems. It means the node component needs to be aware of all such cases and handle those properties in a special way which further breaks the normal UReact paradigm, and failure to do that specialized handling invariably creates bugs. So it's definitely preferable to avoid creating those situations in the first place.
