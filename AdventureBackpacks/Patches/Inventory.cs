using System.Collections.Generic;
using AdventureBackpacks.Assets;
using AdventureBackpacks.Components;
using HarmonyLib;
using Vapok.Common.Managers;

namespace AdventureBackpacks.Patches;


public static class InventoryPatches
{
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.Changed))]
    static class InventoryChangedPatch
    {
        // Saving the backpack every time it's changed is marginally more expensive than the alternative, but it's safer and a lot tidier.
        // The alternative would be to patch every method involved in moving the backpack out of the inventory, which includes dropitem, 4 overloaded moveinventorytothis methods, and more.
        // When you drop an item, you remove the original instance and drop a cloned instance. A solution to this is to serialize the Inventory instance into the ItemData m_crafterName before it's moved.
        static void Postfix(Inventory __instance)
        {
            if (__instance == null)
                return;
            
            // If the inventory changed belongs to a backpack...
            if (__instance.m_name == Backpacks.BackpacksInventoryName)
            {
                var backpack = Backpacks.GetEquippedBackpack();
                
                if (backpack != null) 
                    backpack.Data().GetOrCreate<BackpackComponent>().Save();
            }
        }
    }
    
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.UpdateTotalWeight))]
    static class UpdateTotalWeightPatch
    {
        static void Prefix(Inventory __instance)
        {
            if (__instance == null)
                return;

            // If the current Inventory instance belongs to a backpack...
            if (__instance.GetName() == Backpacks.BackpacksInventoryName)
            {
                // Get a list of all items in the backpack.
                List<ItemDrop.ItemData> items = __instance.GetAllItems();
                var player = Player.m_localPlayer;

                // Go through all the items, match them for any of the names in backpackTypes.
                foreach (ItemDrop.ItemData item in items)
                {
                    // If the item is a backpack...
                    if (Backpacks.BackpackTypes.Contains(item.m_shared.m_name))
                    {
                        // Chuck it out!
                        AdventureBackpacks.Log.Message("You can't put a backpack inside a backpack, silly!");
                        Backpacks.EjectBackpack(item, player, __instance);

                        // There is only ever one backpack in the backpack inventory, so we don't need to continue the loop once we've chucked it out.
                        // Besides, you'll get a "InvalidOperationExecution: Collection was modified; enumeration operation may not execute" error if you don't break the loop here :p
                        break;
                    }
                }
            }

        }

        static void Postfix(Inventory __instance)
        {
            if (__instance == null || Player.m_localPlayer == null)
                return;
            
            var player = Player.m_localPlayer;
            
            if (__instance.GetName() == Backpacks.BackpacksInventoryName)
            {
                // When the equipped backpack inventory total weight is updated, the player inventory total weight should also be updated.
                if (player)
                {
                    player.GetInventory().UpdateTotalWeight();
                }
            }
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.IsTeleportable))]
        static class IsTeleportablePatch
        {
            static void Postfix(Inventory __instance, ref bool __result)
            {
                if (__instance == null || Player.m_localPlayer == null)
                    return;

                // If the inventory being checked for teleportability is the Player's inventory, see whether it contains any backpacks, and then check the backpack inventories for teleportability too
                if (__instance == Player.m_localPlayer.GetInventory())
                {
                    // Get a list of all items on the player.
                    List<ItemDrop.ItemData> items = __instance.GetAllItems();

                    // Go through all the items, match them for any of the names in backpackTypes.
                    // For each match found, check if the Inventory of that backpack is teleportable.
                    foreach (ItemDrop.ItemData item in items)
                    {
                        if (item == null)
                            continue;
                        
                        if (Backpacks.BackpackTypes.Contains(item.m_shared.m_name))
                        {
                            if (!item.Data().GetOrCreate<BackpackComponent>().GetInventory().IsTeleportable())
                            {
                                // A backpack's inventory inside player inventory was not teleportable.
                                __result = false;
                                return;
                            }
                        }
                    }
                }
                // We don't need to search for backpacks inside backpacks, because those are immediately chucked out when you try to put them in anyway.
            }
        }
    }
}