using AdventureBackpacks.Assets;
using AdventureBackpacks.Components;
using AdventureBackpacks.Patches;
using Vapok.Common.Managers;

namespace AdventureBackpacks.Extensions;

public static class PlayerExtensions
{
    public static bool IsBackpackEquipped(this Player player)
    {
        return InventoryGuiPatches.BackpackEquipped;
    }

    public static BackpackComponent GetEquippedBackpack(this Player player)
    {
        if (player == null || player.GetInventory() == null)
            return null;
            
        // Get a list of all equipped items.
        var equippedItems = player.GetInventory().GetEquipedtems();

        if (equippedItems is null) return null;

        // Go through all the equipped items, match them for any of the names in backpackTypes.
        // If a match is found, return the backpack ItemData object.
        foreach (ItemDrop.ItemData item in equippedItems)
        {
            if (item.TryGetBackpackItem(out var backpack))
            {
                return item.Data().GetOrCreate<BackpackComponent>();
            }
        }

        // Return null if no backpacks are found.
        return null;
    }

    public static bool CanOpenBackpack(this Player player)
    {
        return IsBackpackEquipped(player);
    }

    public static void OpenBackpack(this Player player, bool track = true)
    {
        if (player == null)
            return;
        
        var backpackContainer = player.gameObject.GetComponent<Container>();
            
        if (backpackContainer == null)
            backpackContainer = player.gameObject.AddComponent<Container>();

        var backpack = GetEquippedBackpack(player);
        var inventory = backpack.GetInventory();
        backpackContainer.m_inventory = inventory;
        backpackContainer.m_width = inventory.m_width;
        backpackContainer.m_height = inventory.m_height;
        backpackContainer.m_bkg = backpack.Item.m_shared.m_icons[0];

        InventoryGuiPatches.BackpackIsOpen = true;
        InventoryGuiPatches.BackpackIsOpening = track;
        InventoryGui.instance.Show(backpackContainer);
    }

    public static void QuickDropBackpack(this Player player)
    {
        AdventureBackpacks.Log.Message("Quick dropping backpack.");

        if (player == null)
            return;
        
        var backpack = GetEquippedBackpack(player);

        if (backpack == null)
            return;

        AdventureBackpacks.QuickDropping = true;
        
        // Unequip and remove backpack from player's back
        // We need to unequip the item BEFORE we drop it, otherwise when we pick it up again the game thinks
        // we had it equipped all along and fails to update player model, resulting in invisible backpack.
        player.RemoveEquipAction(backpack.Item);
        player.UnequipItem(backpack.Item, true);
        player.m_inventory.RemoveItem(backpack.Item);

        // This drops a copy of the backpack itemDrop.itemData
        var itemDrop = ItemDrop.DropItem(backpack.Item, 1, player.transform.position + player.transform.forward + player.transform.up, player.transform.rotation);
        itemDrop.Save();

        InventoryGuiPatches.BackpackIsOpen = false;
        AdventureBackpacks.QuickDropping = false;
    }
}