using System;
using AdventureBackpacks.Assets;
using AdventureBackpacks.Components;
using AdventureBackpacks.Configuration;
using AdventureBackpacks.Extensions;
using AdventureBackpacks.Patches;
using BepInEx.Configuration;
using UnityEngine;
using Vapok.Common.Managers.Configuration;
using Vapok.Common.Shared;

namespace AdventureBackpacks.Features;

public static class StoreToBackpack
{
    public static bool FeatureInitialized = false;
    public static ConfigEntry<bool> EnableStoreToBackpack;
    public static ConfigEntry<bool> EnableInventoryOverflowToBackpack;

    static StoreToBackpack()
    {
        ConfigRegistry.Waiter.StatusChanged += (_, _) => RegisterConfigurationFile();
    }

    private static void RegisterConfigurationFile()
    {
        ConfigSyncBase.UnsyncedConfig("Automation (Local Only)", "Enable Auto Store to Backpack", true,
            new ConfigDescription("When enabled, picked up or looted items already present in the equipped backpack are automatically stored in the backpack.",
                null,
                new ConfigurationManagerAttributes { Order = 2 }), ref EnableStoreToBackpack);

        ConfigSyncBase.UnsyncedConfig("Automation (Local Only)", "Enable Inventory Overflow To Backpack", true,
            new ConfigDescription("When enabled, if the player inventory is full, picked up or looted items will automatically overflow into the equipped backpack if space is available.",
                null,
                new ConfigurationManagerAttributes { Order = 1 }), ref EnableInventoryOverflowToBackpack);
    }

    public static bool ShouldStoreToBackpack(Player player, ItemDrop.ItemData item, out Inventory backpackInventory)
    {
        backpackInventory = null;

        if (PlayerExtensions.IsDedicatedOrHeadless() || !FeatureInitialized || EnableStoreToBackpack == null || EnableInventoryOverflowToBackpack == null)
            return false;

        if (player == null || item == null || item.m_shared == null)
            return false;

        if (item.IsBackpack() || item.TryGetBackpackItem(out _))
            return false;

        if (AdventureBackpacks.PerformYardSale || AdventureBackpacks.QuickDropping || AdventureBackpacks.BypassMoveProtection)
            return false;

        if (InventoryGuiPatches.BackpackIsOpen)
            return false;

        if (!player.IsBackpackEquipped())
            return false;

        BackpackComponent backpack = player.GetEquippedBackpack();
        if (backpack == null)
            return false;

        backpackInventory = backpack.GetInventory();
        if (backpackInventory == null)
            return false;

        Inventory playerInventory = player.GetInventory();
        if (playerInventory == null)
            return false;

        if (EnableStoreToBackpack.Value && backpackInventory.SafeHaveItem(item.m_shared.m_name))
        {
            if (CanInventoryAccept(backpackInventory, item, item.m_stack))
                return true;

            int freeStack = backpackInventory.SafeFindFreeStackSpace(item.m_shared.m_name, item.m_worldLevel);
            int emptySlots = (backpackInventory.m_width * backpackInventory.m_height) - (backpackInventory.m_inventory?.Count ?? 0);
            if (freeStack > 0 || (emptySlots > 0 && item.m_shared.m_maxStackSize > 1))
                return true;
        }

        if (EnableInventoryOverflowToBackpack.Value && !CanInventoryAccept(playerInventory, item, item.m_stack))
        {
            if (CanInventoryAccept(backpackInventory, item, item.m_stack))
                return true;

            int freeStack = backpackInventory.SafeFindFreeStackSpace(item.m_shared.m_name, item.m_worldLevel);
            int emptySlots = (backpackInventory.m_width * backpackInventory.m_height) - (backpackInventory.m_inventory?.Count ?? 0);
            if (freeStack > 0 || (emptySlots > 0 && item.m_shared.m_maxStackSize > 1))
                return true;
        }

        return false;
    }

    public static bool CanInventoryAccept(Inventory inventory, ItemDrop.ItemData item, int stack = -1)
    {
        if (inventory?.m_inventory == null || item?.m_shared == null)
            return false;

        if (inventory.HaveEmptySlot())
            return true;

        if (item.m_shared.m_maxStackSize <= 1)
            return false;

        if (stack <= 0)
            stack = item.m_stack;

        return inventory.SafeFindFreeStackSpace(item.m_shared.m_name, item.m_worldLevel) >= stack;
    }

    public static bool TryStoreItem(Player player, ItemDrop.ItemData item, Inventory backpackInventory)
    {
        if (PlayerExtensions.IsDedicatedOrHeadless() || player == null || item == null || backpackInventory == null || item.m_shared == null || string.IsNullOrEmpty(item.m_shared.m_name))
            return false;

        try
        {
            if (item.IsBackpack() || item.TryGetBackpackItem(out _) || !Backpacks.CheckForInception(backpackInventory, item))
                return false;

            if (CanInventoryAccept(backpackInventory, item, item.m_stack))
            {
                bool added = backpackInventory.AddItem(item);
                if (added)
                    return true;
            }

            if (item.m_shared.m_maxStackSize > 1 && item.m_stack > 1)
            {
                int freeStack = backpackInventory.SafeFindFreeStackSpace(item.m_shared.m_name, item.m_worldLevel);
                int emptySlots = (backpackInventory.m_width * backpackInventory.m_height) - (backpackInventory.m_inventory?.Count ?? 0);
                int availableSpace = freeStack + (emptySlots * item.m_shared.m_maxStackSize);

                if (availableSpace > 0)
                {
                    int transferAmount = Mathf.Min(item.m_stack, availableSpace);
                    item.m_customData ??= new System.Collections.Generic.Dictionary<string, string>();
                    ItemDrop.ItemData partialItem = item.Clone();
                    partialItem.m_stack = transferAmount;

                    if (backpackInventory.AddItem(partialItem))
                    {
                        item.m_stack -= transferAmount;
                        if (item.m_stack <= 0)
                            return true;
                    }
                }
            }

            return false;
        }
        catch (System.Exception ex)
        {
            AdventureBackpacks.Log?.Warning($"Error in TryStoreItem for {item.m_shared?.m_name}: {ex.Message}");
            return false;
        }
    }
}
