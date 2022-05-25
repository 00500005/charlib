This is a list of in game effect hooks

These hooks are used by skills, classes and more to create real in game effects.

By default we do not change base game behavior.

All implemented effects have a debug override value that can be set via chat command
  
**dev note:** This command will require admin privileges. Currently it is unrestricted as we're in pre-alpha. This will change on release.

```/char set [effect name] [effect value]```

# Table of Contents

> TODO: Find a sensible order for these to make it easier to find what you're looking for

## [Implemented Game Effects](#implemented-game-effects-1)
- [Firepit Cooking Time](#firepit-cooking-time)
- [Firepit Cooking Pot Stack Size](#firepit-cooking-pot-stack-size)
- [Firepit Cooking Pot Capacity (Liters)](#firepit-cooking-pot-capacity)
- [Oven Cooking Time](#oven-cooking-time)
## [Planned Game Effects](#planned-game-effects-1)
- [Firepit Burn Chance](#firepit-burn-chance)
- [Oven Burn Chance](#oven-burn-chance)

# Implemented Game Effects


## Firepit Cooking Time
This changes the amount of time it takes for any given item to cook
### Details
```csharp
{
  Id: "FirepitCookingTime",
  ValueType: float,
  ContextType: typeof(BlockAndPlayerEntity)
}
```
### Known Issues
1. The patch context could be more specific -- it currently just provides a general BlockEntity. More crucially, it does *not* provide an easy way of viewing the inventory contents (you'll have to do a significant amount of casting).
2. The client UI does not correctly render cooking time even when the value is set on client

[Back to Top](#table-of-contents)
## Firepit Cooking Pot Stack Size
Changes the amount of items that can be placed in a pot, which effects the number of servings that can be cooked at once.

**Dev Note**: Be sure to look at [FirepitCookingPotCapacityLiters](
  #firepit-cooking-pot-capacity-(liters)
) as it may be desirable to set as well.
### Details
```csharp
{
  Id: "FirepitCookingPotStackSize",
  ValueType: int,
  ContextType: typeof(BlockAndPlayerEntity)
}
```
### Known Issues
1. Shift clicking ingredients with a source stack count that is higher than all available slots results in all of the stack being consumed. For instance, a stack of 64 meat will go "into" the pot, but only 24 meat will actually be tracked, with the rest being deleted. **Dev Note:** Most likely a vanilla issue, may have been fixed in a later version.

[Back to Top](#table-of-contents)
## Firepit Cooking Pot Capacity
Changes the amount of liquid (in liters) that can be placed in a pot, which effects the number of servings that can be cooked at once.

**Dev Note:** this is distinct from item stack size in Vintage Story, but we're strongly considering making this directly tied to stack size. More specifically, we would treat the result of `FirepitCookingPotStackSize` as the intended value (in whole liters)
### Details
```csharp
{
  Id: "FirepitCookingPotCapacityLiters",
  ValueType: float,
  ContextType: typeof(BlockAndPlayerEntity)
}
```

[Back to Top](#table-of-contents)
## Oven Cooking Time
This changes the amount of time it takes for any given item to cook
### Details
```csharp
{
  Id: "OvenCookingTime",
  ValueType: float,
  ContextType: typeof(BlockAndPlayerEntity)
}
```
### Known Issues
Affected by the issues in (Firepit Cooking Time)[#firepit-cooking-time]

[Back to Top](#table-of-contents)

# Planned Game Effects

## Firepit Burn Chance
This would create a new value that would cause cooking items to produce a burnt variant or generic burnt item instead of their normal output

Alternatively, we could make a `burnt` status that affects food quality, resulting in less saturation, or perhaps mild debuffs.

**TODO:** Decide if we determine how "burnt" food is created and what parameters we could provide, or if we can delegate it as a parameter

[Back to Top](#table-of-contents)

## Oven Burn Chance
See [Firepit Burn Chance](#firepit-burn-chance)

[Back to Top](#table-of-contents)