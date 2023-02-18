# Adventure Backpacks Patchnotes

#1.6.5.0 - Cheb's Necromancy Compatibility and Overhauled Status Effects Management
* Adds compatibility for Cheb's Necromancy
  * Introduces a new Necromancy Backpack: Spectral Shroud of Holding
    * Applies the Necromancy Armor Status Effect
    * Applies Necromancy Skill Modifiers
* Completely Reworked Status Effects Management
  * Implements a new UpdateEquipmentStatusEffects Transpiler and removes several patch points.


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