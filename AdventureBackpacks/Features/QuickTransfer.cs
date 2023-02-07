using AdventureBackpacks.Configuration;
using AdventureBackpacks.Extensions;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using Vapok.Common.Managers.Configuration;
using Vapok.Common.Shared;

namespace AdventureBackpacks.Features;

public static class QuickTransfer
{
    public static bool FeatureInitialized = false;
    public static ConfigEntry<bool> EnableQuickTransfer { get; private set;}
    
    static QuickTransfer()
    {
        ConfigRegistry.Waiter.StatusChanged += (_, _) => RegisterConfiguraitonFile();
    }

    private static void RegisterConfiguraitonFile()
    {
        EnableQuickTransfer = ConfigSyncBase.UnsyncedConfig("Local Config", "Enable Quick Right Click Item Transfer", false,
            new ConfigDescription("When enabled, can move items to/from player inventory to container, by right clicking.",
                null,
                new ConfigurationManagerAttributes { Order = 5 }));

    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnRightClickItem))]
    static class OnRightClickItemPatch
    {
        static bool Prefix(InventoryGui __instance, InventoryGrid grid, ItemDrop.ItemData item)
        {
            if (!FeatureInitialized)
                return true;
            
            if (Player.m_localPlayer == null || __instance == null || item == null)
                return false;

            if (__instance.m_currentContainer == null || !__instance.IsContainerOpen() || !EnableQuickTransfer.Value)
                return true;

            if (Chainloader.PluginInfos.ContainsKey("blumaye.quicktransfer"))
            {
                AdventureBackpacks.Log.Warning("blumaye.quicktransfer mod is enabled. Adventure Backpack's Quick Transfer disabled.");
                return true;
            }

            if (item.m_equiped)
                return true;

            var containerInventory = __instance.m_currentContainer.GetInventory();
            
            if (item.IsBackpack() && containerInventory.IsBackPackInventory())
                return true;
            
            var playerInventory = Player.m_localPlayer.GetInventory();

            if (playerInventory == null || containerInventory == null || grid == null)
                return true;

            Inventory fromInventory;
            Inventory toInventory;

            if (grid.m_inventory == containerInventory)
            {
                fromInventory = containerInventory;
                toInventory = playerInventory;
            }
            else
            {
                fromInventory = playerInventory;
                toInventory = containerInventory;
            }

            toInventory.MoveItemToThis(fromInventory, item);
            __instance.m_moveItemEffects.Create(__instance.transform.position, Quaternion.identity);

            return false;
        }
    }
}