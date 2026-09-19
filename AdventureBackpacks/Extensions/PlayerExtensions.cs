using AdventureBackpacks.Components;
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
    public const string BackpackProxyName = "AB_BackpackProxy";
    private static Container _backpackProxyContainer;

    public static Container GetBackpackContainerProxy(this Player player)
    {
        try
        {
            if (player == null || player.gameObject == null || player.transform == null)
                return null;

            if (_backpackProxyContainer != null &&
                _backpackProxyContainer.gameObject != null &&
                _backpackProxyContainer.transform != null &&
                _backpackProxyContainer.transform.parent == player.transform)
            {
                return _backpackProxyContainer;
            }

            var existingTransform = player.transform.Find(BackpackProxyName);
            if (existingTransform != null && existingTransform.gameObject != null)
            {
                _backpackProxyContainer = existingTransform.GetComponent<Container>();
                if (_backpackProxyContainer != null)
                    return _backpackProxyContainer;

                _backpackProxyContainer = existingTransform.gameObject.AddComponent<Container>();
                _backpackProxyContainer.m_name = "Backpack";
                _backpackProxyContainer.m_nview = player.m_nview;
                return _backpackProxyContainer;
            }

            var proxyObj = new GameObject(BackpackProxyName);
            proxyObj.transform.SetParent(player.transform, false);

            _backpackProxyContainer = proxyObj.AddComponent<Container>();
            _backpackProxyContainer.m_name = "Backpack";
            _backpackProxyContainer.m_nview = player.m_nview;

            return _backpackProxyContainer;
        }
        catch (System.Exception ex)
        {
            AdventureBackpacks.Log?.Warning($"Error obtaining backpack container proxy: {ex.Message}");
            return null;
        }
    }

    public static void DestroyBackpackContainerProxy(this Player player)
    {
        try
        {
            if (_backpackProxyContainer != null)
            {
                if (_backpackProxyContainer.gameObject != null)
                {
                    Object.Destroy(_backpackProxyContainer.gameObject);
                }
                _backpackProxyContainer = null;
            }

            if (player != null && player.gameObject != null && player.transform != null)
            {
                var existingTransform = player.transform.Find(BackpackProxyName);
                if (existingTransform != null && existingTransform.gameObject != null)
                {
                    Object.Destroy(existingTransform.gameObject);
                }
            }
        }
        catch (System.Exception ex)
        {
            AdventureBackpacks.Log?.Debug($"Exception during DestroyBackpackContainerProxy: {ex.Message}");
        }
    }

    public static void DestroyBackpackContainerProxy()
    {
        DestroyBackpackContainerProxy(null);
    }

    public static void OpenBackpack(this Player player, InventoryGui instance = null)
    {
        if (player == null || !player.IsBackpackEquipped())
            return;

        if (instance == null)
            instance = InventoryGui.instance;

        if (instance == null)
            return;

        var backpack = player.GetEquippedBackpack();
        if (backpack == null)
            return;

        var backpackContainer = player.GetBackpackContainerProxy();
        if (backpackContainer == null)
            return;

        backpack.UpdateContainerSizing(ref backpackContainer);

        InventoryGuiPatches.BackpackIsOpen = true;
        try
        {
            instance.Show(backpackContainer);
        }
        catch (System.Exception ex)
        {
            InventoryGuiPatches.BackpackIsOpen = false;
            AdventureBackpacks.Log?.Warning($"Error opening backpack container in InventoryGui: {ex.Message}");
        }
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

        if (!player.m_inventory.RemoveItem(backpack.Item))
        {
            // Removal failed (e.g. another mod blocked it). Do not drop — would duplicate the backpack.
            if (swapItemActivated)
                playerInventory.AddItem(tempItemRemoval);
            AdventureBackpacks.QuickDropping = false;
            player.Message(MessageHud.MessageType.Center, "$vapok_mod_quick_drop_unavailable");
            return;
        }

        // This drops a copy of the backpack itemDrop.itemData
        var itemDrop = ItemDrop.DropItem(backpack.Item, 1, player.transform.position - player.transform.forward + player.transform.up, player.transform.rotation);
        if (itemDrop != null)
        {
            var rb = itemDrop.GetComponent<Rigidbody>();
            if (rb != null)
                rb.linearVelocity = (Vector3.up - player.transform.forward) * 5f;
            itemDrop.Save();
        }

        player.m_dropEffects.Create(player.transform.position, Quaternion.identity);

        if (swapItemActivated)
            playerInventory.AddItem(tempItemRemoval);
        
        InventoryGuiPatches.BackpackIsOpen = false;
        AdventureBackpacks.QuickDropping = false;
    }
}