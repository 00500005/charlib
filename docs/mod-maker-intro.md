<span id="top"></span>
Want to figure out if Charlib is right for your projects? See [Is Charlib Right for Me?](#is-charlib-right-for-me)

Want to just get started on making a charlib mod? See [Quick Start](#quick-start)

Need to find out how to do something in particular? See [API Reference](#api-reference)

# Quick Start

> **_TODO_ (005):** Make and link to a quick-start moders guide

<!-- 
TODO(005): Pick a style and pick with it
###### [Back to top](#top) 
-->
<div id="api-reference" style="
  display: flex; 
  justify-content: space-between;
  align-items: baseline;
  width: 100%;
">
  <h1>API Reference</h1>
  <a href="#top" style="
    font-weight: normal;
    font-size: 8px;
  " >Back to top</a>
</div>

> **_TODO_ (005):** Link to other relevant guides below as they're made. This list serves as a guideline for basic subjects, though is subject to change as we flesh out different parts

> We'll be starting with more advanced topics initially, as those will be used in the contributors docs for implementing core features.

## Defining Skills
### Skill Kind [Active or Passive]
### Passive Skills
#### Effects
#### HUD elements
### Active Skills
#### Activation Requirements
#### Triggers
#### Effects
#### HUD elements
## Defining Skill Trees
### Understanding Nodes
#### Node Visibility
#### Node Conditions
#### Node Parents
#### Node Effect
## Defining Resources
### Resource Kind [Ambient or Pool]
### HUD Elements


###### [Back to top](#top)
# Is Charlib Right for Me?

Do you want to create a mod that has different effects per player? 

- If so, then charlib can likely help make things easier for you.

- If not, then charlib isn't probably going to help much, but you should still talk to our devs for modding tips, and how to make your mod compatible with ours

## What Can I Do With Charlib?

So you're interested in developing your own class mod? Good for you. We can help.

For most mods, our json configuration will likely be enough for your needs. With it, you'll be able to:

  - Create custom skills that:
    - Apply one or more in game effect
    - May be active or passive
    - Active skills:
      - Are by default triggered on keybind. They will automatically be included in our keybinds and action menu and allow the user to configure it (if they have the skill)
        - This behavior is overridable and you can include other triggers, such as using a specific item, or a keybind while using an item.
      - May have usage requirements. This includes consuming custom resources such as mana, having a cooldown, or checking other player state data (ex: is the player a werewolf right now?)
  - Create skill trees that:
    - Are composed of custom skills
    - Can provide multiple levels of a given skill
    - Have purchase/ambient condition requirements per skill level
    - Can have visibility requirements per skill level
    - Can have pre-requisite skill requirements
    - Use different purchase requirements and scaling:
      - Activities can award custom resources (XP) per in game action with different configurable amounts
      - Different resource types are allowed and encouraged. For instance you might want:
        - Different XP pools for different skill trees. For example separating mining XP, forestry XP, combat XP, etc.
        - Custom rare resources for advanced skills. For instance, you could create a resource called "research points" that you only get when using an item (like a book for example).
    - Resources can be configured to be  
      - A consumable pool
        - user will benefit from skills requiring this resource
          - if all other requirements have been met
          - and the resource has been spent / applied to this skill
        - May be uncapped (default) or capped.
        - Note: this requires code to spend said resource. By default we'll add a purchase option in the skill tree UI, but this behavior can be overridden.
      - As an ambient state
        - user will automatically benefit from skills requiring this resource
          - if all other requirements have been met
          - and the requirement condition is met
        - user will automatically no longer benefit from skills requiring this resource
          - if the requirement condition is no longer met
  - Provide filter/HUD/menu configuration per skill or resource
    - Skills may provide filter affects. 
      - This is how you can create effects such as being sent to the "rust" dimension, or having a cat-eyes effect
    - You can provide HUD elements for resources
      - Want to show XP being earned? You can add a HUD element shows resource changes.
      - Want to show a resource pool like a mana bar? There's a HUD element for that too.
      - Custom textures and icons are supported for most HUD elements.
    
## When you might want to add Code

Generally you won't have to. Please ask in our Vintage Story thread or on the Discord, as we may have additional assemblies not included in core to help you out.

In addition, if we don't support your use case, we can either add support for it if it seems widely applicable enough, or help you create an extension mod or assembly. This helps future moders not have to re-invent the wheel, while keeping things small for most users.

These are a few things that would likely result in having to make an extension mod.
  - You want to make specialized custom UI elements
  - You want to make specialized custom effect triggers
  - You want to make a new in game effect type
  - You want to integrate with another mod

###### [Back to top](#top)