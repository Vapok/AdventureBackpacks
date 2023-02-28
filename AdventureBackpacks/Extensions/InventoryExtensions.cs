namespace AdventureBackpacks.Extensions;

public static class InventoryExtensions
{
    public static bool IsBackPackInventory(this Inventory inventory)
    {
        return inventory.m_name.Contains("$vapok_mod_level");
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
}