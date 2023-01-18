using System;
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
        static void Postfix(Inventory __instance)
        {
            if (__instance == null)
                return;

            // If the inventory changed belongs to a backpack...
            if (__instance.m_name == Backpacks.BackpacksInventoryName)
            {
                var backpack = Backpacks.GetEquippedBackpack();
                
                if (backpack != null) 
                    backpack.Save();
            }
        }
    }

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.MoveItemToThis), new[] {typeof(Inventory), typeof(ItemDrop.ItemData), typeof(int), typeof(int), typeof(int)})]
    [HarmonyPriority(Priority.First)]
    static class MoveItemToThisPatch
    {
        static bool Prefix(Inventory __0, ItemDrop.ItemData __1, int __2, int __3, int __4, Inventory __instance)
        {
            var fromInventory = __0;
            var item = __1;
            if (fromInventory == null || item == null)
                return false;
            
            return Backpacks.CheckForInception(__instance, item);
        }
    }

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.MoveItemToThis), new[] {typeof(Inventory), typeof(ItemDrop.ItemData)})]
    [HarmonyPriority(Priority.First)]
    static class MoveItemToThisOtherPatch
    {
        static bool Prefix(Inventory __0, ItemDrop.ItemData __1, Inventory __instance)
        {
            var fromInventory = __0;
            var item = __1;
            if (fromInventory == null || item == null)
                return false;
            
            return Backpacks.CheckForInception(__instance, item);
        }
    }

    
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.UpdateTotalWeight))]
    static class UpdateTotalWeightPatch
    { static void Postfix(Inventory __instance)
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