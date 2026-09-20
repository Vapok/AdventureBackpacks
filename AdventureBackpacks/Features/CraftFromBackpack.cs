using System.Linq;
using AdventureBackpacks.Configuration;
using AdventureBackpacks.Extensions;
using BepInEx.Configuration;
using UnityEngine;
using Vapok.Common.Managers.Configuration;
using Vapok.Common.Shared;

namespace AdventureBackpacks.Features;

public static class CraftFromBackpack
{
    public static bool FeatureInitialized = false;
    public static ConfigEntry<bool> EnableCraftFromBackpack;
    public static ConfigEntry<bool> EnableCraftOutputToBackpack;

    static CraftFromBackpack()
    {
        ConfigRegistry.Waiter.StatusChanged += (_, _) => RegisterConfigurationFile();
    }

    private static void RegisterConfigurationFile()
    {
        ConfigSyncBase.SyncedConfig("Server Config", "Enable Craft From Backpack", true,
            new ConfigDescription("When enabled, materials from the equipped backpack are considered when crafting or building.",
                null,
                new ConfigurationManagerAttributes { Order = 5 }), ref EnableCraftFromBackpack);

        ConfigSyncBase.SyncedConfig("Server Config", "Enable Craft Output To Backpack", true,
            new ConfigDescription("When enabled and player inventory is full, newly crafted items will be placed into the equipped backpack if space is available.",
                null,
                new ConfigurationManagerAttributes { Order = 4 }), ref EnableCraftOutputToBackpack);
    }

    public static bool CanCraftFromBackpack(Player player, out Inventory backpackInventory)
    {
        backpackInventory = null;

        if (!FeatureInitialized || EnableCraftFromBackpack == null || !EnableCraftFromBackpack.Value)
            return false;

        if (player == null || !player.IsBackpackEquipped())
            return false;

        var backpack = player.GetEquippedBackpack();
        if (backpack == null)
            return false;

        backpackInventory = backpack.GetInventory();
        return backpackInventory != null;
    }

    public static bool CanCraftOutputToBackpack(Player player, out Inventory backpackInventory)
    {
        backpackInventory = null;

        if (!FeatureInitialized || EnableCraftOutputToBackpack == null || !EnableCraftOutputToBackpack.Value)
            return false;

        if (player == null || !player.IsBackpackEquipped())
            return false;

        var backpack = player.GetEquippedBackpack();
        if (backpack == null)
            return false;

        backpackInventory = backpack.GetInventory();
        return backpackInventory != null;
    }

    public static int GetBackpackItemCount(Player player, string itemName, int quality = -1)
    {
        if (!CanCraftFromBackpack(player, out var backpackInventory))
            return 0;

        if (string.IsNullOrEmpty(itemName))
            return 0;

        return quality > 0 
            ? backpackInventory.CountItems(itemName, quality) 
            : backpackInventory.CountItems(itemName);
    }

    public static int ConsumeCraftingItem(Player player, string itemName, int amount, int itemQuality = -1)
    {
        var remaining = amount;
        if (remaining <= 0 || player == null || string.IsNullOrEmpty(itemName))
            return remaining;

        try
        {
            var playerInventory = player.GetInventory();
            if (playerInventory != null)
            {
                var allItems = playerInventory.GetAllItems();
                if (allItems != null)
                {
                    var matchingItems = allItems.Where(x => 
                        x != null &&
                        !x.m_equipped && 
                        x.m_shared != null && 
                        string.Equals(x.m_shared.m_name, itemName) &&
                        (itemQuality < 0 || x.m_quality == itemQuality)).ToList();

                    foreach (var item in matchingItems)
                    {
                        if (remaining <= 0)
                            break;

                        var toRemove = Mathf.Min(item.m_stack, remaining);
                        playerInventory.RemoveItem(item, toRemove);
                        remaining -= toRemove;
                    }
                }
            }

            if (remaining > 0 && CanCraftFromBackpack(player, out var backpackInventory) && backpackInventory != null)
            {
                var allBpItems = backpackInventory.GetAllItems();
                if (allBpItems != null)
                {
                    var matchingBpItems = allBpItems.Where(x => 
                        x != null &&
                        x.m_shared != null && 
                        string.Equals(x.m_shared.m_name, itemName) &&
                        (itemQuality < 0 || x.m_quality == itemQuality)).ToList();

                    foreach (var item in matchingBpItems)
                    {
                        if (remaining <= 0)
                            break;

                        var toRemove = Mathf.Min(item.m_stack, remaining);
                        backpackInventory.RemoveItem(item, toRemove);
                        remaining -= toRemove;
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            AdventureBackpacks.Log?.Warning($"Error during ConsumeCraftingItem for {itemName}: {ex.Message}");
        }

        return remaining;
    }
}
