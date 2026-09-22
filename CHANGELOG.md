# 2.1.10 - Azu Extended Player Inventory & Status Effect Fixes
* **Azu Extended Player Inventory & Cape Status Fix**:
  * Fixed an issue when using extended inventory mods (such as AzuExtendedPlayerInventory) where swapping or unequipping cloaks and capes (like the Feather Cape) caused their effects to get stuck on your character.
  * Cloak and gear effects now clear properly when taking off or changing equipment while wearing a backpack.
* **Cold & Warmth Message Loop Fix**:
  * Fixed an issue where "You feel cold" and "You are getting warmer" messages could loop rapidly and freeze permanently on the screen.
  * Cold immunity now prevents cold status messages from repeating when moving through chilly weather or entering shelters.
* **Status Effect Monitoring**:
  * Improved how backpack powers are tracked so they work cleanly alongside other equipment and third-party mods without interfering with your gear.

<details>
<summary><b>2.0 Changelog History (Valheim Release)</b> (<i>click to expand</i>)</summary>

### 2.1.9 - Umbrella Water Resistance & Weather Fixes
* **Water Resistance & Umbrella Mechanic**:
  * Reworked water resistance so standing in the rain keeps you dry and prevents the Wet effect from reappearing or resetting.
  * Swimming or wading in deep water will still make you wet.
  * When leaving the water into the rain, your existing Wet timer will now continue to count down naturally rather than being reset by the rain.
* **Cold Resistance & Weather Mod Compatibility**:
  * Improved Cold Resistance to prevent cold effects when playing with weather mods like Seasonality during winter conditions.
* **Weather & Fireplace Warmth**:
  * Restored natural game behavior around campfires and sheltered buildings in rainy conditions.
* **Stability & Server Protections**:
  * Added safety checks during game startup and server loading to prevent errors when loading swamp backpacks and water resistance effects.
  * Fixed an issue where dedicated servers could fail to load backpacks due to waiting on user interface events.
  * Updated shared libraries to fix locked settings and improve server configuration synchronization.

### 2.1.8 - Ecosystem Compatibility & Documentation Update
* **Mod Ecosystem Documentation & Verification**:
  * Updated and expanded the verified mod compatibility guide to reflect community standards.
  * Added explicit compatibility entries for AzuExtendedPlayerInventory, Extra Slots, and Equipment and Quick Slots (EAQS).
  * Removed references to outdated legacy mods.
* **ContentsWithin Integration**:
  * Fixed an issue where hovering over a player's equipped backpack could display container contents when using the ContentsWithin mod.
* **Cape Appearance with Extended Inventory**:
  * Fixed an issue where wearing a cape alongside a backpack caused the cape to turn invisible when using the AzuExtendedPlayerInventory mod (thanks @BaalEvan!).
* **Dedicated Server Hardening & Stability**:
  * Added startup safeguards to ensure client-only features (such as menus, quick item transfers, and local interactions) are completely skipped on dedicated servers.
* **Dependency Updates**:
  * Updated shared libraries and Jotunn dependencies to their latest stable releases.

### 2.1.7 - Storage & Inventory Mod Compatibility
* **Storage & Chest Mod Compatibility**:
  * Fixed an issue where opening a backpack while using certain storage mods (such as MidgardPlus) could cause an error and stop the container from working properly.
  * Isolated the backpack window system so external chest and sorting mods no longer conflict with player network data.
* **AzuExtendedPlayerInventory Compatibility**:
  * Fixed a startup conflict that caused game errors when Adventure Backpacks and AzuExtendedPlayerInventory were installed together.
* **Crafting & Resource Safety**:
  * Improved crafting from backpacks to prevent errors when rapidly crafting items or consuming materials from nearly empty item stacks.
* **General Stability**:
  * Added safeguards when reading backpack settings and biome powers to prevent unexpected errors.

### 2.1.6 - Bug Fixes & Slot Mod Compatibility
> **Author's Note**:
> A huge thank you to **shudnal** for the wonderful support and integration work provided for Adventure Backpacks! I apologize for inadvertently changing method signatures on you in recent updates. Moving forward, backwards compatibility for mods interfacing with Adventure Backpacks is a top priority.

* **shudnal's Extra Slots Compatibility & Death Safeguard**:
  * Fixed an issue where dying while wearing a backpack alongside **shudnal's ExtraSlotsCustomSlots** (Extra Slots) caused the game to hang on a black screen without dropping items or creating a tombstone.
  * Restored full compatibility with equipment and slot extension mods when unequipping backpacks.
* **Storage & Crafting Mod Safeguards**:
  * Added safeguards when opening and closing backpacks alongside chest and crafting mods to prevent crashes and errors in the background.

### 2.1.5 - Multi-Player Isolation & Crafting Mod Compatibility
> **Author's Note**:
> I apologize for the rush updates that occurred today which ended up in 2.1.4 having a number of issues in multi-player as well as various crafting mods. This has been resolved and tested. As always, please let me know if you encounter issues.

* **AzuCraftyBoxes & Crafting Mod Support**:
  * Added seamless integration with AzuCraftyBoxes so crafting stations properly show and consume crafting materials directly from your equipped backpack.
  * Building pieces with the hammer and bulk-crafting now recognize backpack resources smoothly.
* **Developer API Enhancements**:
  * Added new methods to the public mod API allowing other mod developers to easily access equipped backpack storage.
* **Dedicated Server & Multiplayer Improvements**:
  * Fixed an issue that could cause errors on dedicated servers when players joined or equipped backpacks.
  * Backpack inventory windows are now strictly isolated to your own character on your own screen, preventing multiplayer conflicts.
* **Stability & Compatibility Fixes**:
  * Added safeguards to prevent crashes when loading backpacks, crafting items, or opening inventory with other chest and sorting mods installed.
  * Fixed an issue where saving or loading character data could sometimes encounter an error.

### 2.1.4 - Container Proxy Compatibility & Defensive Safety
* **External Storage Mod Compatibility**:
  * Improved compatibility with chest sorting and storage mods when closing or updating backpack containers.
  * Updated the backpack storage window to display your equipped backpack's name.
* **Defensive Safety & Error Handling**:
  * Added safeguards across equipping, unequipping, crafting, and backpack window management to prevent unexpected errors.

### 2.1.3 - Auto-Store Hotfix
* Fixed an issue where auto-storing items into the equipped backpack could cause errors when interacting with external container, drawer, or sorting mods.

### 2.1.2 - Player Container Isolation & Ecosystem Compatibility
* **Player Entity Container Isolation**:
  * Removed the direct `Container` component from the root Player character object, preventing external container mods and vanilla systems from misidentifying the player as a world chest or container piece.
  * Migrated all backpack container operations to a dedicated, isolated child proxy GameObject (`AB_BackpackProxy`) that exists exclusively to back the backpack UI window.
* **Container Proxy Virtualization & Stability Fixes**:
  * Added safeguards to intercept container network routines on the local proxy, resolving an issue where opening, updating, or closing the backpack could cause errors (`NullReferenceException` in `Container.SetInUse`).
  * Ensured container actions like `Take All` and `Stack All` interact smoothly with the backpack proxy and local inventory.
  * Added lifecycle management to guarantee clean destruction and cleanup of the proxy upon unequipping gear, character death, or respawning.

### 2.1.1 - Crafting & Container Mod Compatibility
* **Compatibility with ValheimPlus, ItemDrawers & Container Mods**:
  * Fixed an issue where enabling `Enable Craft From Backpack` prevented building hammer pieces and crafting recipes from detecting materials in nearby chests.
  * Converted crafting requirement checks to cooperative hooks so vanilla game systems and other crafting/container mods evaluate uninterrupted.
* **Auto Store to Backpack & Crafting Item Fixes**:
  * Resolved an issue where empty or mod-manipulated inventory slots could cause an error when crafting or building (`NullReferenceException` in `ConsumeCraftingItem`).
  * Resolved an issue where uninitialized or modded items could cause an error during automatic storage (`NullReferenceException` in `StoreToBackpack.TryStoreItem`).
  * Ensured partial item consumption smoothly delegates remaining material requirements to external container networks.
* **Equipment & Stability Fixes**:
  * Fixed an error that could occur when unequipping gear (`NullReferenceException` in `Humanoid.UnequipItem`).
  * Added error handling during localization loading so malformed community translation files no longer prevent backpacks from initializing.

### 2.1.0 - Craft From Backpack & Auto Store To Backpack
* **New Feature: Craft From Backpack**:
  * When enabled, crafting at crafting stations and building placeables with the hammer considers materials in both the Player inventory and the currently equipped backpack.
  * When crafting or building, materials are pulled from the Player inventory first, and then the equipped backpack inventory.
  * When player inventory is full, newly crafted items can optionally be placed directly into the equipped backpack if space is available.
  * Added server-synced settings: `Enable Craft From Backpack` and `Enable Craft Output To Backpack` under `Server Config`.
* **New Feature: Auto Store To Backpack**:
  * When enabled, picked up or looted items already present in the equipped backpack (at least 1 item) automatically store into the equipped backpack instead of Player inventory (assuming room).
  * If the backpack has no room for the item, storage automatically falls back to Player inventory.
  * If Player inventory is full, items automatically continue storing into the equipped backpack without showing "Inventory Full" (configurable overflow).
  * Full compatibility with storage and container mods (AzuAutoStore, CraftFromContainers, AzuCraftyBoxes) and safeguards to prevent accidental storing when manually moving items in open backpack containers.
  * Added server-synced settings: `Enable Auto Store to Backpack` and `Enable Inventory Overflow To Backpack` under `Server Config`.

### 2.0.10 - Fix Recipe Upgrades & Backpack Status Effects
* **Fix for Backpack Status Effect Initialization**:
  * Fixed an issue where newly crafted or spawned backpacks did not immediately activate their equipment status effects (including Frost Resistance, carry weight bonus, and speed modifiers) until restarting or reloading the game.
  * Fixed an issue where the "Frost Resistance" buff icon could display in the status bar while the player still suffered from the Freezing debuff in Mountain biomes.
* **Fix for "Require Only One Resource" Configuration**:
  * Fixed an issue where enabling `Require only one resource` in the configuration caused backpack upgrade recipes to falsely indicate they were ready to craft when missing required materials.
* **Bugs Fixed from Submitted Error Reports**:
  * Fixed an issue where backpacks initialized early during startup could cause an error when game localization was not yet loaded.
  * Fixed a configuration issue where mod settings could trigger a missing field error when interacting with third-party configuration managers.

### 2.0.9 - General Bugfixes and Splash Window Updates
* **Bugs Fixed from Submitted Error Reports**:
  * 5x NullReferenceExceptions
  * 3x ArgumentNullExceptions
  * 3x TypeLoadExceptions
  * 1x AmbiguousMatchException
  * 1x JsonReaderException
  * 1x FieldAccessException
  * 1x IOException, SemanticErrorException, and MethodAccessException
* **Splash Window Updates**:
  * Telemetry is now unchecked when first loaded (Opt-In visibility)
  * Added Send Error Logs (Opt-Out)
  * Privacy Policy is now available directly in-game
  * Added Data Disclaimers on hover over checkboxes for transparency on what data is sent

### 2.0.8 - Jewelcrafting Font Compatibility
* Fixed: Jewelcrafting packages its own font which was overriding part of a vanilla font, causing the Splash screen to appear blank.

### 2.0.7 - Updated README with Telemetry Information
* Updated the README.md with Anonymous Telemetry information per request of mod stores.

### 2.0.6 - Unified Splash Screen & Telemetry Controls
* **Unified Startup Splash Screen**: Integrated with a centralized startup splash screen.
  * Added configurable `Show on Game Startup` which can be enabled or disabled in the configuration file.
* **Anonymous Telemetry**: 
  * Added configurable `Enable Anonymous Telemetry` configuration which can be enabled or disabled in the configuration file.
    * Defaults to enabled with auto-opt-in on launch. Uncheck to Opt-Out
    * ANONYMOUS DATA ONLY - I track version number and usage data. No personal data is ever collected. For more information, see the [Privacy Policy](https://vapok.io/privacy-policy/).

### 2.0.4 - Container Mod Compatibility & Item Duplication Fix
* Fixed: Item duplication and inventory reset when building or crafting with container-scanning mods (e.g. AzuCraftyBoxes, CraftFromContainers).
* Fixed: Inventory desynchronization between player container component and equipped backpack data.
* Fixed: Prevented container-saving logic from writing to the player character's network ZDO data.
* Minor stability and null-safety improvements during backpack resizing.

### 2.0.3 - Fix API DLL Size
* Fixed: `AdventureBackpacksAPI.dll` size bloat by excluding asset bundles and mod-only source files from API build configuration.

### 2.0.2 - Transpiler Resilience & Mod Compatibility Update
* Overhauled Transpilers for requirement counting and resource consumption (`Player.HaveRequirementItems`, `Player.ConsumeResources`, `InventoryGui.SetupRequirement`) to support mods injecting crafting logic (AzuCraftyBoxes, Valheim Plus, EpicLoot, AzuAutoStore).
* Improved `Humanoid.UpdateEquipmentStatusEffects` transpiler resilience against opcode variations.
* Added defensive null checks across requirement parsing and unequipped item consumption helpers.

### 2.0.1 - ZenDragon ModLib Compatibility
* Added defensive transpiler checks for compatibility with ZenDragon's MobLib.

### 2.0.0 - Valheim 1.0+ Adventure Backpacks
* Updated Adventure Backpacks for Valheim 1.0.
* Updated all Transpilers and Harmony References.
* Fixed: Drop rates now properly account for World Scaling and Level/Star creature ratings.
* Added config settings to adjust drops by Level Factor and World Scaling Factor.

</details>

