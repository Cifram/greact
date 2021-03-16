This tutorial will instruct you in how to build the example which exists in `Examples/UI`, which shows a set of lists, where you can add and delete lists, and add and delete items from the lists. You can reference that any time you want to see what the finished product looks like.

This tutorial assumes you already know your way around Godot and C#, and just need to learn how to use GReact.

# Your First GReact Program

Let's start with a super simple GReact program, before we get into all the complexity of our list management program. First the dispatcher:

```c#
// Dispatcher.cs
public class Dispatcher : Godot.Node {
  private GReact.Renderer renderer = new();

  public override void _Process(float delta) {
    renderer.Render(this, Component.Root());
    base._Process(delta);
  }
}
```

Then the root component:

```c#
// Root.cs
using GReact;

[Component]
public static class RootComponent {
  public struct Props { }

  public static Element New(Props props) =>
    Component.Label(
      vert: UIDim.Manual.Center(20),
      horiz: UIDim.Manual.Center(100),
      text: "Hello World!"
    );
}
```

Now just create a new, empty scene in Godot, add a root node, and attach the `Dispatcher` script to it, and hit run. You should see the "Hello World!" label centered on the screen.

Congradulations! You've made a GReact program. Now let's unpack a bit what we just did.

The starting point for any usage of GReact is generally a class called a dispatcher. This class derives from `Node` and should be placed in the scene in the Godot editor. It's generally a pretty simple class, even in more complicated programs. It just stores the relevant state and the GReact renderer, and calls that renderer and updates the state every frame on the `Ready` event. This dispatcher skips the state portion because this program doesn't need any state.

So the most important line in the Dispatcher is:

```c#
renderer.Render(this, Component.Root());
```

`Render` needs to be called every frame. The first argument passed to it is the root `Node` that the GReact tree should be parented to. Most dispatchers use `this`, but it could be any `Node`. In order to do anything, the `Renderer` needs to be given a component to render, which is the second argument.

GReact UIs are composed of components. Each component is a static class with the `[Component]` attribute on it. The class must contain a struct called `Props`, which contains all the properties the component needs, and a `public static Element New(Props props)` function. Usually, at the root of the scene graph is a component called `RootComponent`. This is what you see in `Root.cs`.

So the `New` function needs to make and return an `Element`, but what even is an `Element`?

In short, an `Element` is a lightweight representation of a `Godot.Node` and it's children. Your component is thus describing, with this `Element`, what it's section of the scene graph should look like right now. Then GReact will take this `Element` and do whatever it needs to do in order to make sure the scene graph looks how you describe it.

You don't usually have to construct an `Element` directly when writing components. You will generally be calling other components that do that for you. See, components come in two varieties. Node components directly create an `Element` with no children that represent a specific `Node`, like a `ButtonComponent` is a node component that makes an `Element` representing a `Button` `Node`. Composite components do not directly create `Element`s, but rely on other components that do, and this is the type of component you'll generally be writing. Most composite components actually call numerous other components, both node components and other composite components, to build an `Element` hierarchy, and then return the `Element` at the root of that hierarchy.

Note that you never call this `New` function directly, or create an instance of the `Props` structure yourself. GReact uses a source generator to build a cleaner API on top of that. The source generator creates a new static class called `Component`, with a static function on it for every component in the codebase, both those built-in to GReact and those you create. This function takes an argument for every property in the `Props` struct, and then builds the `Props` struct and passes it to the `New` function for you. Note that if the component class ends with the word `Component`, the source generator strips that word off the final function it creates, so, for instance, the `ButtonComponent` is called with `Component.Button`, not `Component.ButtonComponent`.

Looking back to `RootComponent.New`:

```c#
public static Element New(Props props) =>
  Component.Label(
    vert: UIDim.Manual.Center(20),
    horiz: UIDim.Manual.Center(100),
    text: "Hello World!"
  );
```

This just directly calls another component, the `LabelComponent`. As the name implies, this describes a `Label`. We give it 3 properties. The `vert` and `horiz` properties are of type `UIDim`, which is a class that gives us a convenient shorthand for all the many formatting options that Godot gives us for UI controls. As you might expect, the `vert` prop represents the vertical formatting, and the `horiz` prop the horizontal formatting. The `UIDim.Manual.Center` function creates a `UIDim` that does manual formatting (i.e. using anchors and margins, not relying on a container to format the control for us), and centers the control, with the given size. So this creates control that's centered both vertically and horizontally, and has a width of 100 and height of 20. The `text` prop should be self-explanitory.

You'll notice the `Props` struct is totally empty, and seems kind of pointless here. Which is true. But in real programs, it's virtually never the case that you can make a useful component which takes no properties.

# Adding State

Now that we've established the basics of how GReact works, let's start building something a bit more complicated. The next thing we want to talk about is state.

The thing that really makes GReact great is that it separates state management from presentation. When writing components, you can pretend that you're building the entire scene graph from scratch every frame, just saying, "Given this current state, what should the scene look like?" Then GReact deals with rectifying the scene you described with the one that's already there, so you don't have to think about the actual scene graph transitions. Then separately, you can build and update the state, without worrying about how that state relates to the presentation.

By separating these concerns, you vastly simplify what you need to think about at any given time, which makes the code easier to reason about and thus reduces the odds of creating bugs.

Note that GReact isn't particularly opinionated about how you setup your state management. It should ideally be entirely separated from the Godot scene graph, and ultimately keep everything in one centralized state object that is a central source of truth, but beyond that, it's entirely up to you how you organize and update that state. The state system we'll build here is just one approach, which mostly focuses on keeping things simple. Other ideas to consider:

- Immutable state structures, as used by Redux, one of the most popular state systems used in conjunction with the original React. See [the official Redux site](https://redux.js.org/) for details.
- Having state actions, which update state, that are automatically synchronized over the network. Can be a very convenient way for synchronizing multiplayer games.
- Dual states, so there's a read-only state from the previous frame which is copied to a new state, with appropriate modifications for the current frame, and then they're swapped, so the new state because the old one, and the old one becomes the new one and is copied over.

Anyway, the program we ultimately want to create is a list management program. It's essentially a list of lists, so let's make a very simple state class to reflect that:

```c#
// State.cs
using System.Collections.Generic;

public class State {
  public Dictionary<int, List<string>> lists = new();
  public int nextId = 0;
}
```

So first, we have a dictionary of lists, indexed by `int`s. You may wonder why we didn't just use a `List<List<string>>`. The reason is that we could delete a list in the middle, and which would cause the indices of all the lists after that one to change, but we want to maintain the identity of each list when that happens. So they need an ID that doesn't change when other lists are removed.

Then, we have an `int` called `nextId`. This is one greater than the maximum ID we've assigned to a list, so the next time we make a list, we can use this ID, and then increment it.

Now, let's update the `RootComponent` to do something with this state. First, we want to add it to the `RootComponent.Props`, like so:

```c#
// Root.cs
using GReact;

public static class RootComponent {
  public struct Props {
    public State state;
  }

  public static Element New(Props props) {
    // do something cool ...
  }
}
```

Then we want to update the `New` function to actually build something out of it. We want the lists to be organized as a sequence of columns, so we should make an `HBoxContainer`, like so:

```c#
// Root.cs
using GReact;

public static class RootComponent {
  public struct Props {
    public State state;
  }

  public static Element New(Props props) =>
    Component.HBoxContainer(
      vert: UIDim.Manual.Extend(0, 0),
      horiz: UIDim.Manual.Extend(0, 0)
    );
}
```

The `UIDim.Manual.Extend` function, as a note, makes the control stretch to the full size of it's parent. The two numbers passed in are the margin to apply at the start and end.

Okay, that's a start, but that won't do anything yet. We need to give the `HBoxContainer` a child for each list. Thankfully, `Element` has a function on it called `Child` for just this purpose. So for now, let's just make a label for each list, just to get something on the screen:

```c#
// Root.cs
using System.Linq;
using GReact;

public static class RootComponent {
  public struct Props {
    public State state;
  }

  public static Element New(Props props) =>
    Component.HBoxContainer(
      vert: UIDim.Manual.Extend(0, 0),
      horiz: UIDim.Manual.Extend(0, 0)
    ).Children(
      props.state.lists.Select(list =>
        Component.Label(
          id: list.Key,
          horiz: UIDim.Container.ShrinkStart(200),
          test: $"List {list.Key}"
        )
      ).ToArray()
    );
}
```

Here we introduce the `Chilren` function. This is a variadic function, and makes every `Element` you pass to it a child of the original `Element`. Since a variadic function can also take an array, we can use LINQ functions like `Select` to build a set of children from a collection, and pass it in here. Note that `Children` also returns the original `Element`, so it can be used as a pass-through as you see here.

Note that `UIDim.Container` specifies container formatting, i.e. the options you have for formatting a control in a container, where the container ultimately controls the formatting. The `ShrinkStart(200)` function makes it so the control will shrink to either fit it's contents, or the specified minimum size (200 pixels), whichever is larger. This is equivalent to setting `HorizSizeFlags` to `0` and `MinSize.x` to `200` on the final `Node`.

The `id` prop is one that exists on every node component. It takes an integer, and uniquely identifies the `Element` among sibling `Element`s representing the same type of `Node`. This ID is how GReact can tell that a given `Element` created this frame refers to the same `Node` as a similar `Element` from the previous frame, so it knows to update the existing `Node` instead of creating a new one. If you don't provide an `id`, one will be automatically created for you, based on the order in which the `Element`s are added, and for most circumstances that's fine. However, if you have a list which may have items added or removed in the middle, then the standard IDs will associate with the wrong `Node`s for every item in the list after the one that was added or removed, so in such a case you can get much better performance if you provide your own IDs that maintain consistency.

Now, we need to update the dispatcher to instatiate the state, and pass it into the component:

```c#
// Dispatcher.cs
public class Dispatcher : Godot.Node {
  private GReact.Renderer renderer = new();
  private State state = new();

  public override void _Ready() {
    state.lists[0] = new List<string>();
    state.lists[1] = new List<string>();
    state.nextId = 2;
  }

  public override void _Process(float delta) {
    renderer.Render(this, Component.Root(state: state));
    base._Process(delta);
  }
}
```

We set some initial state in `_Ready` just so there's something to display.

# Making It Interactive

Well, that looks good, but so far there's nothing interactive. We want to be able to add and delete lists, right? Let's start by making a button that will add a list.

```c#
// Root.cs
using Godot;
using GReact;

public static class RootComponent {
  public struct Props {
    public State state;
  }

  public static Element New(Props props) =>
    Component.HBoxContainer(
      vert: UIDim.Manual.Extend(0, 0),
      horiz: UIDim.Manual.Extend(0, 0)
    ).Children(
      props.state.lists.Keys.Select(listId =>
        Component.Label(
          id: listId,
          horiz: UIDim.Container.ShrinkStart(200),
          test: $"List {listId}"
        )
      ).ToArray()
    ).Child(
      Component.Button(
        vert: UIDim.Container.ShrinkStart(),
        text: "New List",
        onPressed: Signal.New(OnAddList, props)
      )
    );

  private static void OnAddList(Node node, Props props) {
    props.state.lists[props.state.nextId] = new List<string>();
    props.state.nextId++;
  }
}
```

So first, we add an extra call to `Child` to the end, to add a button. The `Child` function is very much like `Children`, except it only takes a single argument. When you only want to add a single child, it's marginally more efficient, because calling a variadic function requires an allocation for the array. But if you're adding multiple children, calling `Child` multiple times will generally be less efficient, because the function call overhead will overwhelm the extra allocation. Also, we could theoretically use some shenanigans to add the new button to the end of the array we were already passing into `Chilren`, but it's much cleaner and easier to just add an extra call to `Child`, and likely more efficient as well.

In order to make the button do something, we assign a `Signal` to it's `onPressed` prop. `Signal` is a GReact class for packaging up a callback with some arguments, in a way which is comparable. The API could have been designed to just take a closure here, but that creates a problem: every frame, the component would create a new closure, and comparing the closure from the last frame to the one from this frame would always show them as unequal, even when it's exactly the same function taking exactly the same arguments. This means that GReact wouldn't be able to tell if the closure has changed, and thus would need to disconnect the old Godot signal, and connect the new one, every single frame, which would be really inefficient.

The `Signal` class, though, allows for callbacks that are comparable. The function passed to `Signal.New` must not be a closure, and in fact it will throw an exception if you give it a closure, for the same reason we don't just use bare closures. It wouldn't be comparable. What you should use is a static function, as shown here. That function will take two arguments: the first is the `Node`, and the second is whatever type was passed to the second argument of `Signal.New`. The second argument should also be something that is meaningfully comparable. Generally a primitive value, a struct or a tuple. It's common to do what we did here, and just pass on the component's props struct.

Note that you should avoid using the `Node` argument as much as you can. It's there to give you an escape hatch for the rare, special cases where the GReact paradigm just doesn't give you the access you need to do something. So sometimes it's necessary, and we'll cover such a case near the end of this tutorial. But most of the time, it's best to just pretend that argument doesn't exist.

Finally, let's update `Dispatcher` to remove the initialization it does of the state, since we can add lists interactively now:

```c#
// Dispatcher.cs
public class Dispatcher : Godot.Node {
  private GReact.Renderer renderer = new();
  private State state = new();

  public override void _Process(float delta) {
    renderer.Render(this, Component.Root(state: state));
    base._Process(delta);
  }
}
```

Now if you run this, you should see a "New List" button to the right of all the lists, and when you click on that it'll add a new list!

# Components Calling Components

Each list, though, is still just a label. In order to make it a proper list, we could keep making `RootComponent` more and more complicated, but there comes a time when it's cleaner to just make a new component to handle a particularly complicated bit. So let's add a `ListComponent`:

```c#
// List.cs
using System.Collections.Generic;
using System.Linq;
using GReact;

public static class ListComponent {
  public struct Props {
    public int id;
    public List<string> list;
  }

  public static Element New(Props props) =>
    Component.VBoxContainer(
      id: props.id,
      horiz: UIDim.Container.ShrinkStart(200)
    ).Children(
      props.list.Select(name =>
        Component.Label(
          vert: UIDim.Container.Fill(),
          horiz: UIDim.Container.Fill(),
          text: name
        )
      ).ToArray()
    );
}
```

So first, in the props, we pass in the ID, and the list itself. Then in the `New` function, we build a `VBoxContainer`, and add a child `Label` for each element in the list.

Now we want to go back to the `RootComponent` and make it use this new `ListComponent`:

```c#
// Root.cs
using Godot;
using GReact;

public static class RootComponent {
  public struct Props {
    public State state;
  }

  public static Element New(Props props) =>
    Component.HBoxContainer(
      vert: UIDim.Manual.Extend(0, 0),
      horiz: UIDim.Manual.Extend(0, 0)
    ).Children(
      props.state.lists.Keys.Select(listId =>
        Component.List(
          id: listId,
          list: props.state.lists[listId],
        )
      ).ToArray()
    ).Child(
      Component.Button(
        vert: UIDim.Container.ShrinkStart(),
        text: "New List",
        onPressed: Signal.New(OnAddList, props)
      )
    );

  private static void OnAddList(Node node, Props props) {
    props.state.lists[props.state.nextId] = new() { "Item 1", "Item 2" };
    props.state.nextId++;
  }
}
```

We also updated the `OnAddList` put a couple items in the list, so we can see that `ListComponent` is working.

Well, that's pretty straight-forward! Calling our new composite component looks pretty much the same as calling all the node components we were already using.

# Updating State

So far, we've been updating state directly in our callbacks. But if we keep going this route, we're going to start having problems, so we're going to have to make our state a little bit smarter.

To illustrate the problem, let's think about adding a delete button to each list. It makes sense that button should be part of the `ListComponent`, right? To make that work with our current approach to state, we have two options:

- The `ListComponent` creates a `Signal` that it attaches to the delete button, which removes the list in the state. But in order to delete a list, it needs to have access to the dictionary in which the list resides, which we're not currently passing into the `ListComponent`. This means we pretty much need to pass the entire state into `ListComponent` for it to function, which seems unfortunate. The `ListComponent` shouldn't have to know that much about the larger structure or state it's interacting with.
- The `ListComponent` takes a `Signal` as one of it's props, called `onDeleteList`. Then the `RootComponent`, which has access to the the whole state, can have the callback, and just pass that into the `ListComponent`. This keeps the `ListComponent` from having to know about a lot of things it doesn't need to, but now the `RootComponent` is responsible for deleting lists, which seems a bit out of scope.

So, neither of these options are great. A better option is to update our state system, by giving it the concept of actions. An action, in this context, is a function that encapsulates a state update. The way we'll do this is by putting a set of static second order functions on `State`. They'll each take some arguments, and then return a new closure which takes a `State` and updates it. Then we'll add a non-static function to `State` which takes an `Action<State>` and calls it. We can call that function `Apply`, and that's what we'll pass into all our composite components that need to modify state.

That may sound a bit confusing, so let's see it in action. So far, we've only got one update we make to the state, adding a list, but we also want to be able to delete a list. So let's see what the state looks like with those two actions:

```c#
// State.cs
using System;
using System.Collections.Generic;

public class State {
  public Dictionary<int, List<string>> lists = new();
  public int nextId = 0;

  public void Apply(Action<State> action) {
    action(this);
  }

  public static Action<State> AddList() => state => {
    state.lists[state.nextId] = new();
    state.nextId++;
  };

  public static Action<State> RemoveList(int listId) => state => {
    state.lists.Remove(listId);
  };
}
```

So the `Apply` function is very simple: it just takes an `Action<State>` and calls it with `this`. No problem.

The `AddList` and `RemoveList` functions are also pretty simple. They each just create a new closure, which they return as an `Action<State>`. That closure takes the `State` passed to it, and updates it appropriately.

Put together, this allows you to pass the `Apply` function around, as a prop of type `Action<Action<State>>`, and then call it like `props.apply(State.AddList())` or `props.apply(State.RemoveList(id))`, without having a copy of the actual state. Now, the only state data you need access to in a given component is the state you need to read to determine how to draw the component, not the state you need to modify if the component does something interactive. For that, you call an action which handles the change, without the component having to worry itself about exactly how that change happens.

So first, let's update the `ListComponent` with a delete button that calls this new `RemoveList` action:

```c#
// List.cs
using System.Collections.Generic;
using System.Linq;
using GReact;

public static class ListComponent {
  public struct Props {
    public int id;
    public List<string> list;
    public Action<Action<State>> apply;
  }

  public static Element New(Props props) =>
    Component.VBoxContainer(
      id: props.id,
      horiz: UIDim.Container.ShrinkStart(200)
    ).Child(
      Component.HBoxContainer(
        horiz: UIDim.Container.ExpandFill()
      ).Children(
        Component.Label(
          horiz: UIDim.Container.ExpandFill(),
          text: $"List {pros.id}",
        ),
        Component.Button(text: "X", onPressed: Signal.New(OnRemoveList, props))
      )
    ).Children(
      props.list.Select(name =>
        Component.Label(
          vert: UIDim.Container.Fill(),
          horiz: UIDim.Container.Fill(),
          text: name
        )
      ).ToArray()
    );

  private static void OnRemoveList(Node node, Props props) {
    props.apply(State.RemoveList(props.id));
  }
}
```

We added the `apply` function to the props, and the `OnRemoveList` static function to provide our signal that calls the `apply` function with the `RemoveList` action. Then, we added a new call to `Child` on the outer `VBoxContainer` to add a header to the list, which includes an "X" button that deletes the list.

Next, let's update `RootComponent`:

```c#
// Root.cs
using Godot;
using GReact;

public static class RootComponent {
  public struct Props {
    public Dictionary<int, List<string>> lists;
    public Action<Action<State>> apply;
  }

  public static Element New(Props props) =>
    Component.HBoxContainer(
      vert: UIDim.Manual.Extend(0, 0),
      horiz: UIDim.Manual.Extend(0, 0)
    ).Children(
      props.lists.Keys.Select(listId =>
        Component.List(
          id: listId,
          list: props.lists[listId],
          apply: props.apply
        )
      ).ToArray()
    ).Child(
      Component.Button(
        vert: UIDim.Container.ShrinkStart(),
        text: "New List",
        onPressed: Signal.New(OnAddList, props)
      )
    );

  public static void OnAddList(Node node, Props props) {
    props.apply(State.AddList());
  }
}
```

You'll notice our `OnAddList` call just got way simpler. The `RootComponent` is no longer managing state at all, it's just triggering the state to manage itself. And the `ListContainer` is similarly now able to take action without having to explicitly manage state itself. It just needs `RootComponent` to pass on the `props.apply` function it was given.

And finally, update how we call the `RootComponent` in `Dispatcher`:

```c#
// Dispatcher.cs
public class Dispatcher : Godot.Node {
  private GReact.Renderer renderer = new();
  private State state = new();

  public override void _Process(float delta) {
    renderer.Render(this, Component.Root(lists: state.lists, apply: State.Apply));
    base._Process(delta);
  }
}
```

# Adding and Removing List Items

No new concepts will be required for this part, just expanding on what we already know. We want to be able to add and remove items from a list, so we'll need two new state actions, `AddItemToList` and `RemoveItemFromList`. Also, the individual list items will need to have their own "X" button now, which is making them complicated enough that it seems worthwhile to break them into their own `ListItemComponent`. Finally, we want to replace the list label with a "New Item" button, that adds an item to the list.

First, let's add the two new state actions:

```c#
// State.cs
using System;
using System.Collections.Generic;

public class State {
  public Dictionary<int, List<string>> lists = new();
  public int nextId = 0;

  public void Apply(Action<State> action) {
    action(this);
  }

  public static Action<State> AddList() => state => {
    state.lists[state.nextId] = new();
    state.nextId++;
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
}
```

Those are really straight forward. So now let's make the new `ListItemComponent`:

```c#
// ListItem.cs
using Godot;
using GReact;

[Component]
public static class ListItemComponent {
  public struct Props {
    public string text;
    public Signal onDelete;
  }

  public static Element New(Props props) =>
    Component.HBoxContainer(
      horiz: UIDim.Container.ExpandFill()
    ).Children(
      Component.Label(
        horiz: UIDim.Container.ExpandFill(),
        text: props.text,
      ),
      Component.Button(
        text: "X",
        onPressed: props.onDelete
      )
    );
}
```

Also pretty straight forward. The one thing that may be surprising here is that we're passing in the `onDelete` as a signal, rather than just passing in the `State.Apply` function and letting `ListItemComponent` call that itself. The reason for this is that the call to the `RemoveItemFromList` action would require both the ID of the list, and the item index to be removed, so we'd have to give the `ListItemComponent` two additional props which it doesn't actually need to render itself, just so it can delete itself. But, the containing `ListComponent` already has this information, so it can build the correct deletion signal for the list item, and just pass that on.

To be clear, it would not have been terrible practice to pass those IDs into the `ListItemComponent` and let it handle the deletion directly. There's a balancing act between passing in everything a component needs to manage it's own state, and passing in signals to keep it's props from getting too bloated, and exactly where you draw the line on the relative complexity of the two requires a judgment call. There's no hand and fast rule about it.

Anyway, let's update the `ListComponent` to have the "New Item" button, and to properly call the new `ListItemComponent`:

```c#
// List.cs
using System.Collections.Generic;
using System.Linq;
using GReact;

public static class ListComponent {
  public struct Props {
    public int id;
    public List<string> list;
    public Action<Action<State>> apply;
  }

  public static Element New(Props props) =>
    Component.VBoxContainer(
      id: props.id,
      horiz: UIDim.Container.ShrinkStart(200)
    ).Child(
      Component.HBoxContainer(
        horiz: UIDim.Container.ExpandFill()
      ).Children(
        Component.Label(
          horiz: UIDim.Container.ExpandFill(),
          text: $"List {pros.id}",
        ),
        Component.Button(text: "X", onPressed: Signal.New(OnRemoveList, props))
      )
    ).Children(
      props.list.Select(name =>
				props.list.Select((name, i) => Component.ListItem(
					text: name,
					onDelete: Signal.New(OnRemoveItem, (i, props)),
				)).ToArray()
      ).ToArray()
    );

  private static void OnRemoveList(Node node, Props props) {
    props.apply(State.RemoveList(props.id));
  }

  private static void OnRemoveItem(Node node, (int itemIndex, Props props) args) {
    args.props.apply(State.RemoveItemFromList(args.props.id, args.itemIndex));
  }

  private static void OnAddItem(Node node, Props props) {
    props.apply(State.AddItemToList(props.id));
  }
}
```

The most notable thing here is that `OnRemoveItem` is the first time we've seen a signal function that didn't just take `Props` for it's second argument. Instead, it takes a tuple of `(int, Props)`, which it immediately destructures. It's actually pretty common that a signal function needs to be given an ID of some sort to know which thing in the `Props` to operate on, so the convention for that is to package the ID in with the `Props` using a tuple, like you see here.

And that's it. We require no updates to `RootComponent` or `Dispatcher` to make this work.

# Editing List Items

Finally, to make these lists actually useful, we need to be able to edit the contents of a list item. This requires using the `LineEdit` control, and using it's change handler.

So first, let's add a new `ChangeItem` state action:

```c#
// State.cs
using System;
using System.Collections.Generic;

public class State {
  public Dictionary<int, List<string>> lists = new();
  public int nextId = 0;

  public void Apply(Action<State> action) {
    action(this);
  }

  public static Action<State> AddList() => state => {
    state.lists[state.nextId] = new();
    state.nextId++;
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
```

That's simple enough. Now let's update the `ListItemComponent` to have the `LineEdit` control with it's appropriate `onChange` handler:

```c#
// ListItem.cs
using Godot;
using GReact;

[Component]
public static class ListItemComponent {
  public struct Props {
    public string text;
    public Signal onDelete;
    public Signal<string> onChange;
  }

  public static Element New(Props props) =>
    Component.HBoxContainer(
      horiz: UIDim.Container.ExpandFill()
    ).Children(
      Component.LineEdit(
        horiz: UIDim.Container.ExpandFill(),
        text: props.text,
        onTextChanged: props.onChange,
      ),
      Component.Button(
        text: "X",
        onPressed: props.onDelete
      )
    );
}
```

This is again really simple. The only new idea here is the generic `Signal<string>` type. Some signals come with an intrinsic argument, so there is a generic version of `Signal` which accommodates that. In this case, the argument is the new text that the `LineEdit` control now contains when it's changed, as a `string`. Once again, like with `onDelete`, we don't create the signal in the `ListItemComponent`, but allow the `ListComponent` to pass it in, because that signal would require both the list ID and the item index within the list.

Let's now see how that `Signal<string>` is defined in the `ListComponent`:

```c#
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GReact;

[Component]
public static class ListComponent {
  public struct Props {
    public int id;
    public List<string> list;
    public Action<Action<State>> apply;
  }

  public static Element New(Props props) =>
    Component.VBoxContainer(
      id: props.id,
      horiz: UIDim.Container.ShrinkStart(200)
    ).Child(
      Component.HBoxContainer(
        horiz: UIDim.Container.ExpandFill()
      ).Children(
        Component.Button(
          horiz: UIDim.Container.ExpandFill(),
          text: "Add Item",
          onPressed: Signal.New(OnAddItem, props)
        ),
        Component.Button(text: "X", onPressed: Signal.New(OnRemoveList, props))
      )
    ).Children(
      props.list.Select((name, i) => Component.ListItem(
        text: name,
        onDelete: Signal.New(OnRemoveItem, (i, props)),
        onChange: Signal<string>.New(OnChangeItem, (i, props))
      )).ToArray()
    );

  private static void OnRemoveList(Node node, Props props) {
    props.apply(State.RemoveList(props.id));
  }

  private static void OnRemoveItem(Node node, (int itemIndex, Props props) args) {
    args.props.apply(State.RemoveItemFromList(args.props.id, args.itemIndex));
  }

  private static void OnAddItem(Node node, Props props) {
    props.apply(State.AddItemToList(props.id));
  }

  private static void OnChangeItem(Node node, (int itemIndex, Props props) args, string newValue) {
    args.props.apply(State.ChangeItem(args.props.id, args.itemIndex, newValue));
  }
}
```

The `onChange` signal here looks a lot like the `onDelete` signal, using the same kind of tuple. It's just that the `OnChangeItem` static function has an additional `string` argument at the end, for the new text value of the `LineEdit` field, which it passes along to the `ChangeItem` state action.

And that's it, you should now be able to edit list items.

# Maintaining Focus

There's one remaining bit of awkwardness in this. When you add a new list item, you then have to go click on it to start editing, because it's not focused. It'd be nice if new list items were automatically focused, so you could just start typing. Sadly, GReact can't handle focus directly. Godot has it's own system for managing focus, and it's got a lot of complicated rules. In order to manage focus entirely within GReact, you'd basically have to discard the Godot system and entirely re-implement it yourself, which trust me, you don't want to do.

But all hope is not lost. GReact has an escape hatch for circumstances like this. Remember that every signal function takes as it's first argument the actual Godot `Node`. We cautioned you earlier against using this argument of very specific circumstances. Those circumstances are specifically when you need to manipulate some part of the node that GReact can't manage. Focus is just such a thing.

GReact controls also include an `onReady` signal, which maps to Godot's `_Ready()`. We can use that, along with the `Node` argument, to get exactly what we want. Let's see the updated `ListItemComponent` that handles this:

```c#
using Godot;
using GReact;

[Component]
public static class ListItemComponent {
  public struct Props {
    public string text;
    public Signal onDelete;
    public Signal<string> onChange;
  }

  public static Element New(Props props) =>
    Component.HBoxContainer(
      horiz: UIDim.Container.ExpandFill()
    ).Children(
      Component.LineEdit(
        horiz: UIDim.Container.ExpandFill(),
        text: props.text,
        onTextChanged: props.onChange,
        onReady: Signal.New(OnLineEditReady)
      ),
      Component.Button(
        text: "X",
        onPressed: props.onDelete
      )
    );

  public static void OnLineEditReady(Node node) {
    if (node is LineEdit control) {
      control.GrabFocus();
      control.SelectAll();
    }
  }
}
```

Along with focus, we also call `SelectAll()` on the control, since text selection is another thing, very much like focus, that GReact can't control directly. But in order for typing immediately to work how you would want, it needs to select all the text so as soon as you start typing it replaces what's already there.

You should be able to plug this in and test it now.

# Watching Node Churn

GReact handles the creation, destruction and changing of nodes for you, behind the scenes, by looking at the element tree you built this frame, and comparing it to the one from last frame, and it's internal knowledge of which node was associated with which element from the last frame. GReact doesn't always get this right, and if it messes up, it may end up creating and destroying a lot of nodes it doesn't need to. Using the optional `id` prop on node components is the main lever you have for fixing it. That said, it's often hard to tell if GReact is getting this wrong, since it'll produce the correct scene graph each frame regardless. It just might waste a lot of effort doing it.

This is where the `trackNodeChurn` flag on `Renderer` comes in. There are two other properties on the `Renderer`, called `nodesCreated` and `nodesDestroyed`, which will be populated each time you call `Render`, but only if `trakcNodeChurn` is `true`. It's off by default, because there's some performance overhead in doing this tracking. Let's turn it on, and output the results each time it does something, to get a sense of how the node churn is working. You can do this entirely in `Dispatcher`, as so:

```c#
// Dispatcher.cs
public class Dispatcher : Godot.Node {
  private State state = new();
  private GReact.Renderer renderer = new();

  public override void _Ready() {
    renderer.trackNodeChurn = true;
    base._Ready();
  }

  public override void _Process(float delta) {
    renderer.Render(this, GReact.Component.Root(lists: state.lists, apply: state.Apply));
    if (renderer.nodesCreated != 0 || renderer.nodesDestroyed != 0) {
      Godot.GD.Print($"{renderer.nodesCreated} nodes created and {renderer.nodesDestroyed} destroyed");
    }
    base._Process(delta);
  }
}
```

So it turns on `trackNodeChurn` in the `_Ready` method, and then in the `_Process`, after it calls `Render`, it checks if there's been any churn (i.e. if `nodesCreated` or `nodesDestroyed` are non-zero), and if so, it outputs exactly how much churn.

If you run the code now, you should see that most frames it outputs nothing, because nothing changed, so the element tree was the same, and GReact knew not to add or remove any nodes. But any time you add or remove a list or a list item, it should show an appropriate amount of churn.

# Next Steps

That concludes this tutorial! You should now have all the information you need to know in order to build a UI with GReact.

As a reminder, you can find the full source code for the example you built in this tutorial at `Examples/UI/` in this project.

If you haven't yet read the [Overview](overview.md), it's highly recommended. It gives you roughly the same information as this tutorial, but in a more concise and thurough package, instead of step-by-step instructions, and thus is more useful as a continuing reference to return to when you have questions.

If while working with GReact, you find any bugs or important missing features, please go to [the GitHub page](https://github.com/Cifram/greact) and create an issue. And if you're feeling particularly helpful, you could even build a pull request yourself to fix the issue! GReact is still a very new library, so there's plenty of work to be done.