using AdventureBackpacks.Configuration;
using AdventureBackpacks.Extensions;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using PlayFab.Internal;
using UnityEngine;
using Vapok.Common.Managers.Configuration;
using Vapok.Common.Shared;

namespace AdventureBackpacks.Features;

public static class QuickTransfer
{
    public static bool FeatureInitialized = false;
    public static ConfigEntry<bool> EnableQuickTransfer;

    private static InventoryGui _inventoryGuiInstance;
    private static Inventory _fromInventory;
    private static Inventory _toInventory;

    private static bool _processingRightClick;

    static QuickTransfer()
    {
        ConfigRegistry.Waiter.StatusChanged += (_, _) => RegisterConfiguraitonFile();
    }

    private static void RegisterConfiguraitonFile()
    {
        ConfigSyncBase.UnsyncedConfig("Local Config", "Enable Quick Right Click Item Transfer", false,
            new ConfigDescription("When enabled, can move items to/from player inventory to container, by right clicking.",
                null,
                new ConfigurationManagerAttributes { Order = 5 }),ref EnableQuickTransfer);
    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnRightClickItem))]
    [HarmonyPriority(Priority.First)]
    static class OnRightClickItemPatch
    {
        static void Prefix(InventoryGui __instance, InventoryGrid grid, ItemDrop.ItemData item)
        {
            if (!FeatureInitialized)
                return;

            if (Player.m_localPlayer == null || __instance == null || item == null)
                return;

            if (__instance.m_currentContainer == null || !__instance.IsContainerOpen() || !EnableQuickTransfer.Value)
                return;

            if (Chainloader.PluginInfos.ContainsKey("blumaye.quicktransfer"))
            {
                AdventureBackpacks.Log.Warning(
                    "blumaye.quicktransfer mod is enabled. Adventure Backpack's Quick Transfer disabled.");
                return;
            }

            //If item is equipped, let's skip the move, because this is most likely an unequip action.
            if (item.m_equipped)
                return;

            //If I have a backpack open, and I currently have nothing in the equipped slot, I want to prioritize equipping it over storing it.
            if (item.IsEquipable() && grid.m_inventory == Player.m_localPlayer.GetInventory())
            {
                switch (item.m_shared.m_itemType)
                {
                    case ItemDrop.ItemData.ItemType.Helmet:
                        if (Player.m_localPlayer.m_helmetItem == null)
                            return;
                        break;
                    case ItemDrop.ItemData.ItemType.Chest:
                        if (Player.m_localPlayer.m_chestItem == null)
                            return;
                        break;
                    case ItemDrop.ItemData.ItemType.Legs:
                        if (Player.m_localPlayer.m_legItem == null)
                            return;
                        break;
                    case ItemDrop.ItemData.ItemType.Bow:
                    case ItemDrop.ItemData.ItemType.TwoHandedWeapon:
                    case ItemDrop.ItemData.ItemType.OneHandedWeapon:
                        if (Player.m_localPlayer.m_rightItem == null)
                            return;
                        break;
                    case ItemDrop.ItemData.ItemType.Shoulder:
                        if (Player.m_localPlayer.m_shoulderItem == null)
                            return;
                        break;
                    case ItemDrop.ItemData.ItemType.Utility:
                        if (Player.m_localPlayer.m_utilityItem == null)
                            return;
                        break;
                    case ItemDrop.ItemData.ItemType.Shield:
                        if (Player.m_localPlayer.m_leftItem == null)
                            return;
                        break;
                }
            }

            var containerInventory = __instance.m_currentContainer.GetInventory();
            
            if (item.IsBackpack() && containerInventory.IsBackPackInventory())
                return;
            
            var playerInventory = Player.m_localPlayer.GetInventory();

            if (playerInventory == null || containerInventory == null || grid == null)
                return;

            _inventoryGuiInstance = __instance;
            
            if (grid.m_inventory == containerInventory)
            {
                _fromInventory = containerInventory;
                _toInventory = playerInventory;
            }
            else
            {
                _fromInventory = playerInventory;
                _toInventory = containerInventory;
            }

            _processingRightClick = true;
        }

        static void Postfix()
        {
            _processingRightClick = false;
            _toInventory = null;
            _fromInventory = null;
            _inventoryGuiInstance = null;
        }
    }

    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UseItem))]
    [HarmonyPriority(Priority.First)]
    static class UseItemPatch
    {
        static bool Prefix(ItemDrop.ItemData item)
        {
            if (!_processingRightClick)
                return true;

            _toInventory.MoveItemToThis(_fromInventory, item);
            _inventoryGuiInstance.m_moveItemEffects.Create(_inventoryGuiInstance.transform.position, Quaternion.identity);

            return false;
        }
    }
}