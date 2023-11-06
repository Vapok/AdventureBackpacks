# 1.7.5 - More Compatibility Support
* Added in Protection Override so that backpacks can be placed on an Item Stand, similar to Armor Stands.
* Added initial ContentsWithin support to prevent backpacks from being previewed. 

<details>
<summary><b>Changelog History</b> (<i>click to expand</i>)</summary>

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

# 1.6.14.0 - Fixing Equippable Items, Adding Hover Over Interaction option, Fixing Signs.
* New Feature: Open Inventory with Hover Interaction
  * When enabled, this will override the Open with Inventory and Close Inventory Options.
  * When enabled, hovering over an equipped backpack item in the player inventory and pressing the Backpack Open Hot Key will open equipped backpack.
* Discovered a bug (or unintended interaction) that prevented the ability to equip armor if a backpack is configured to open with inventory (and close with inventory) when using "Right Click Quick Transfer" functionality
  * This has been fixed. Right Click Quick Transfer will now detect if there is armor in the spot it would be equipped at.
    * If no item is equipped in it's intended slot, it will equip the item instead of transferring it.
    * if item slot is already filled, it will quick transfer the equipable item.
    * This does mean, if your intent is to swap out armor, you'll have to unequip the current item equipped manually.
      * This is not an issue if Right Click Quick Transfer is disabled.
* When typing in Signs, inventory was opening with a hotkey.
  * Fixed to prevent inventory from opening while typing in signs.

# 1.6.13.0 - Refactor of Backpack Interaction Controls
* Enhanced and improved the mechanics behind how backpacks open.
* Fixed the backpack not closing when open.
* All Configuration settings for backpack controls have been validated.
  * **Please double check your settings if interactions have changed for you**
* Community has verified controller support from previous version.
* Fixed an issue with bags falling through the floor when dropped with Outward Mode

# 1.6.12.0 - Redefining Keymappings and Open with Inventory Option
* Adjusted Config Keymappings to allow for Gamepad, Mouse, or Keyboard to be set.
  * Untested Controller Support - I'm hoping this works, but it might not. Please provide feedback.
* Added an additional configuration option to Open Backpack Inventory when Player Inventory is Opened.
  * This defaults to Disabled. Set to Enabled (true) to open the backpack at the same time as inventory.

# 1.6.11.0 - Clean Up and Upgrade of BepInEx
* Updates to BepInEx 5.4.21
* Various Clean Up

# 1.6.10.0 - Valheim 0.214.2 Update
* Updates to Valheim 0.214.2
* Adds in Ukranian Translation (Thanks to @Stadde1n for stopping by the Discord!)

# 1.6.9.0 - Bugfixes and Chinese Translation
* Onward Mode/Quick Drop Bug
  * Fixed an issue with quick drop mod when bag inventory is full, while using Extended Inventory mods
* Added Chinese Simplified Translations
  * Thanks to DuDaowl for dropping me a line on Discord!

# 1.6.8.0 - Adjusting Container.TakeAll and Adding Chinese Traditional Translation
* When running some Extended Inventory Mod's, interacting with the Tombstone causes odd behavior.
  * Fixed: Items dupe and "teleport" to other players.
* Adding Chinese Traditional Translation
  * Thanks to 全家就是你家 for dropping by the Discord to submit it!

# 1.6.7.0 - Hotfix - Holy Status Effects Batman!
* Bug was identified that all status effects applied to Player were being replicated to all creatures spawned in.
  * While this is quite magical for the Wisplight effect, in giving every creature in the Mistlands Demister, (which in and of itself is pretty spectacular to see), this is indeed a bug that needs to be patched quickly.

# 1.6.6.0 - Hotfix - Sneaky Bug Is Sneaky
* The Troll Armor status effect was being applied even without wearing the full set when a backpack was equipped with that effect.
  * This ensures that the full troll armor set is worn.

# 1.6.5.0 - Cheb's Necromancy Compatibility and Overhauled Status Effects Management
* Adds compatibility for Cheb's Necromancy
  * Introduces a new Necromancy Backpack: Spectral Shroud of Holding
    * Applies the Necromancy Armor Status Effect
    * Applies Necromancy Skill Modifiers
    * Adds New Necromancy Option to Backpack Biome selection.
      * This is used to apply the Necromancy Armor effect to just the Necromancy Backpack
* Completely Reworked Status Effects Management
  * Implements a new UpdateEquipmentStatusEffects Transpiler and removes several patch points.
* Multiple biomes can now be selected on backpacks, and applied effects will stack.
  * If Backpack has more than 1 biome configured, and the biomes utilize the same effect, the backpack must meet all requirements for the effect to trigger.
* Adds Translation for Japanese and Portuguese Brazilian
  * Big thanks to Xutz and RedeyeBear for dropping by the Discord to contribute these!
* Bugfixes: 
  * Drops Enabled/Disable now completely work and will definitely disable drops.
    * Apologies that this took so long for me to track down.
  * When Backpack with items is placed on an Armor Stand, Thor accidentally duplicates items.
    * Fixes GitHub #67



# 1.6.4.0 - Bug Fixes + Jewelcrafting Compatibility + Spanish Language Translation
* Adds Spanish Language Translation File
  * Thanks to Esdac (on Discord) and lopezp9492 (GitHub) for contributions.
* Fixes Take All Deletion of Backpacks when Targeted Container is Full
* Fixes Gravestone Take All Duplications (same issue as above, but with Thor watching)
* Jewelcrafting consistently removes Status Effects repeatedly (whether it needs to or not), causing Backpack Wisplight to not function. 
  * I have added in support to both prevent the removal, and optimize status effect identification. 
* Configuration File is generated with split section names accidentally.  Now section names in the config file itself should all be in English, and Configuration Manager should show localized Section Names.
  * May or may not have an overall effect on random drops occuring.
  * I have tested Config setting Drops Enabled/Disabled extensively and it is absolutely disabling drops if not enabled.

# 1.6.3.0 - Key/Door Detection - Performance Tuning
* Keys stored in **Equipped Backpack** are detected for purposes of entering through doors. (e.g. Swamp Key opens Crypts).
  * Keys stored in backpacks NOT equipped, will be hidden from detection.
* Improved Backpack Protection Guards and Mod Compatibility
  * Ensuring that mods are handling your backpacks appropriately to prevent item loss and/or item dupping.
* Includes updated translations for Czech and Korean, as well as adds support for Swedish translation
* Changed Default Size of Level 1 Explorer's Wisppack from 4x4 (16 Slot Inventory) to a 8x2 (16 Slot Inventory)
* Quick Dropping (Outward Mode) now drops behind the Player running.
* Enhanced and Expanded Readme File

# 1.6.2.0 - Bug Fixes and Mod Compatibility
* The last update introduced a Language Translation issue where it stopped loading Translation files. 
  * This has been resolved. Apologies to my non-english friends!
* Adding in Language Translation Support for Czech
* Additional Mod Compatibility changes.
  * Quick Stack Store would cause Thor to empty backpacks when using the Take All / Store All commands.
    * This has been fixed.
    * This resolves an item dupping issue that is being experienced with Multi-User-Chests (not validated yet)
* This update also attempts to fix the Grid Not Displaying on Level 1 Wisppacks when opened on initial load of Valheim.

# 1.6.1.0 - Module Compatibility on Right Click Quick Transfer
* Reworked Logic to make Right Click Quick Transfer friendly to other inventory mods.
    * Adds Compatibility to **[Auto Split Stack](https://www.nexusmods.com/valheim/mods/76?tab=files&file_id=7184)**, as well as **[Quick Stack Sort](https://www.nexusmods.com/valheim/mods/2094?tab=description)**
    * May add additional compatibility to other mods not tested.

# 1.6.0.0
* Updating for Valheim Version 0.213.4
* Right Click Fast Item Transfer Fixes
  * Stacking Items when bags are full are fixed.
  * Added vanilla effects on item transfer for crisper feel.

# 1.5.9.0
* New Feature Added: Right Click Quick Transfer
    * This feature, when enabled (disabled by default), allows you to transfer contents between player inventory and containers by right clicking.
* _**blumaye.quicktransfer**_ Module Compatibility Issue Discovered which could cause loss of backpacks and items in backpacks.
    * Right Click Quick Transfer Feature meant to replace this mod.
    * Recommended to remove _**blumaye.quicktransfer**_ mod
* Added Inception checker on Inventory.AddItem()
* Added Backpack Removal Guard on Inventory.RemoveItem()
    * Backpack trashed or removed while containing items can still be deleted, but contained items will be saved by Thor.

# 1.5.8.0
* Added Frost Resistance as a Configurable Effect.
* Added Troll Armor Set as a Configurable Effect.
* Completely reworked the Effects System, introducing Factory pattern.
* Reworked Backpack Inception. The gods have looked down upon you unfavorably.

# 1.5.7.0
* We have finally identified the issue with the drop rates.  The issue stemmed from Non-English OS based players (where decimals are represented as 0,00, not 0.00) were experiencing an issue with a Culture variation issue.
    * I couldn't detect it because I only run in English mode, and so it worked for me Apologies to my non-English friends out there.
    * **_NOTE: PLEASE DELETE YOUR CONFIGURATION ON BOTH SERVER AND CLIENT WHEN UPDATING TO 1.5.7_**
* Revamped the Configuration Section to use non-localized section names in the configuration file, while still showing localized labels in the Configuration Manager UI.
* Turned off materials shader replacer, which was causing a flicker.

# 1.5.6.0
* Adding in Korean Translation thanks to hanawa07!
* When using the Configuration Manager, there was a weird interaction dealing with the Drop Chance that was making it impossible to edit the drop chance.
  * I have changed the way the drop chance is configured in Configuration Manager, and the value needs to be a fraction of a number.
  * As an example: 1 = 100%.  0.02 = 2%.

# 1.5.5.0
* Adding in German Translation thanks to Tyrone.
* Adding in Cold Resistance as a configurable Status Effect.  
* Big Shoutout to Agrivar/carpenteer for his assistance in testing reported bugs! Appreciate it!
  * Added in additional configuration options for Effects that allow effects to be more configurable to multiple bags.
  * Added in Module Compatibility to Smoothbrain's Blacksmithing (allowing quality level 5 for effects)
  * Added in Module Compatibility to Golden's TorchesAndCapes Environment changes.  Effects now work along side this mod being enabled.
* Added Drops and Drop Configurations to that Bags can be added to loot tables.
* Fixed a few minor bugs

# 1.5.4.0
* French words have characters that are not allowed in BepInEx configs that was causing major start issues. 
  * I have fixed this by sanitizing words before making configs.
* Cleaned up the way I was patching Status Effects. More Robust and won't conflict with other mods.

# 1.5.3.0
* Found another instance of Feather Fall not working. Fixed.
* Found a bug in the Epic Loot Augmenter when changing enchants.  Fixed.

# 1.5.2.0
* Fixing the Wisplight and Feathfall breaking because of previous fix. These now function as expected.
* Adding in French Translation Files (Thanks Renard!!)

# 1.5.1.0
 * Fixed an annoying bug on the Level 4 Explorer's Wisppack where the Demister would activate on every equipment change. Annoying!
 * Adjusted some shaders.
 * Adding fresh Russian Translation File thanks to BIATLONIST!  
   * (Have you translated this into your language?  Let me know, and I'll add it the releases!)

# 1.5.0.0
 * Initial Release of Adventuring Backpacks introducing 6 New Backpack Prefabs (4 New Models and Designs)
   * The original two prefabs, have been identified as legacy items, that can no longer be built.  They will live on in your inventory as "Old".  Functionally, they'll exist as they have been.  But they aren't craftable, nor are they upgradeable.  (though they are configurable).
   * The 6 new Prefabs, introduce the 6 new bags that are intended to be used as progression bags.
   * The mod author would prefer that folks adventure in the world of Valheim and stumble across what it takes to craft the assortment of bags, however, those with less adventuring desires, may look at the configuration, where I do expose all of the recipes and what you have to touch in order to gain the recipes.  Yes, this also means that all of the bags are configurable for those that pack to the beat of a different drum.
   * The 6 new backpacks are:
     * **Satchel** - _A small backpack capable of holding things._
     * **Rugged Backpack**  - _A rugged backpack, complete with buckles and fine leather straps._
     * **Bloodbag Wetpack** - _A durable backpack sealed using waterproof blood bags._
     * **Arctic Sherpa Pack** - _An arctic backpack, fit for long treks through the mountains._
     * **Lox Hide Knappsack** - _An adventuring backpack made from extremely durable lox hide._
     * **Explorers Wisppack** - _A finely crafted, mystical backpack. Complete with it's own Box of Holding. No one is quite sure how it works._
   * Greatly Expanded and 100% completely configurable settings.
   
# 1.0.4.0
 * Fixed a rare error on Piece Manager where on local play, if there is no adminlist file, it would error in the console.
 * Fixed, In the event that there are multiple language files found for the same language, LocalizationManager would fail thus failing to load the mod.
# 1.0.3.0 
 * Fixed Locking Server Config with Config Sync.  Set 'Lock Config' to True in Server config (set while server is off)
 * Various Code Refactors and reorganization of methods.
 * Added Config to allow inventory and bag to be closed with the same hotkey.
# 1.0.2.0
 * Localization Updates was very chatty.  Muted.
 * Arctic Backpack sizing was set to rugged on the X axis.  Opps.
 * Also, BepInEx version was set wrong. Reset.
# 1.0.1.0
 * Adds in Weightless Compatibility with Epic Loot to ensure maximum epicness.  (also you're cheating... lol)
 * Resolves a Bag Duplication that was occuring when trying to insert a backpack into a backpack. (The gods are watching you.)
# 1.0.0.0
 * Initial Release of Adventure Backpacks
   * This is a full refactor and completely re-writen version of JotunnBackpacks.
   * Adventure Backpacks will seamlessly convert Jotunn Backpacks into new Adventure Backpacks.
   * As such, Jotunn Backpacks is incompatible with Adventure Backpacks
     * Utilizing the same Prefab Name, to get technical.

</details>