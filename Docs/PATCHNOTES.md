# 2.0.9 - General Bugfixes and Splash Window Updates
* **Bugfixes & Null Safety Guards**:
  * Added defensive null checks in `AssetItem` across `GetItemDrop()`, `RegisterShaderSwap()`, `SetPersistence()`, and `ResetPrefabArmor()` to prevent `NullReferenceException` and `ArgumentNullException` during early asset instantiation.
  * Added null validation and defensive `try/catch` error handling in `PlayerExtensions.OpenBackpack()` when retrieving and attaching `Container` components on the player `GameObject`.
  * Guarded `PlayerPatches.AwakePostfix` with defensive `GetComponent<Container>()` checks and `try/catch` handlers.
  * Added defensive exception handling to `InventoryGuiPatches.CheckForTextInput()`.
* **Configuration & Error Reporting**:
  * Added `Send Error Reports` configuration setting (`ConfigRegistry.SendErrorReports`), defaulting to enabled (Opt-Out).
  * Set `Enable Anonymous Telemetry` default to false (Opt-In).
  * Registered `SendErrorReports` with `ModSplashManager`.
* **Vapok.Common Dependency Bump**:
  * Updated internalized dependency to `Vapok.Valheim.Common` 3.12.1014 bringing in-game scrollable Privacy Policy overlay, multi-language localization, and hover data disclaimers.

# 2.0.8 - Jewelcrafting Font Compatibility
* **Compatibility Fix**: Fixed issue where Jewelcrafting packages its own font which was overriding part of a vanilla font, causing the Splash screen to appear blank.
* **Vapok.Common Dependency Bump**: Updated internalized dependency to `Vapok.Valheim.Common` 3.11.1012.

# 2.0.7 - Updated README with Telemetry Information
* **Documentation Update**: Updated the README.md with Anonymous Telemetry and Privacy section per request of mod stores.
* **Vapok.Common Dependency Bump**: Updated internalized dependency to `Vapok.Valheim.Common` 3.9.1012.

# 2.0.6 - Unified Splash Screen & Telemetry Controls
* **ModSplashManager Integration**: Registered Adventure Backpacks with the unified Vapok startup splash screen, including tagline and localization support.
* **Anonymous Telemetry Configuration**: Added local configuration bindings (`Show Splash on Startup`, `Enable Anonymous Telemetry`) to manage opt-in anonymous startup and session reporting.
* **Vapok.Common Dependency Bump**: Updated internalized dependency to `Vapok.Valheim.Common` 3.5.1012.

# 2.0.4 - Container Mod Compatibility & Item Duplication Fix
* Fixed: Item duplication and inventory reset issue when building or crafting with container mods (e.g. AzuCraftyBoxes).
  * Resolved an inventory instance desynchronization where `FirstLoad()` and `Deserialize()` instantiated separate `Inventory` objects.
  * Synchronized the player's equipped container inventory with `_backpackInventory` across loading, deserializing, and resizing.
  * Added fallback reconciliation in `InventoryChangedPatch` to ensure modifications to the equipped container inventory save directly to the backpack's data.
  * Prevented vanilla `Container.Save()` and `Container.Load()` from overwriting or corrupting the player character's network ZDO data.
* Fixed: Missing null check when resizing backpack containers during quality and dimension validation.
* Removed redundant inventory loading calls during resize operations.

# 2.0.3 - Fix API DLL Size
* Fixed: `AdventureBackpacksAPI.dll` size bloat by excluding asset bundles and mod-only source files from API build configuration.

# 2.0.2 - Transpiler Resilience & Mod Compatibility Update
* Overhauled Transpilers for `Player.HaveRequirementItems`, `Player.ConsumeResources`, and `InventoryGui.SetupRequirement`:
  * Migrated requirement counting and resource consumption logic to direct evaluation-stack interception.
  * Dynamically resolves requirement locals from IL rather than relying on strict opcode positions or hardcoded indices.
  * Fixes transpiler patching failures and crashes when running alongside mods that inject crafting/inventory calculation logic (e.g. Valheim Plus CraftFromChest, AzuCraftyBoxes, AzuAutoStore, EpicLoot).
* Made `Humanoid.UpdateEquipmentStatusEffects` transpiler resilient to opcode variations (`Stloc_0`, `Stloc_S`, `Stloc`).
* Added defensive null checks across requirement parsing and unequipped item consumption helpers.

# 2.0.1 - ZenDragon ModLib Compatibility
* Added in defensive transpiler checks to ensure compatibility with ZenDragon's MobLib

# 2.0.0 - Valheim 1.0+ Adventure Backpacks
* Updated Adventure Backpacks for 2.0.0
  * Updated All Transpilers and Harmony References for Valheim 1.0
  * Fixed: Bug on Drops were not taking into account World Scaling and Level/Star rating of creatures.
    * Enhancements:
      * Two New Config Settings
        * Adjust Drops by Level Factor
        * Adjust Drops by World Scaling Factor
  * Tested Extensively without other mods.
    * I DO NOT KNOW IF THERE ARE COMPATIBILITY ISSUES WITH OTHER MODS BUT PLEASE LET ME KNOW <3 XOXO.
  * No New Content in this version, this is to get AB working quickly with 1.0
    * I see you Ashlands. There will be more bags, but wanted to get the initial 1.0 out to get you all rolling.
  * Please inform me of any errors or issues to my GitHub or Discord and I'll respond quickly.

# 1.9.13 - Refreshed Drop Lists & Bugfixes
* Refreshed Drop Lists for All Backpacks
  * All Biome Bosses through Plains now have a drop change by default.
  * Bears, Viles, and other newly introduced mobs have been added to drop lists.
* Fixed: InventoryGrid Index Error when opening a backpack up for the first time upon game load.
  * I am not sure if this error was caused by Adventure Backpacks or another mod compatibility issue, as it seems random when the error occurs.
  * Additionally, swaping backpacks back and forth fixes the inventory grid issue.
  * I have implemented a fix that corrects the InventoryGrid regardless.
* Fixed: Container Prefix Patch incorrectly referencing ArmorStand
* Fixed: A number of areas where possible NRE's could occur.
* Fixed: Effect Registration was happening twice.
* Optimization: Optimized Effect Toggle Handling

# 1.9.12 - External References Updated and Item Duplication Guardrails
* Still getting a few random reports about item duplication, around the yard sale function.
  * I've implemented additional checks and guardrails to see if we can prevent this.
    * This could mean that Thor might strike you down after trying to put a backpack in a backpack, and then nothing happens (and the backpack doesn't move)
    * If you are getting duplication issues, please join my Discord and let me know. Log files are helpful.
* Updating to Vapok.Valheim.Common 2.11.22112
* Updating to Jotunn 2.27.1

# 1.9.11 - Valheim Version Maintenance - Compatibility Note
* Updated to Valheim Version 0.221.12
* ZenUI/ZenModLib Compatibility
  * ZenUI removes the Vanilla InventoryGui.Show() method
  * As a result, a change has been made, that if Show() method is missing, look further up.
    * If another mod removes the vanilla Player.ShowTutorial(), this fix is cooked.

# 1.9.10 - Additional Fixes
* Fixed: Issue when backpack size x or y get set to 0, causes game crash.
* Fixed: Item loss issue when upgrading non-equipped backpacks while having a backpack equipped.
* Fixed: Character Load Failures - Given Key '500' Not Present Key
* Bug Reports That Can't Be Reproduced:
  * Reports on Inventory Clearing on Teleport
    * Tested on XPortal, Advanced Portals, Vanilla Portals (normal and stone), all work for me.
    * Tested with ExtraSlots, and a number of other mods provided, all working..
  * If you come across something that is found, please join my Discord and let me know.

# 1.9.9 - Regression Issue on Backpack Equiping and Upgrades
* Last Update caused a regression issue on backup equiping and upgrading
  * This should fix it.
* Apologies on the many updates. Trying to push out quickly to prevent these stupid issues.
  * Also hopefully this slows down the updates.

# 1.9.8 - Fixing an Item Duping Issue
* Fixed: Item Duping during Backpack Resizing
* Changes
  * Updated Config Hint Text for Multipler. Lot of confusion on this.
    * Setting to 100% basically disables the weight reduction.
      * I see what you are doing out there.
      * Setting to 0% will give you the desired effect.
* API Changes 1.1.0 -> 1.2.0
  * Updated API Dll to .NET 4.8

# 1.9.7 - Item Loss Issue - Update to Dedicated Server Strictness and Wisplight Config Changes
* It has been identified that major item data loss can occur if two clients are on a dedicated server, but only one client has Adventure Backpacks installed. The client without the mod can create dataloss, including the contents of a backpack.
  * To safeguard against this, I have enabled JVL Network Compatibility to enforce that all clients must use Adventure Backpacks.
  * This will require Adventure Backpacks, along with JVL and Yaml mods to be installed on dedicated servers.
  * Additionally all clients and servers must be using the same version as of this update.
* Reorganized Configuration Settings for Wisplight, now found under "Wisplight Client Settings" under the Effect
  * This is prep work for providing similar functionality later on for other effects.
  * This might result in resetting of your client config for these settings.
* Updates to ReadMe

# 1.9.6 - TIL Wisplights are Desired
* Added a new config setting called Wisplight Biome Logic
  * This can be disabled to allow the wisplight to be used in any biome
  * You're welcome schrodingerspsycho <3
* Adjusted Readme to highlight new settings.

# 1.9.5 - More Updates and Enhancements
* Explorers Wisppack Wisplight is Now Togglable
  * Set keybind in config, default is "L" for Light
  * Additionally, Wisplight will automatically be put away when not in Mistland's Biome's.
* Updated AssetBundles to Unity 6.0.0.0
* Revamped Managers to no longer look for ServerSync
  * This has moved all configuration now to Jotunn for managers now.
  * Note: This has reduced some of the "user friendly" fields in Configuration Manager (for now)
* The Trinket slot could be a lot of fun.

# 1.9.4 - Turkish, Upgraded Bag Upgrades, Localization, and Dependency Updates
* Added Turkish Translation thanks to Fahrim!
* Tired of Thor saving your bags?
  * Now upgrade bags with inventory without having your inventory explode.
* Completely revamped Localization System
  * Localizations are now working again.
  * Yes, they were broken. I'm sorry about that.
* Various Dependencies Updated to Current Versions

# 1.9.3 - Fixing Dedicated Server Config Syncing
* A regression issue was introduced when switching to Jotunn preventing servers from dictating configs to clients.
  * This has been resolved.
* Appropriately added the BepInDependency Flags for graceful mod exit if missing dependencies.

# 1.9.2 - Removed ServerSync - Updated to Jotunn
* 1.9.2 - Forgot to actually change Project References (good news it still worked!)
* 1.9.1 - Removed ServerSync and Updated to Jotunn for Config Management
* 1.9.0 - Updated for Valheim 0.221.4

# 1.7.10 - Updates for 0.219.16
* Resolved a Shader Compatibility issue with Blacks7ar's FeatherCollector
* Added a configuration setting to enable/disable the Shader Replacer (requires game restart)

# 1.7.9 - Additional Updates and Bug Fixes
* Updated Changes based on feedback from Pull Requests
* Fixed Bug that removed default effects from Backpacks
* Changed and updated default settings for Frost and Cold Resistances on Backpacks
  * You may need to reset your Cold Resist configs for Cold Resist to show back up on Lox Hide Backpack

# 1.7.8 - Forgot to increment the in-game version to 1.7.7
* In-game version now shows 1.7.8

# 1.7.7 - Updating for Ashland and Bog Witch Updates
* Updates to Valheim 0.219.14
* Updated ItemManager and Piece Manager

# 1.7.6 - Updating for 0.217.38 Valheim
* Fixed error on load.

# 1.7.5 - More Compatibility Support
* Added in Protection Override so that backpacks can be placed on an Item Stand, similar to Armor Stands.
* Added initial ContentsWithin support to prevent backpacks from being previewed. 

# 1.7.4 - One more Drop Bug
* Fixed: Drops were being enabled on world start.

# 1.7.3 - Fixed Configuration Issue
* Fixed the ItemManager issue and now running on new version.
* Drops tested and working as expected.

# 1.7.2 - Fixing Drop Issues
* Reverting to a previous version of ItemManager until I can understand why drops are not working fully.
  * If you having continued issues with drops after updating to this update, you might have reset your Adventure Backpacks configuration file.

# 1.7.1 - Release of the Adventure Backpacks API - Effect and Backpack Creation
* API (v1.1) now supports bringing in custom effects and backpacks
  * [Documentation](https://github.com/Vapok/AdventureBackpacks/blob/main/Docs/AdventureBackpacksAPI.md)
  * [Download ABAPI.DLL from GitHub](https://github.com/Vapok/AdventureBackpacks/releases)
* Cheb's Necromany Backpack Integration converted to API
* Updated to 0.218.28 Valheim
* Upgraded to .net 4.7.2

# 1.7.0 - Release of the Adventure Backpacks API
* Initial Release of the ABAPI.
  * [Documentation](https://github.com/Vapok/AdventureBackpacks/blob/main/Docs/AdventureBackpacksAPI.md)
  * [Download ABAPI.DLL from GitHub](https://github.com/Vapok/AdventureBackpacks/releases)
* Valheim Update 0.217.27
* Upgraded to .net 4.7.2

# 1.6.29 - Multiplayer Issue
* Fixing ServerSync

# 1.6.28 - 0.217.24 Update
* Updates for Valheim 0.217.24

# 1.6.27 - Bug Fix
* Fixed: In a rare case, when a bag is upgraded, and then you die before unequipping your bag, it can potentially lose the contents of the backpack.
  * This is fixed in this version.

# 1.6.26 - Hotfix to address Critical Crashing Bug
* Turns out adding a container on Players, make them interactable which crashes the player object.
  * Disabled the interaction function on containers when the container is Player(Clone)

# 1.6.25 - Container Checking for Mod Compatibility
* Made changes will allow the Container to be recognized as "player built"
  * This is needed to prevent other mods from having to make specific updates for this mod.
  * AzuCraftyBoxes is now fully supported with Adventure Backpacks when using the Equipped Backpack
  * Craft From Containers needs 1 update in order to work (They need to not check for Piece component)
* Fixes a Compatibility issue with Valheim+ Where Transpilers were fighting for attention.

# 1.6.24 - Updates and Compatibilities
* Fixing a Player Load error on Startup when wearing a backpack.

# 1.6.23 - Updates and Compatibilities
* Fixed the Inventory Input Control that was broken.
* Added in logging and warning messages when Transpilers don't patch.
* Added in Container compatibility for mods (like AzuCraftyBoxes) that would access containers. (GitHub Issue #110)
  * This places a Container component on the Player(Clone) that will always contain the inventory of the EQUIPPED backpack
  * This would allow Craft from Containers (assuming no code changes needed on other mods) to access that inventory.
* Added Configuration to Show/Hide the Backpack Status Effect (GitHub Issue #104)
* Added Configuration to give Backpack Status Effect a Custom Name (GitHub Issue #104)

# 1.6.22 - Hildir's Bug Fixing
* Fixes Item Requirement Count Method - Missed 1 method.

# 1.6.21 - Hildir's Bug Fixing
* Fixes Item Requirement Count Method
* Updated Russian Translations

# 1.6.20 - Valheim 0.217.14 Update
* Implements updates needed for Valheim Update Hildir's Request

# 1.6.19 - Valheim 0.216.9 Update
* Implements updates needed for Valheim Update 0.216.9
* Adds in Polish Translations (big thanks to Gryfu and rysson for the collaboration and pull requests)

# 1.6.18 - Cheb's Necromancy Hotfix
* Fixes Cheb's Necromancy Asset Issue
* Put in strong error handling for when this will happen again in the future.

# 1.6.17 - MaxAxe Compatibility Hotfix
* Changes to crafting were assuming unstackable items.
  * Fixed to allow stackable items (that can be equipped) to be removed correctly.

# 1.6.16 - Various Updates
* Crafting bags will no longer allow you to consume equipped cape's.
  * To craft a bag with a cape, and the only one in inventory is equipped, it must be unequipped in order for it to be used.
* Scaled down size and repositioned the Explorer's Wisppack.
* Updated Adventure Backpacks Unity version to 2020.3.45
* Updates codebase to 0.214.300 Valheim References
* Adding CHANGELOG.md to Thunderstore package

# 1.6.15.0 - Controller Support!!!, also some bug fixes.
* Fully Implemented Controller/Gamepad Support.
  * Set bindings in configuration for opening up the backpack and other settings.
* Rebuilt the mechanism for calculating backpack weight. Now uses transpiler.
* Addition Sign and Tame Rename Interaction Fixes
* Open Backpack With Hover now fully works on extended inventory and quick slot grids.
* Open with Inventory now work with Open/Close Backpack with Hover.
