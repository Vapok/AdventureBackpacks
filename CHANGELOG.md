# 2.1.4 - Container Proxy Compatibility & Defensive Null Safety
* **External Storage Mod Compatibility (AzuAutoStore)**:
  * Assigned player network view reference to the backpack UI proxy container and elevated intercept priority to prevent external storage and sorting mods from encountering errors when closing or updating containers (`NullReferenceException` in `Container.SetInUse`).
  * Dynamically synchronized the UI proxy container title with the equipped backpack's localized name instead of defaulting to a generic chest name.
* **Defensive Null Safety & Error Handling**:
  * Added defensive safeguards and null checks across item equipping, unequipping, inventory UI detection, crafting consumption, and proxy management.

<details>
<summary><b>2.0 Changelog History (Valheim Release)</b> (<i>click to expand</i>)</summary>

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

