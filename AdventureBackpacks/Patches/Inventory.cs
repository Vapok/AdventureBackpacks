using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AdventureBackpacks.Assets;
using AdventureBackpacks.Components;
using AdventureBackpacks.Extensions;
using AdventureBackpacks.Features;
using HarmonyLib;
using Vapok.Common.Managers;
namespace AdventureBackpacks.Patches;


public static class InventoryPatches
{
    private static bool _movingItemBetweenContainers;
    private static bool _droppingOutside;
    public static bool IsDoingCrafting = false;
    private static BackpackComponent _savedBackpackData = null;

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

            var player = Player.m_localPlayer;
            // If the inventory changed belongs to a backpack...
            if (__instance.IsBackPackInventory())
            {
                AdventureBackpacks.Log.Debug($"#### Patch Inventory Changed: {__instance.m_name} (HashCode: {__instance.GetHashCode()}, Slots: {__instance.m_inventory.Count})");
                
                if (player.IsBackpackEquipped())
                {
                    var backpack = player.GetEquippedBackpack();
                    if (backpack != null)
                    {
                        var bpInventory = backpack.GetInventory();
                        AdventureBackpacks.Log.Debug($"########################################");
                        AdventureBackpacks.Log.Debug($"####       Inventory.Changed       #####");
                        AdventureBackpacks.Log.Debug($"Changed Inventory: {__instance.m_name} (HashCode: {__instance.GetHashCode()})");
                        AdventureBackpacks.Log.Debug($"Backpack Item: {backpack.Item?.m_shared?.m_name}");
                        AdventureBackpacks.Log.Debug($"Backpack Inventory: {bpInventory?.m_name} (HashCode: {bpInventory?.GetHashCode()})");
                        AdventureBackpacks.Log.Debug($"########################################");

                        if (bpInventory == __instance)
                        {
                            AdventureBackpacks.Log.Debug($"#### Before Save BackpackComponent Inventory Count: {bpInventory.m_inventory.Count}");
                            if (backpack.IsLoadingInventory)
                            {
                                AdventureBackpacks.Log.Debug($"Bypassing Save - Inventory Is Loading ----->");
                            }
                            else
                            {
                                backpack.Save(__instance);
                            }
                            AdventureBackpacks.Log.Debug($"#### After Save BackpackComponent Inventory Count: {bpInventory.m_inventory.Count}");
                        }
                        else
                        {
                            var backpackContainer = player.GetBackpackContainerProxy();
                            if (backpackContainer != null && backpackContainer.m_inventory == __instance)
                            {
                                backpack.SetInventory(__instance);
                            }
                            else
                            {
                                var allItems = player.GetInventory()?.GetAllItems();
                                var matchingBackpack = allItems?.FirstOrDefault(i => i.IsBackpack() && i.Data().Get<BackpackComponent>()?.GetInventory() == __instance);
                                if (matchingBackpack != null)
                                {
                                    var comp = matchingBackpack.Data().Get<BackpackComponent>();
                                    if (comp != null && !comp.IsLoadingInventory)
                                        comp.Save(__instance);
                                }
                            }
                        }
                    }
                    else
                    {
                        AdventureBackpacks.Log.Warning($"#### Player.IsBackpackEquipped() was true, but GetEquippedBackpack() returned null!");
                    }
                }
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
        
        static void Postfix()
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
        
        static void Postfix()
        {
            _droppingOutside = false;
        }
    }

    
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveItem), new[] { typeof(ItemDrop.ItemData) })]
    [HarmonyPriority(Priority.First)]
    static class RemoveItem1Patch
    {
        static bool Prefix(Inventory __instance, ItemDrop.ItemData item, ref bool __result)
        {
            if (item == null || __instance == null)
                return true;
            if (!RemoveItemPrefix(__instance, item))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveItem), new[] { typeof(ItemDrop.ItemData), typeof(int) })]
    [HarmonyPriority(Priority.First)]
    static class RemoveItem2Patch
    {
        static bool Prefix(Inventory __instance, ItemDrop.ItemData item, ref bool __result)
        {
            if (item == null || __instance == null)
                return true;
            if (!RemoveItemPrefix(__instance, item))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveOneItem), new[] { typeof(ItemDrop.ItemData) })]
    [HarmonyPriority(Priority.First)]
    static class RemoveItem3Patch
    {
        static bool Prefix(Inventory __instance, ItemDrop.ItemData item, ref bool __result)
        {
            if (item == null || __instance == null)
                return true;
            if (!RemoveItemPrefix(__instance, item))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Returns true to run the original RemoveItem; false to block (e.g. when yard sale could not empty backpack — prevents inception).
    /// </summary>
    private static bool RemoveItemPrefix(Inventory __instance, ItemDrop.ItemData item)
    {
        if (__instance == null || Player.m_localPlayer == null)
            return true;

        if (_movingItemBetweenContainers || _droppingOutside)
            return true;

        if (IsDoingCrafting)
        {
            if (item.IsBackpack())
                _savedBackpackData = item.Data().Get<BackpackComponent>();
            return true;
        }

        if (AdventureBackpacks.PerformYardSale || AdventureBackpacks.QuickDropping || AdventureBackpacks.BypassMoveProtection)
            return true;

        if (!item.TryGetBackpackItem(out _))
            return true;

        AdventureBackpacks.Log.Debug($"Checking for Backpack {item.m_shared.m_name}");
        if (IsItemFromQueue(item))
        {
            AdventureBackpacks.Log.Debug($"Exiting RemoveItem for {item.m_shared.m_name}");
            return true;
        }

        var backpack = item.Data().Get<BackpackComponent>();
        if (backpack == null)
            return true;

        var inventory = backpack.GetInventory();
        if (inventory == null || inventory.m_inventory.Count == 0)
            return true;

        // Empty backpack first so we don't allow "backpack with contents" to be moved into another backpack (inception).
        // If yard sale fails (e.g. another mod blocks drop), block the remove so backpack stays put.
        if (!Backpacks.PerformYardSale(Player.m_localPlayer, item, true))
        {
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$vapok_mod_yard_sale_blocked");
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.AddItem),
        new[]
        {
            typeof(string), typeof(int), typeof(int), typeof(int), typeof(long), typeof(string), typeof(Vector2i),
            typeof(bool) ,typeof(bool) ,typeof(bool)
        })]
    [HarmonyPriority(Priority.First)]
    static class AddItemCraftingPatch
    {
        static void Postfix(Inventory __instance, ref ItemDrop.ItemData __result)
        {
            if (IsDoingCrafting && __instance != null)
            {
                _savedBackpackData = null;
            }
        }
    }
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.CanAddItem), new[] { typeof(ItemDrop.ItemData), typeof(int) })]
    [HarmonyPriority(Priority.First)]
    static class CanAddItemPatch
    {
        private static bool _evaluatingCanAddItem;

        static bool Prefix(Inventory __instance, ItemDrop.ItemData item, int stack, ref bool __result)
        {
            if (_evaluatingCanAddItem)
                return true;

            if (item == null || Player.m_localPlayer == null || __instance != Player.m_localPlayer.GetInventory())
                return true;

            if (_movingItemBetweenContainers)
                return true;

            _evaluatingCanAddItem = true;
            try
            {
                // If player inventory can already accept the item, let vanilla handle it
                if (StoreToBackpack.CanInventoryAccept(__instance, item, stack))
                    return true;

                // Check if crafting output overflow to backpack is active (strictly excluding backpacks)
                if (IsDoingCrafting && !item.IsBackpack() && !item.TryGetBackpackItem(out _) && CraftFromBackpack.CanCraftOutputToBackpack(Player.m_localPlayer, out var craftBpInventory))
                {
                    if (StoreToBackpack.CanInventoryAccept(craftBpInventory, item, stack))
                    {
                        __result = true;
                        return false;
                    }
                }

                if (StoreToBackpack.ShouldStoreToBackpack(Player.m_localPlayer, item, out _))
                {
                    __result = true;
                    return false;
                }

                return true;
            }
            finally
            {
                _evaluatingCanAddItem = false;
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
                if (__instance.HaveEmptySlot())
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
            }

            if (Player.m_localPlayer != null && __instance == Player.m_localPlayer.GetInventory() && !_movingItemBetweenContainers)
            {
                // If crafting result and player inventory is full, store to equipped backpack if enabled (never for backpacks)
                if (IsDoingCrafting && !item.IsBackpack() && !item.TryGetBackpackItem(out _) && CraftFromBackpack.CanCraftOutputToBackpack(Player.m_localPlayer, out var craftBpInventory))
                {
                    if (!StoreToBackpack.CanInventoryAccept(__instance, item, item.m_stack))
                    {
                        var fullyStoredCraft = StoreToBackpack.TryStoreItem(Player.m_localPlayer, item, craftBpInventory);
                        if (fullyStoredCraft)
                        {
                            __result = true;
                            return false;
                        }
                    }
                }

                if (StoreToBackpack.ShouldStoreToBackpack(Player.m_localPlayer, item, out var backpackInventory))
                {
                    var fullyStored = StoreToBackpack.TryStoreItem(Player.m_localPlayer, item, backpackInventory);
                    if (fullyStored)
                    {
                        __result = true;
                        return false;
                    }
                }
            }
            
            return true;
        }
    }

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveItem), new[] { typeof(string), typeof(int), typeof(int), typeof(bool) })]
    static class RemoveItemByNamePatch
    {
        static bool Prefix(Inventory __instance, string name, ref int amount, int itemQuality)
        {
            if (!IsDoingCrafting || Player.m_localPlayer == null || __instance != Player.m_localPlayer.GetInventory())
                return true;

            if (CraftFromBackpack.CanCraftFromBackpack(Player.m_localPlayer, out _))
            {
                var remaining = CraftFromBackpack.ConsumeCraftingItem(Player.m_localPlayer, name, amount, itemQuality);
                if (remaining <= 0)
                {
                    return false;
                }

                amount = remaining;
                return true;
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
            }
        }
        
        static void Postfix()
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

        static void Postfix()
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

        static void Postfix()
        {
            _movingItemBetweenContainers = false;
        }
    }

    
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.UpdateTotalWeight))]
    static class UpdateTotalWeightPatch
    { 
        static void Postfix(Inventory __instance)
        {
            if (__instance == null || Player.m_localPlayer == null)
                return;
            
            var player = Player.m_localPlayer;
            
            if (__instance.IsBackPackInventory())
            {
                // When the equipped backpack inventory total weight is updated, the player inventory total weight should also be updated.
                if (player.IsBackpackEquipped())
                {
                    var backpack = player.GetEquippedBackpack();
                    if (backpack != null && backpack.GetInventory() == __instance)
                    {
                        AdventureBackpacks.Log.Debug($"########################################");
                        AdventureBackpacks.Log.Debug($"####       UpdateTotalWeight       #####");
                        AdventureBackpacks.Log.Debug($"Inventory Instance: {__instance.m_name}");
                        AdventureBackpacks.Log.Debug($"Backpack Name: {backpack.Item?.m_shared?.m_name}");
                        AdventureBackpacks.Log.Debug($"########################################");
                        
                        player.GetInventory()?.UpdateTotalWeight();
                    }
                }
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

            // Get a list of all items on the player.
            List<ItemDrop.ItemData> items = __instance.GetAllItems();
            
            // If the inventory being checked for teleportability is the Player's inventory, see whether it contains any backpacks, and then check the backpack inventories for teleportability too
            if (__instance == Player.m_localPlayer.GetInventory())
            {
                //am I wearing a backpack?
                if (Player.m_localPlayer.IsBackpackEquipped())
                {
                    var backpack = Player.m_localPlayer.GetEquippedBackpack();
                    if (backpack != null && !backpack.GetInventory().IsTeleportable(false))
                    {
                        __result = false;
                        return;
                    }
                }
                
                // Go through all the items, match them for any of the names in backpackTypes.
                // For each match found, check if the Inventory of that backpack is teleportable.
                foreach (ItemDrop.ItemData item in items)
                {
                    if (item == null)
                        continue;
                
                    if (item.IsBackpack())
                    {
                        if (!item.Data().GetOrCreate<BackpackComponent>().GetInventory().IsTeleportable(false))
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
