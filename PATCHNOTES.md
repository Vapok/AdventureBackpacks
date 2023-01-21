# Adventure Backpacks Patchnotes

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