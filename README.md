# Charlib - Character Data Library

Charlib is a set of developer mods for Vintage Story, planned to be split into 2-4 parts

## Who is this for?

These mods are intended to be used by mod makers for making their own mods. Charlib by itself does not change gameplay in any way.

  > **_TODO (005):_** Add an interesting set of classes balanced for Single Player games as both an example implementation and an implementation that causal users can easily find and use.

If you're interested in making a mod that uses Charlib, see our mod developer's guide [here](docs/mod-maker-intro.md)

If you're interested in contributing to Charlib itself, see our contributor's guide [here](docs/contributor-intro.md)

## Parts of Charlib

1. CharlibUtils - A lightweight set of generic Vintage Story utilities that is not technically a mod but can be consumed and packaged by other mods

  > **_TODO (005):_** Currently this is packaged with core. This will be extracted if there's any demand at all for it as a standalone library.
  
  > **_NOTE:_** End users (when creating a server) will not need to add this library as it's not a mod
    
2. CharlibCore:
    * Is our core functionality mod that minimally modifies game code and primarily provides various mechanisms for making other mods
    * Provides access to last accessing and last modifying users for various block entities
    * Provides uniform access to arbitrary user data.
        * No need to think about how save data is done, we do that for you
        * No need to think about how the client gets the initial server data, we do that for you
        * No need to think about how to register your types or to the player entity, we do that for you
        * Confused about how the server and client communicate? We let you choose from a few different schemes, and handle that for you too.
            * Reducers + different reducer schemes provide different options that trade off performance, security and maintainability
            * Set it once and forget it. The Reducer pattern allows you to just write your mod code without having to think about how it gets applied to the server and/or client
    * Provides intelligent debugging access and support.
        * Any code patch that has an in game effect can easily provide debug override method (via in game chat), allowing for devs to more easily test that their patch has an effect.
        * Reducers provide in depth logging that devs can query (via in game chat) and determine if their reducers are being applied incorrectly.
    * Allows moders to easily add user data or in game effects.
        * Following a few simple patterns will allow moders to make their own custom effects that work in the exact same way as the ones provided by the core
        * Following our patterns will also ensure that moders will benefit from our debugging and json implementation
3. CharlibClasses
    * Is an extension mod for allowing users to create their own classes
    * Provides a number of patches to create in game effects.
    * Provides a ability template to allow mod developers to make their own player abilities.
    * Provides a skill tree template to allow mod developers to make their own skill trees with custom abilities/skills and requirements through json configuration.
    * Provides a class template to allow mod developers to make their own classes with custom abilities/skills and default class abilities through json configuration.

  > **_TODO (005):_** Currently, game effect patches are in core as part of core's development cycle (we need to make sure things are working). 

  > I'm currently undecided on whether or not to make the game effects their own mod or if I should package them with another part like Classes. 

  > My reasoning is that Core should remain as pure and non-modifying as reasonably possible so it can be used as a utility library for completely different mods. In game effects are a large change, but they, by default, don't actually change vanilla functionality, just provide hooks for other mods
