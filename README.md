# Adventure Backpacks by Vapok

This Valheim mod seeks to introduce the concept of Backpacks throughout the Valheim progression. 
Starting as a wee Viking, rummaging through the tranquil fields of the Meadows, you'll happen upon materials 
that you think will eventually lead to a more meaningful destiny.  From Deer Hide capes and beyond, you'll soon 
learn how to make your very own, Adventure Backpacks!  Go forth and wander, ye wanderer of the wanders! 

---

## How to Use Adventure Backpacks
* Play Valheim as you would As you craft items and explore materials you will learn new recipes for Adventuring Backpacks
* The default hotkey is `I` to open the equipped backpack.
* Each backpack is completely different in form, function, and size.  Upgrading backpacks will unlock additional features depending on the progresion that backpack is intended to be used with.
* Check the Configuration for ALL the different ways that you can modify these packs.
* Keybindings and Actions are Controller Supported

## How To Install Adventure Backpacks
* Install Adventure Backpacks into it's own FOLDER inside of the `BepInEx/plugins` folder.
  * Create a folder called `Translations` and ensure all Translation files are stored in there.
    * Translations files should be named `AdventureBackpacks.<language key>.json`
* Adventure Backpacks is a client-side and server-side mod.
  * If using on Dedicated Servers:
    * Configuration Lock and Sync is available and disabled by default.
      * Enabling Locked and Synced Configs will require server restart.
      * All other settings will be synced to connected clients, and server configs will be enforced.

---

## Gear Introduced In This Mod
* The 6 new backpacks are:
    * **Satchel** - _A small backpack capable of holding things._
    * **Rugged Backpack**  - _A rugged backpack, complete with buckles and fine leather straps._
    * **Bloodbag Wetpack** - _A durable backpack sealed using waterproof blood bags._
    * **Arctic Sherpa Pack** - _An arctic backpack, fit for long treks through the mountains._
    * **Lox Hide Knappsack** - _An adventuring backpack made from extremely durable lox hide._
    * **Explorers Wisppack** - _A finely crafted, mystical backpack. Complete with it's own Box of Holding. No one is quite sure how it works._

## Features of Backpacks
* Adventure Backpacks API Available
  * [Documentation](https://github.com/Vapok/AdventureBackpacks/blob/main/Docs/AdventureBackpacksAPI.md)
  * [Download ABAPI.DLL from GitHub](https://github.com/Vapok/AdventureBackpacks/releases)
    * API Features Include:
      * Registering Your Own Status Effects
      * Registering your own Backpacks (including models)
      * Getting Information about the Player worn backpack.
        * Is Item a Backpack
        * Is Backpack Equipped
        * Get Backpack Information (including Inventory on any backpack item, not just equipped)
        * Get Active Backpack Effects
      * View which effects are registered to Adventure Backpacks
* Each Backpack Biome can be fully configured for progression.
  * Configure Sizing
    * Each Quality Level of Backpack can have a different inventory grid size. Simply adjust the width and height in configuration for each quality level.
  * Configure Recipes
    * Default Recipes can be found in the configuration.
  * Configure Drops
    * Creatures and Drop Rates can be fully customized.
    * Drops are **DISABLED** by default. (as of version 1.6.3)
  * Configure Effects
    * Each Backpack Biome can be configured for any number of effects that are included in this mod.  There is nothing hardcoded about the effects.
  * Configure Carry Weight Maximum
    * Allows configuration for adjusting the additional carry weight allowed, per level of backpack.
  * Configure Speed Modification
    * Configure Speed Modification (slowness).
      * Upon each quality upgrade of backpack, speed modification is reduced (never eliminated).
  * Configure Opening of Backpack with Inventory
    * When enabled, opens backpack inventory with player inventory without additional interaction
    * Can also set Mouse, Keyboard, and Gamepad bindings.
  * Configure Opening of Backpack with Hover + Interaction
    * When enabled, will open backpack when hovered over in Player Inventory and the Open Hot Key is pressed.
    * This feature overrides Close with Inventory.
* Backpack Inventory Protection Guard
  * Every backpack inventory is specially handled by Thor himself and is monitored for any interactions that might otherwise harm the existence of items in your backpacks.
  * Backpacks in Backpacks is not allowed and the only feature that is not configurable. This is how the Allfather dreamt of it.
  * Current verified list of Compatible Inventory Mods:
    * Quick Stack Store
    * Fast Item Transfer (function is included in Backpacks)
    * Multi-User-Chest
* Backpack Monitoring System
  * Features complete support for Portal Technology to ensure no undesired items are hiding inside of backpacks in Player Inventory.
    * This feature will work with any Portal/Teleportation Mod that uses the `Inventory.IsTeleportable()` method.
      * Protip: Do not use `Humanoid.IsTeleportable()` as it won't respect backpack inventory.
      * Current List of verified Portal Compatibility:
        * Valheim Vanilla Portals
        * Advanced Portals
        * AnyPortal
        * XPortal
  * Keys stored in **Equipped Backpack** will active appropriate locked doors without having to move the key to Player inventory.
    * Swamp Key for Crypts
* Optional Right Click Quick Transfer (Fast Item Transfer)
  * Allows single right-click transfer of an item/stack of items between Player Inventory and any Open Container
  * This is the same functionality that's available as the stand-alone mod **Fast Item Transfer**
* Outward Run Away Mode
  * Pressing the Quick Drop keybind (default is `Y`), will immediately release the equipped backpack and drop it behind the player on the ground.
  * This feature is optional, and is disabled out of the box.
       

## Effects Used In This Mod
* This mod utilizes the following effects depending on backpack and quality level:
  * Carry Weight Modifications
  * Speed Modifications
  * Frost Resistance
  * Cold Resistance
  * Troll Armor Set
  * Waterproof
  * Slow Fall
  * Demister

## Currently Available Translations
* Czech / čeština
* Chinese / 简体中文
* Chinese Traditional / 繁體中文
* English
* French / Français
* German / Deutsch
* Japanese / 日本
* Korean / 한국인
* Norwegian / norsk
* Polish / Polski
* Portuguese Brazilian / Português Brasileiro
* Russian / Русский
* Spanish / Español
* Swedish / svenska
* Ukrainian / українська
* *Don't see your language, I'm looking for submissions for additional languages. Please find me on Discord (see link below) or submit a Pull Request!*

## Current Patch Notes
[Adventure Backpack Patchnotes](https://github.com/Vapok/AdventureBackpacks/blob/main/CHANGELOG.md) 

## Compatible Mods (Verified)
* Epic Loot 0.9.3+
  * Check out our Discord to get Epic Loot Patches for Dropping backpacks as Epic Loot!
* Equipment and Quickslots
* Advanced Portals
* AnyPortal
* XPortal
* Project Auga
* Quick Stack Store
* Auto Split Stack
* AzuCraftyBoxes
* Multi-User-Chests
* Fast Item Transfer
* Equipment and Quick Slots
* Jewelcrafting
* Shield Me Bruh!
* Cheb's Necromancy
  * Spectral Shroud of Holding Backpack
    * Necromancy Armor Status Effect
    * Necromancy Skill Modifier
* _There's probably a ton of others. This mod is friendly to most mods. If you see a conflict though, let me know!_

## Incompatible Mods
* JotunnBackpacks
  * This will convert bags, but safe to revert back to JotunnBackpacks.

---

### About Vapok Gaming
![Vapok Gaming](https://avatars.githubusercontent.com/u/1264136?s=180&v=4)

Author: [Vapok](https://github.com/Vapok)

Source: [Github](https://github.com/Vapok/AdventureBackpacks)

Discord: [Vapok's Mod's Community](https://discord.gg/5YAJkRFBXt)

Patch notes: [Github Patchnotes](https://github.com/Vapok/AdventureBackpacks/blob/main/CHANGELOG.md)