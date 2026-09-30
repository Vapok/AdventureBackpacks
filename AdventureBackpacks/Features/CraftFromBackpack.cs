using System.Collections.Generic;
using System.Linq;
using AdventureBackpacks.Components;
using AdventureBackpacks.Configuration;
using AdventureBackpacks.Extensions;
using BepInEx.Configuration;
using UnityEngine;
using Vapok.Common.Managers.Configuration;
using Vapok.Common.Shared;

namespace AdventureBackpacks.Features;

public enum ConsumptionPriority
{
    PlayerInventoryFirst,
    BackpackFirst
}

public static class CraftFromBackpack
{
    public static bool FeatureInitialized = false;
    public static ConfigEntry<bool> EnableCraftFromBackpack;
    public static ConfigEntry<bool> EnableCraftOutputToBackpack;
    public static ConfigEntry<ConsumptionPriority> MaterialConsumptionPriority;
    public static ConfigEntry<bool> LeaveOneItemInBackpack;
    public static ConfigEntry<bool> DisplayTotalIngredientCount;
    public static ConfigEntry<string> IngredientCountMatchPattern;

    static CraftFromBackpack()
    {
        ConfigRegistry.Waiter.StatusChanged += (_, _) => RegisterConfigurationFile();
    }

    private static void RegisterConfigurationFile()
    {
        ConfigSyncBase.UnsyncedConfig("Automation (Local Only)", "Enable Craft From Backpack", true,
            new ConfigDescription("When enabled, materials from the equipped backpack are considered when crafting or building.",
                null,
                new ConfigurationManagerAttributes { Order = 6 }), ref EnableCraftFromBackpack);

        ConfigSyncBase.UnsyncedConfig("Automation (Local Only)", "Enable Craft Output To Backpack", true,
            new ConfigDescription("When enabled and player inventory is full, newly crafted items will be placed into the equipped backpack if space is available.",
                null,
                new ConfigurationManagerAttributes { Order = 5 }), ref EnableCraftOutputToBackpack);

        ConfigSyncBase.UnsyncedConfig("Automation (Local Only)", "Material Consumption Priority", ConsumptionPriority.PlayerInventoryFirst,
            new ConfigDescription("Determines whether materials are drawn from the player inventory or the equipped backpack first during crafting and building.",
                null,
                new ConfigurationManagerAttributes { Order = 4 }), ref MaterialConsumptionPriority);

        ConfigSyncBase.UnsyncedConfig("Automation (Local Only)", "Leave One Item In Backpack", true,
            new ConfigDescription("When enabled, at least one item of each resource type will remain in the backpack and will not be consumed or counted during crafting and building.",
                null,
                new ConfigurationManagerAttributes { Order = 3 }), ref LeaveOneItemInBackpack);

        ConfigSyncBase.UnsyncedConfig("Automation (Local Only)", "Display Total Ingredient Count", true,
            new ConfigDescription("When enabled, requirement amounts in the crafting panel display as 'Available/Required' to reflect backpack contents.",
                null,
                new ConfigurationManagerAttributes { Order = 2 }), ref DisplayTotalIngredientCount);

        ConfigSyncBase.UnsyncedConfig("Automation (Local Only)", "Ingredient Count Match Pattern", @"\d+[/(]\d+",
            new ConfigDescription("Regex pattern used to detect if another mod has already formatted the requirement count label (e.g. '123/50' or '50(123)'). If matched, AdventureBackpacks will not overwrite it.",
                null,
                new ConfigurationManagerAttributes { Order = 1 }), ref IngredientCountMatchPattern);
    }

    public static bool CanCraftFromBackpack(Player player, out Inventory backpackInventory)
    {
        backpackInventory = null;

        try
        {
            if (PlayerExtensions.IsDedicatedOrHeadless())
                return false;

            if (!FeatureInitialized || EnableCraftFromBackpack == null || !EnableCraftFromBackpack.Value)
                return false;

            if (player == null || !player.IsBackpackEquipped())
                return false;

            BackpackComponent backpack = player.GetEquippedBackpack();
            if (backpack == null)
                return false;

            backpackInventory = backpack.GetInventory();
            return backpackInventory != null;
        }
        catch (System.Exception ex)
        {
            AdventureBackpacks.Log?.Warning($"Error in CanCraftFromBackpack: {ex.Message}");
            return false;
        }
    }

    public static bool CanCraftOutputToBackpack(Player player, out Inventory backpackInventory)
    {
        backpackInventory = null;

        try
        {
            if (PlayerExtensions.IsDedicatedOrHeadless())
                return false;

            if (!FeatureInitialized || EnableCraftOutputToBackpack == null || !EnableCraftOutputToBackpack.Value)
                return false;

            if (player == null || !player.IsBackpackEquipped())
                return false;

            BackpackComponent backpack = player.GetEquippedBackpack();
            if (backpack == null)
                return false;

            backpackInventory = backpack.GetInventory();
            return backpackInventory != null;
        }
        catch (System.Exception ex)
        {
            AdventureBackpacks.Log?.Warning($"Error in CanCraftOutputToBackpack: {ex.Message}");
            return false;
        }
    }

    public static int GetBackpackItemCount(Player player, string itemName, int quality = -1)
    {
        if (!CanCraftFromBackpack(player, out Inventory backpackInventory))
            return 0;

        if (string.IsNullOrEmpty(itemName))
            return 0;

        int count = quality > 0 
            ? backpackInventory.CountItems(itemName, quality) 
            : backpackInventory.CountItems(itemName);

        if (LeaveOneItemInBackpack != null && LeaveOneItemInBackpack.Value && count > 0)
        {
            count = Mathf.Max(0, count - 1);
        }

        return count;
    }

    public static int GetPlayerAvailableCount(Player player, string itemName, int itemQuality = -1)
    {
        if (player == null || string.IsNullOrEmpty(itemName))
            return 0;

        Inventory playerInventory = player.GetInventory();
        if (playerInventory == null)
            return 0;

        List<ItemDrop.ItemData> allItems = playerInventory.GetAllItems();
        if (allItems == null)
            return 0;

        int count = 0;
        for (int i = 0; i < allItems.Count; i++)
        {
            ItemDrop.ItemData item = allItems[i];
            if (item != null && !item.m_equipped && item.m_shared != null &&
                string.Equals(item.m_shared.m_name, itemName) &&
                (itemQuality < 0 || item.m_quality == itemQuality))
            {
                count += item.m_stack;
            }
        }
        return count;
    }

    public static int GetBackpackAvailableCount(Inventory backpackInventory, string itemName, int itemQuality = -1)
    {
        if (backpackInventory == null || string.IsNullOrEmpty(itemName))
            return 0;

        List<ItemDrop.ItemData> allBpItems = backpackInventory.GetAllItems();
        if (allBpItems == null)
            return 0;

        int total = 0;
        for (int i = 0; i < allBpItems.Count; i++)
        {
            ItemDrop.ItemData item = allBpItems[i];
            if (item != null && item.m_shared != null &&
                string.Equals(item.m_shared.m_name, itemName) &&
                (itemQuality < 0 || item.m_quality == itemQuality))
            {
                total += item.m_stack;
            }
        }

        if (LeaveOneItemInBackpack != null && LeaveOneItemInBackpack.Value && total > 0)
        {
            total = Mathf.Max(0, total - 1);
        }

        return total;
    }

    public static void ProtectEquippedItems(Inventory playerInventory, string itemName)
    {
        if (playerInventory == null || playerInventory.m_inventory == null || string.IsNullOrEmpty(itemName))
            return;

        List<ItemDrop.ItemData> inventoryList = playerInventory.m_inventory;
        for (int i = 0; i < inventoryList.Count; i++)
        {
            ItemDrop.ItemData item = inventoryList[i];
            if (item != null && item.m_equipped && item.m_shared != null && string.Equals(item.m_shared.m_name, itemName))
            {
                bool hasUnequippedLater = false;
                for (int j = i + 1; j < inventoryList.Count; j++)
                {
                    ItemDrop.ItemData laterItem = inventoryList[j];
                    if (laterItem != null && !laterItem.m_equipped && laterItem.m_shared != null && string.Equals(laterItem.m_shared.m_name, itemName))
                    {
                        hasUnequippedLater = true;
                        break;
                    }
                }

                if (hasUnequippedLater)
                {
                    inventoryList.RemoveAt(i);
                    inventoryList.Add(item);
                    i--;
                }
            }
        }
    }

    public static int DeductBackpackCraftingItem(Player player, Inventory backpackInventory, string itemName, int amount, int itemQuality = -1)
    {
        if (amount <= 0 || player == null || backpackInventory == null || string.IsNullOrEmpty(itemName))
            return amount;

        ConsumptionPriority priority = MaterialConsumptionPriority?.Value ?? ConsumptionPriority.PlayerInventoryFirst;

        int playerAvailable = GetPlayerAvailableCount(player, itemName, itemQuality);
        int bpAvailable = GetBackpackAvailableCount(backpackInventory, itemName, itemQuality);

        int toConsumeFromBp = 0;
        if (priority == ConsumptionPriority.BackpackFirst)
        {
            toConsumeFromBp = Mathf.Min(amount, bpAvailable);
        }
        else
        {
            int shortage = Mathf.Max(0, amount - playerAvailable);
            toConsumeFromBp = Mathf.Min(shortage, bpAvailable);
        }

        if (toConsumeFromBp > 0)
        {
            try
            {
                backpackInventory.RemoveItem(itemName, toConsumeFromBp, itemQuality);
            }
            catch (System.Exception ex)
            {
                AdventureBackpacks.Log?.Warning($"Error removing {itemName} from backpack: {ex.Message}");
            }
        }

        int remainingForPlayer = amount - toConsumeFromBp;
        return remainingForPlayer;
    }

    private static void ConsumeFromPlayer(Player player, string itemName, ref int remaining, int itemQuality)
    {
        if (remaining <= 0 || player == null)
            return;

        Inventory playerInventory = player.GetInventory();
        if (playerInventory == null)
            return;

        List<ItemDrop.ItemData> allItems = playerInventory.GetAllItems();
        if (allItems == null)
            return;

        List<ItemDrop.ItemData> matchingItems = allItems.Where(x => 
            x != null &&
            !x.m_equipped && 
            x.m_shared != null && 
            string.Equals(x.m_shared.m_name, itemName) &&
            (itemQuality < 0 || x.m_quality == itemQuality)).ToList();

        foreach (ItemDrop.ItemData item in matchingItems)
        {
            if (remaining <= 0)
                break;

            if (item == null || item.m_stack <= 0)
                continue;

            int toRemove = Mathf.Min(item.m_stack, remaining);
            playerInventory.RemoveItem(item, toRemove);
            remaining -= toRemove;
        }
    }

    private static void ConsumeFromBackpack(Inventory backpackInventory, string itemName, ref int remaining, int itemQuality)
    {
        if (remaining <= 0 || backpackInventory == null)
            return;

        List<ItemDrop.ItemData> allBpItems = backpackInventory.GetAllItems();
        if (allBpItems == null)
            return;

        List<ItemDrop.ItemData> matchingBpItems = allBpItems.Where(x => 
            x != null &&
            x.m_shared != null && 
            string.Equals(x.m_shared.m_name, itemName) &&
            (itemQuality < 0 || x.m_quality == itemQuality)).ToList();

        int maxConsumable = matchingBpItems.Sum(x => x.m_stack);
        if (LeaveOneItemInBackpack != null && LeaveOneItemInBackpack.Value && maxConsumable > 0)
        {
            maxConsumable = Mathf.Max(0, maxConsumable - 1);
        }

        int toConsumeTotal = Mathf.Min(remaining, maxConsumable);
        if (toConsumeTotal <= 0)
            return;

        backpackInventory.RemoveItem(itemName, toConsumeTotal, itemQuality);
        remaining -= toConsumeTotal;
    }

    public static int ConsumeCraftingItem(Player player, string itemName, int amount, int itemQuality = -1)
    {
        int remaining = amount;
        if (remaining <= 0 || player == null || string.IsNullOrEmpty(itemName) || PlayerExtensions.IsDedicatedOrHeadless())
            return remaining;

        try
        {
            if (CanCraftFromBackpack(player, out Inventory backpackInventory) && backpackInventory != null)
            {
                remaining = DeductBackpackCraftingItem(player, backpackInventory, itemName, remaining, itemQuality);
            }

            if (remaining > 0)
            {
                ConsumeFromPlayer(player, itemName, ref remaining, itemQuality);
            }
        }
        catch (System.Exception ex)
        {
            AdventureBackpacks.Log?.Warning($"Error during ConsumeCraftingItem for {itemName}: {ex.Message}");
        }

        return remaining;
    }
}
