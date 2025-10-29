﻿using AdventureBackpacks.Components;
using AdventureBackpacks.Patches;
using UnityEngine;
using Vapok.Common.Managers;

namespace AdventureBackpacks.Extensions;

public static class PlayerExtensions
{
    public static bool IsBackpackEquipped(this Player player)
    {
        if (player == null || player.GetInventory() == null)
            return false;
            
        if (player.m_shoulderItem == null)
            return false;

        return player.m_shoulderItem.IsBackpack();
    }

    public static bool IsThisBackpackEquipped(this Player player, ItemDrop.ItemData itemData )
    {
        if (player == null || player.GetInventory() == null)
            return false;
            
        if (player.m_shoulderItem == null)
            return false;

        if (!player.m_shoulderItem.IsBackpack())
            return false;
        
        return player.m_shoulderItem.Equals(itemData);
    }

    public static BackpackComponent GetEquippedBackpack(this Player player)
    {
        if (player == null || player.GetInventory() == null)
            return null;
            
        if (player.m_shoulderItem == null)
            return null;

        if (player.m_shoulderItem.IsBackpack())
        {
            return player.m_shoulderItem.Data().GetOrCreate<BackpackComponent>();
        }
        // Return null if no backpacks are found.
        return null;
    }

    public static bool CanOpenBackpack(this Player player)
    {
        return IsBackpackEquipped(player);
    }

    public static void OpenBackpack(this Player player, InventoryGui instance)
    {
        if (player == null)
            return;
        
        var backpackContainer = player.gameObject.GetComponent<Container>();

        InventoryGuiPatches.BackpackIsOpen = true;
        instance.Show(backpackContainer);
    }

    public static void QuickDropBackpack(this Player player)
    {
        if (player == null)
            return;
        
        var backpack = GetEquippedBackpack(player);

        if (backpack == null)
            return;

        ItemDrop.ItemData tempItemRemoval = null;
        var swapItemActivated = false;
        var playerInventory = player.GetInventory();
        
        if (!playerInventory.ContainsBackpack(backpack.Item) && !playerInventory.HasEmptySlot())
        {
            tempItemRemoval = playerInventory.FindNonBackpackItem();
            
            if (tempItemRemoval != null && playerInventory.RemoveItem(tempItemRemoval))
                    swapItemActivated = true;
            else
            {
                player.Message(MessageHud.MessageType.Center, "$vapok_mod_quick_drop_unavailable");
                return;
            }
        }

        AdventureBackpacks.QuickDropping = true;
        AdventureBackpacks.Log.Message("Quick dropping backpack.");        
        // Unequip and remove backpack from player's back
        // We need to unequip the item BEFORE we drop it, otherwise when we pick it up again the game thinks
        // we had it equipped all along and fails to update player model, resulting in invisible backpack.
        player.RemoveEquipAction(backpack.Item);
        player.UnequipItem(backpack.Item, true);
        player.m_inventory.RemoveItem(backpack.Item);

        // This drops a copy of the backpack itemDrop.itemData
        var itemDrop = ItemDrop.DropItem(backpack.Item, 1, player.transform.position - player.transform.forward + player.transform.up, player.transform.rotation);
        itemDrop.GetComponent<Rigidbody>().linearVelocity = (Vector3.up - player.transform.forward) * 5f;
        player.m_dropEffects.Create(player.transform.position, Quaternion.identity);
        itemDrop.Save();

        if (swapItemActivated)
            playerInventory.AddItem(tempItemRemoval);
        
        InventoryGuiPatches.BackpackIsOpen = false;
        AdventureBackpacks.QuickDropping = false;
    }
}