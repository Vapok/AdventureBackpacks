# 2.0.4 - Container Mod Compatibility & Item Duplication Fix
* Fixed: Item duplication and inventory reset when building or crafting with container-scanning mods (e.g. AzuCraftyBoxes, CraftFromContainers).
* Fixed: Inventory desynchronization between player container component and equipped backpack data.
* Fixed: Prevented container-saving logic from writing to the player character's network ZDO data.
* Minor stability and null-safety improvements during backpack resizing.

<details>
<summary><b>2.0 Changelog History (Valheim Release)</b> (<i>click to expand</i>)</summary>

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

<details>
<summary><b>1.0 Changelog History (Valheim Early Access)</b> (<i>click to expand</i>)</summary>

### 1.9.13 - Refreshed Drop Lists & Bugfixes
* Refreshed Drop Lists for all backpacks with new biomes and creatures.
* Fixed first-load InventoryGrid indexing issues.
* Fixed Container prefix patch referencing ArmorStand.
* Optimized effect registration and toggle handling.

### 1.9.12 - External References Updated and Item Duplication Guardrails
* Added guardrails and validation checks against item duplication.
* Updated Vapok.Valheim.Common and Jotunn references.

### 1.9.11 - Valheim Version Maintenance
* Updated for Valheim 0.221.12.
* Added ZenUI / ZenModLib compatibility handling.

### 1.9.10 - Additional Fixes
* Fixed crash when backpack size is configured to 0.
* Fixed item loss when upgrading unequipped backpacks.
* Fixed character load failures.

### 1.9.9 - Regression Fixes
* Fixed backpack equipping and upgrading regressions.

### 1.9.8 - Backpack Resizing Fix
* Fixed item duplication during backpack resizing.
* Clarified configuration hints for weight reduction multiplier.
* Updated API DLL to .NET 4.8.

### 1.9.7 - Dedicated Server Strictness & Wisplight Config
* Enabled network compatibility enforcement on dedicated servers.
* Reorganized Wisplight configuration settings.

### 1.9.6 - Wisplight Biome Logic
* Added config setting to allow Wisplight usage across all biomes.

### 1.9.5 - Explorer's Wisppack & Asset Updates
* Wisplight is now togglable via hotkey (default 'L').
* Updated AssetBundles to Unity 6.
* Migrated configuration to Jotunn.

### 1.9.4 - Translations & Upgrades
* Added Turkish translation.
* Improved backpack upgrading with inventory contents.
* Overhauled localization system.

### 1.9.3 - Dedicated Server Config Syncing
* Fixed server-to-client configuration syncing via Jotunn.

### 1.9.2 - Dependency Cleanup
* Removed ServerSync and finalized Jotunn config management.

### 1.9.0 - Valheim Update
* Updated for Valheim 0.221.4.

</details>
