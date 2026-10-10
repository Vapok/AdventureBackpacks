using System.Collections.Generic;
using AdventureBackpacks.Components;
using AdventureBackpacks.Patches;
using Jotunn.Managers;
using UnityEngine;
using Vapok.Common.Managers;

namespace AdventureBackpacks.Extensions;

public static class PlayerExtensions
{
    public static bool IsBackpackEquipped(this Player player)
    {
        if (player == null || player.GetInventory() == null)
            return false;
            
        if (player.m_shoulderItem != null && player.m_shoulderItem.IsBackpack())
            return true;

        List<ItemDrop.ItemData> equippedItems = player.GetInventory().GetEquippedItems();
        if (equippedItems != null)
        {
            foreach (ItemDrop.ItemData item in equippedItems)
            {
                if (item != null && item.IsBackpack())
                    return true;
            }
        }

        return false;
    }

    public static bool IsThisBackpackEquipped(this Player player, ItemDrop.ItemData itemData)
    {
        if (player == null || player.GetInventory() == null || itemData == null)
            return false;
            
        if (player.m_shoulderItem != null && player.m_shoulderItem.IsBackpack() && player.m_shoulderItem.Equals(itemData))
            return true;

        List<ItemDrop.ItemData> equippedItems = player.GetInventory().GetEquippedItems();
        if (equippedItems != null)
        {
            foreach (ItemDrop.ItemData item in equippedItems)
            {
                if (item != null && item.IsBackpack() && item.Equals(itemData))
                    return true;
            }
        }
        
        return false;
    }

    public static BackpackComponent GetEquippedBackpack(this Player player)
    {
        if (player == null || player.GetInventory() == null)
            return null;
            
        if (player.m_shoulderItem != null && player.m_shoulderItem.IsBackpack())
        {
            return player.m_shoulderItem.Data().GetOrCreate<BackpackComponent>();
        }

        List<ItemDrop.ItemData> equippedItems = player.GetInventory().GetEquippedItems();
        if (equippedItems != null)
        {
            foreach (ItemDrop.ItemData item in equippedItems)
            {
                if (item != null && item.IsBackpack())
                {
                    return item.Data().GetOrCreate<BackpackComponent>();
                }
            }
        }

        return null;
    }

    public static bool CanOpenBackpack(this Player player)
    {
        return IsBackpackEquipped(player);
    }

    public const string BackpackProxyName = "AB_BackpackProxy";

    public static bool IsDedicatedOrHeadless()
    {
        if (GUIManager.IsHeadless())
            return true;
        if (ZNet.instance != null && ZNet.instance.IsDedicated())
            return true;
        return false;
    }

    public static Container GetBackpackContainerProxy(this Player player, bool createIfMissing = true)
    {
        try
        {
            if (player == null || player.gameObject == null || player.transform == null)
                return null;

            if (IsDedicatedOrHeadless() || player != Player.m_localPlayer)
                return null;

            Transform existingTransform = player.transform.Find(BackpackProxyName);
            if (existingTransform != null && existingTransform.gameObject != null)
            {
                Container existingContainer = existingTransform.GetComponent<Container>();
                if (existingContainer != null)
                    return existingContainer;

                if (!createIfMissing)
                    return null;

                ZNetView existingNview = existingTransform.GetComponent<ZNetView>();
                if (existingNview == null)
                    existingNview = existingTransform.gameObject.AddComponent<ZNetView>();

                existingContainer = existingTransform.gameObject.AddComponent<Container>();
                existingContainer.m_name = "Backpack";
                existingContainer.m_nview = existingNview;
                return existingContainer;
            }

            if (!createIfMissing)
                return null;

            GameObject proxyObj = new GameObject(BackpackProxyName);
            proxyObj.transform.SetParent(player.transform, false);

            ZNetView proxyNview = proxyObj.AddComponent<ZNetView>();

            Container newContainer = proxyObj.AddComponent<Container>();
            newContainer.m_name = "Backpack";
            newContainer.m_nview = proxyNview;

            return newContainer;
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
        if (Player.m_localPlayer != null)
            Player.m_localPlayer.DestroyBackpackContainerProxy();
    }

    public static void OpenBackpack(this Player player, InventoryGui instance = null)
    {
        player.OpenBackpack(instance, 1);
    }

    public static void OpenBackpack(this Player player, InventoryGui instance, int activeGroup)
    {
        if (player == null || !player.IsBackpackEquipped())
            return;

        if (instance == null)
            instance = InventoryGui.instance;

        if (instance == null)
            return;

        BackpackComponent backpack = player.GetEquippedBackpack();
        if (backpack == null)
            return;

        Container backpackContainer = player.GetBackpackContainerProxy();
        if (backpackContainer == null)
            return;

        backpack.UpdateContainerSizing(ref backpackContainer);

        InventoryGuiPatches.BackpackIsOpen = true;
        try
        {
            instance.Show(backpackContainer, activeGroup);
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
        
        try
        {
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
            player.RemoveEquipAction(backpack.Item);
            player.UnequipItem(backpack.Item, true);

            if (!player.m_inventory.RemoveItem(backpack.Item))
            {
                if (swapItemActivated)
                    playerInventory.AddItem(tempItemRemoval);
                player.Message(MessageHud.MessageType.Center, "$vapok_mod_quick_drop_unavailable");
                return;
            }

            if (backpack.Item != null)
                backpack.Item.m_customData ??= new System.Collections.Generic.Dictionary<string, string>();

            ItemDrop itemDrop = ItemDrop.DropItem(backpack.Item, 1, player.transform.position - player.transform.forward + player.transform.up, player.transform.rotation);
            if (itemDrop != null)
            {
                Rigidbody rb = itemDrop.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.linearVelocity = (Vector3.up - player.transform.forward) * 5f;
            }

            player.m_dropEffects.Create(player.transform.position, Quaternion.identity);

            if (swapItemActivated)
                playerInventory.AddItem(tempItemRemoval);
            
            InventoryGuiPatches.BackpackIsOpen = false;
        }
        catch (System.Exception ex)
        {
            AdventureBackpacks.Log?.Warning($"Error during QuickDropBackpack: {ex.Message}");
        }
        finally
        {
            AdventureBackpacks.QuickDropping = false;
        }
    }
}