namespace AdventureBackpacks.Extensions;

public static class InventoryExtensions
{
    public static bool IsBackPackInventory(this Inventory inventory)
    {
        return inventory.m_name.Contains("$vapok_mod_level");
    }
}