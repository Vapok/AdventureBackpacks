# Adventure Backpacks Patchnotes

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