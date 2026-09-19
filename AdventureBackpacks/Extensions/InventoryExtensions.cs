namespace AdventureBackpacks.Extensions;

public static class InventoryExtensions
{
    public static bool IsBackPackInventory(this Inventory inventory)
    {
        return inventory != null
               && !string.IsNullOrEmpty(inventory.m_name)
               && inventory.m_name.Contains("$vapok_mod_level");
    }

    public static bool ContainsBackpack(this Inventory inventory, ItemDrop.ItemData backpackItem)
    {
        //This is a special contains method to prevent other extended inventory mods from checking other inventories 
        //by prefixing Inventory.Contains() method.
        
        bool IsBackpackItemAt(int x, int y)
        {
            if (x < 0)
            {
                x = inventory.m_width - 1;
                y--;
            }

            if (y < 0)
                return false;
            
            var itemAt = inventory.GetItemAt(x, y);
            
            if (itemAt != null && itemAt == backpackItem)
                return true;
            
            return IsBackpackItemAt(x-1, y);
        }

        return IsBackpackItemAt(inventory.m_width - 1, inventory.m_height - 1);
    }
    public static bool HasEmptySlot(this Inventory inventory)
    {
        bool IsBackpackItemAt(int x, int y)
        {
            if (x < 0)
            {
                x = inventory.m_width - 1;
                y--;
            }

            if (y < 0)
                return false;
            
            var itemAt = inventory.GetItemAt(x, y);
            
            if (itemAt == null)
                return true;
            
            return IsBackpackItemAt(x-1, y);
        }

        return IsBackpackItemAt(inventory.m_width - 1, inventory.m_height - 1);
    }
    public static ItemDrop.ItemData FindNonBackpackItem(this Inventory inventory)
    {
        
        ItemDrop.ItemData GetNonBackpackItem(int x, int y)
        {
            if (x < 0)
            {
                x = inventory.m_width - 1;
                y--;
            }

            if (y < 0)
                return null;
            
            var itemAt = inventory.GetItemAt(x, y);
            
            if (itemAt != null && itemAt.IsBackpack())
                itemAt = GetNonBackpackItem(x-1, y);
            
            return itemAt;
        }

        return GetNonBackpackItem(inventory.m_width - 1, inventory.m_height - 1);
    }

    /// <summary>
    /// Safely checks whether an inventory contains an item with matching name, guarding against null items and uninitialized shared data.
    /// </summary>
    public static bool SafeHaveItem(this Inventory inventory, string name)
    {
        if (inventory?.m_inventory == null || string.IsNullOrEmpty(name))
            return false;

        foreach (var item in inventory.m_inventory)
        {
            if (item != null && item.m_shared != null && item.m_shared.m_name == name)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Safely calculates available free stack space in an inventory, guarding against null items, uninitialized shared data, and external mod slot anomalies.
    /// </summary>
    public static int SafeFindFreeStackSpace(this Inventory inventory, string name, float worldLevel)
    {
        if (inventory?.m_inventory == null || string.IsNullOrEmpty(name))
            return 0;

        int num = 0;
        foreach (var item in inventory.m_inventory)
        {
            if (item != null && item.m_shared != null && item.m_shared.m_name == name && item.m_stack < item.m_shared.m_maxStackSize && (float)item.m_worldLevel == worldLevel)
            {
                num += item.m_shared.m_maxStackSize - item.m_stack;
            }
        }
        return num;
    }
}