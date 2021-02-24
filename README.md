# What GReact Is

GReact is a framework for creating and managing scene graphs declaratively in the Godot game engine.

### What does that mean?

Rather than explicitly creating `Node`s and managing their state and transitions, you instead make a function which essentially takes, as arguments, the current state relevant to the `Node` and its children, and returns how the `Node` and its children should look given that state.

### Why is this better?

The traditional way of building the scene graph is extremely error prone. Since each `Node` is managing its own state, and the various events that affect it could arrive with hard to predict timing, it's extremely difficult to make sure that it handles all possible cases elegantly and can't get into a bad or confused state, which results in bugs.

This system allows you to put all the game state in a centralized store, which is a definitive source of truth. There are many ways to manage this centralized state, and GReact isn't particularly opinionated about that, but the important part is that there is one source of truth for what the entire game world looks like at this moment, and GReact updates the scene graph every frame to match that source of truth.

One place where this really shines is with networking. Updates to the state store can come from local interactions, or from the network. It doesn't matter to GReact. It'll just update the scene graph to match whatever the state says, without having to concern itself with how the state got there. Only the state update code has to care about the network. A clever state system can automatically synchronize the parts of the state store that are relevant to all users.

### Wait, that sounds like it's rebuilding everything every frame. How can that be performant?

It's not actually rebuilding everything every frame. It creates the illusion that it's doing that, because it makes it easier to think about when writing code. However, in actuality, it builds up a lightweight representation of the scene graph every frame, which can be done pretty quickly. Then, it compares that to the representation that was built up the previous frame, and applies any changes to the actual scene graph. The result is that only the minimal set of changes are actually applied to the scene graph.

That said, there is still a lot of room for optimization in the process. The GReact library is very new, and it's still quite easy to make things with it that will not perform well, if you're not careful. Contributions and suggestions welcome.

### This sounds a lot like the React web framework. Even the name is similar.

It does. It's loosely based on React. It does not attempt to exactly copy the React API, as React was written for JavaScript, HTML and CSS, and GReact is written for C# and Godot's `Node`-based scene graph. The differences necessitate some changes, but it does maintain the core principles and structure of React.

That said, there's a lot of room for improvement in GReact's APIs. The library is very new, and some things could certainly be a lot better. Contributions and suggestions welcome.

### Wait, isn't React a UI framework? Why are you using it for the scene graph?

GReact is actually intended to be used for both the UI and the game scene graph. It turns out these things have a lot in common, which is why Godot actually uses the scene graph to manage the UI. That said, the GReact way of doing things is only well suited for managing the actual game scene graph for certain types of games, particularly those that do not need to make heavy use of Godot's physics. It should work well for the UI for pretty much any game.

# How to Install GReact

Copy the `GReact` directory from this repo into your project. That's it!

# How to Use GReact

See the [Overview](Docs/overview.md), which gives a high level, but fairly complete, description of how GReact works and how you should use it.

# Contributing

Pull requests welcome!

To contribute to GReact, follow the standard GitHub process: create a fork, make your changes, and then make a pull request. It will be reviewed, and some alterations may be request, but if and when it's approved, it'll be integrated.

Please follow the coding style already in use. Much of this can be enforced with the settings in omnisharp.json. If using VS Code, it's recommended to turn on the omnisharp auto-formatter.

Any substantial new functionality or redesigns should get an issue first, so it can be opened to discussion before you put in a lot of work on it. Nothing is worse than putting a ton of work into a change only to have it refused because it's contrary to the vision of the project.

Bug fixes or small backward-compatible improvements you can just submit.

# License

This is licensed under the MIT license. See the LICENSE file.

This should be plenty permissive for any reasonable use case, but if it's a problem for you, please make an issue describing why, so we can discuss alternatives.
