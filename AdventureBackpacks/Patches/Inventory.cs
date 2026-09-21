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
                            var backpackContainer = player.GetBackpackContainerProxy(false);
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
        [HarmonyPrepare]
        private static bool Prepare() => !PlayerExtensions.IsDedicatedOrHeadless();

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
        [HarmonyPrepare]
        private static bool Prepare() => !PlayerExtensions.IsDedicatedOrHeadless();

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
    private static bool RemoveItemPrefix(Inventory __instance, ItemDrop.ItemData item)
    {
        if (__instance == null || Player.m_localPlayer == null)
            return true;

        if (_movingItemBetweenContainers || _droppingOutside)
            return true;

        if (IsDoingCrafting)
        {
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

        if (!Backpacks.PerformYardSale(Player.m_localPlayer, item, true))
        {
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$vapok_mod_yard_sale_blocked");
            return false;
        }

        return true;
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
                if (StoreToBackpack.CanInventoryAccept(__instance, item, stack))
                    return true;

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
        [HarmonyPrepare]
        private static bool Prepare() => !PlayerExtensions.IsDedicatedOrHeadless();

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
        [HarmonyPrepare]
        private static bool Prepare() => !PlayerExtensions.IsDedicatedOrHeadless();

        static void Postfix(Inventory __instance)
        {
            if (__instance == null || Player.m_localPlayer == null)
                return;
            
            var player = Player.m_localPlayer;
            
            if (__instance.IsBackPackInventory())
            {
                if (player.IsBackpackEquipped())
                {
                    var backpack = player.GetEquippedBackpack();
                    if (backpack != null && backpack.GetInventory() == __instance)
                    {
                        player.GetInventory()?.UpdateTotalWeight();
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.IsTeleportable))]
    static class IsTeleportablePatch
    {
        [HarmonyPrepare]
        private static bool Prepare() => !PlayerExtensions.IsDedicatedOrHeadless();

        static void Postfix(Inventory __instance, ref bool __result)
        {
            if (__instance == null || Player.m_localPlayer == null)
                return;

            List<ItemDrop.ItemData> items = __instance.GetAllItems();
            
            if (__instance == Player.m_localPlayer.GetInventory())
            {
                if (Player.m_localPlayer.IsBackpackEquipped())
                {
                    var backpack = Player.m_localPlayer.GetEquippedBackpack();
                    var bpInventory = backpack?.GetInventory();
                    if (bpInventory != null && !bpInventory.IsTeleportable(false))
                    {
                        __result = false;
                        return;
                    }
                }
                
                if (items != null)
                {
                    foreach (ItemDrop.ItemData item in items)
                    {
                        if (item == null)
                            continue;
                    
                        if (item.IsBackpack())
                        {
                            var bpInventory = item.Data()?.GetOrCreate<BackpackComponent>()?.GetInventory();
                            if (bpInventory != null && !bpInventory.IsTeleportable(false))
                            {
                                __result = false;
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}
