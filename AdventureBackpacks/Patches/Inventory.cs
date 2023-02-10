using System;
using System.Collections.Generic;
using System.Linq;
using AdventureBackpacks.Assets;
using AdventureBackpacks.Components;
using AdventureBackpacks.Extensions;
using HarmonyLib;
using Vapok.Common.Managers;
namespace AdventureBackpacks.Patches;


public static class InventoryPatches
{
    private static bool _movingItemBetweenContainers;
    private static bool _droppingOutside;

    private static readonly Queue<KeyValuePair<ItemDrop.ItemData,DateTime>> ItemsAddedQueue = new();

    private static bool IsItemFromQueue(this ItemDrop.ItemData item)
    {
        if (ItemsAddedQueue.Any(x => x.Key.Equals(item)))
        {
            AdventureBackpacks.Log.Debug($"Item {item.m_shared.m_name} Found!");
            return true;
        }
        AdventureBackpacks.Log.Debug($"Item Not Found!");
        return false;
    }

    public static void ProcessItemsAddedQueue()
    {
        while (ItemsAddedQueue.Any() && DateTime.Now.Subtract(ItemsAddedQueue.Peek().Value).TotalSeconds > 0.5)
        {
            AdventureBackpacks.Log.Debug($"Process Cache Removing {ItemsAddedQueue.Peek().Key.m_shared.m_name} for Date {ItemsAddedQueue.Peek().Value} with a difference of {DateTime.Now.Subtract(ItemsAddedQueue.Peek().Value).TotalSeconds} total seconds.");
            ItemsAddedQueue.Dequeue();
        }
    }    

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.Changed))]
    static class InventoryChangedPatch
    {
        static void Postfix(Inventory __instance)
        {
            if (__instance == null || Player.m_localPlayer == null)
                return;

            // If the inventory changed belongs to a backpack...
            if (__instance.IsBackPackInventory())
            {
                var backpack = Player.m_localPlayer.GetEquippedBackpack();
                
                if (backpack != null) 
                    backpack.Save();
            }
        }
    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnDropOutside))]
    [HarmonyPriority(Priority.First)]
    static class OnDropOutsideItemPatch
    {
        static void Prefix(InventoryGui __instance)
        {
            if (__instance == null || __instance.m_dragItem == null)
                return;
            
            if (__instance.m_dragItem.IsBackpack() && __instance.m_dragItem.m_stack == 0)
                __instance.m_dragItem.m_stack = 1;

            _droppingOutside = true;
        }
        
        static void Finalizer(Exception __exception)
        {
            _droppingOutside = false;
        }
    }

    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.DropItem), new[] { typeof(Inventory), typeof(ItemDrop.ItemData), typeof(int) })]
    [HarmonyPriority(Priority.First)]
    static class HumanoidDropItemPatch
    {
        static void Prefix(ItemDrop.ItemData item)
        {
            _droppingOutside = true;
        }
        
        static void Finalizer(Exception __exception)
        {
            _droppingOutside = false;
        }
    }

    
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveItem), new[] { typeof(ItemDrop.ItemData) })]
    [HarmonyPriority(Priority.First)]
    static class RemoveItem1Patch
    {
        static void Prefix(Inventory __instance, ItemDrop.ItemData item)
        {
            if (item != null && __instance != null)
                RemoveItemPrefix(__instance, item);
        }
    }

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveItem), new[] { typeof(ItemDrop.ItemData), typeof(int) })]
    [HarmonyPriority(Priority.First)]
    static class RemoveItem2Patch
    {
        static void Prefix(Inventory __instance, ItemDrop.ItemData item)
        {
            if (item != null && __instance != null)
                RemoveItemPrefix(__instance, item);
        }
    }

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveOneItem), new[] { typeof(ItemDrop.ItemData) })]
    [HarmonyPriority(Priority.First)]
    static class RemoveItem3Patch
    {
        static void Prefix(Inventory __instance, ItemDrop.ItemData item)
        {
            if (item != null && __instance != null)
                RemoveItemPrefix(__instance, item);
        }
    }

    
    private static void RemoveItemPrefix(Inventory __instance, ItemDrop.ItemData item)
    {
        if (__instance == null || Player.m_localPlayer == null)
            return;
        
        if (_movingItemBetweenContainers || _droppingOutside)
        {
            return;
        }
        
        if (AdventureBackpacks.PerformYardSale || AdventureBackpacks.QuickDropping)
            return;
            
        if (item.TryGetBackpackItem(out var backpackItem))
        {
            AdventureBackpacks.Log.Debug($"Checking for Backpack {item.m_shared.m_name}");
            if (IsItemFromQueue(item))
            {
                AdventureBackpacks.Log.Debug($"Exiting RemoveItem for {item.m_shared.m_name}");
                return;
            }
            
            var backpack = item.Data().Get<BackpackComponent>();
            if (backpack == null)
                return;
            var inventory = backpack.GetInventory();
                
            if (inventory != null && inventory.m_inventory.Count > 0)
            {
                Backpacks.PerformYardSale(Player.m_localPlayer, item, true);
            }
        }
    }
    
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.AddItem), new[] { typeof(ItemDrop.ItemData) })]
    [HarmonyPriority(Priority.First)]
    static class AddItemPatch
    {
        static bool Prefix(Inventory __instance, ItemDrop.ItemData item, ref bool __result)
        {
            if (item == null)
                return false;

            if (item.IsBackpack())
            {
                var noInception = Backpacks.CheckForInception(__instance, item);
                if (noInception)
                {
                    if (!_movingItemBetweenContainers)
                    {
                        ItemsAddedQueue.Enqueue(new KeyValuePair<ItemDrop.ItemData, DateTime>(item,DateTime.Now));
                    }
                }
                __result = noInception;
            
                return noInception;
            }
            
            return true;
        }
    }

    [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.DropItem), new[] { typeof(Inventory), typeof(ItemDrop.ItemData), typeof(int), typeof(Vector2i) })]
    [HarmonyPriority(Priority.First)]
    static class InventoryGridDropItemPatch
    {
        static bool Prefix(InventoryGrid __instance, Inventory fromInventory, ItemDrop.ItemData item, int amount, Vector2i pos)
        {
            var itemAt = __instance.m_inventory.GetItemAt(pos.x, pos.y);
            
            if (itemAt == item)
                return true;
            if (itemAt == null || !(itemAt.m_shared.m_name != item.m_shared.m_name) && (item.m_shared.m_maxQuality <= 1 || itemAt.m_quality == item.m_quality) && itemAt.m_shared.m_maxStackSize != 1 || item.m_stack != amount)
                return true;

            if (AdventureBackpacks.PerformYardSale)
                return true;
            
            if (itemAt.IsBackpack() && fromInventory.IsBackPackInventory())
            {
                return Backpacks.CheckForInception(fromInventory, itemAt);
            }

            if (item.IsBackpack() && __instance.m_inventory.IsBackPackInventory())
            {
                return Backpacks.CheckForInception(__instance.m_inventory, item);
            }
            
            _movingItemBetweenContainers = true;

            return true;
        }
    }
    
    
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.MoveAll), new[] { typeof(Inventory) })]
    [HarmonyPriority(Priority.First)]
    static class MoveAllPatch
    {
        static void Prefix(Inventory fromInventory, Inventory __instance)
        {
            if (fromInventory == null)
                return;

            if (!__instance.IsBackPackInventory())
            {
                _movingItemBetweenContainers = true;
                return;
            }
        }
        
        static void Finalizer(Exception __exception)
        {
            _movingItemBetweenContainers = false;
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

            if (!__instance.IsBackPackInventory())
            {
                _movingItemBetweenContainers = true;
                return true;
            }
            
            return Backpacks.CheckForInception(__instance, item);
        }

        static void Finalizer(Exception __exception)
        {
            _movingItemBetweenContainers = false;
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

            if (!__instance.IsBackPackInventory())
            {
                _movingItemBetweenContainers = true;
                return true;
            }

            
            return Backpacks.CheckForInception(__instance, item);
        }

        static void Finalizer(Exception __exception)
        {
            _movingItemBetweenContainers = false;
        }
    }

    
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.UpdateTotalWeight))]
    static class UpdateTotalWeightPatch
    { static void Postfix(Inventory __instance)
        {
            if (__instance == null || Player.m_localPlayer == null)
                return;
            
            var player = Player.m_localPlayer;
            
            if (__instance.IsBackPackInventory())
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
                        
                        if (item.IsBackpack())
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