using AdventureBackpacks.Configuration;
using AdventureBackpacks.Extensions;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
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
        static bool Prefix(InventoryGui __instance, ItemDrop.ItemData item)
        {
            if (Player.m_localPlayer == null || __instance == null || item == null)
                return false;

            if (__instance.m_currentContainer == null || !__instance.IsContainerOpen() || !EnableQuickTransfer.Value)
                return true;

            if (Chainloader.PluginInfos.ContainsKey("blumaye.quicktransfer"))
            {
                AdventureBackpacks.Log.Warning("blumaye.quicktransfer mod is enabled. Adventure Loot's Quick Transfer disabled.");
                return true;
            }

            if (item.m_equiped)
                return true;

            var containerInventory = __instance.m_currentContainer.GetInventory();
            
            if (item.IsBackpack() && containerInventory.IsBackPackInventory())
                return true;
            
            var playerInventory = Player.m_localPlayer.GetInventory();
            
            var itemMoved = true;

            if (playerInventory == null || containerInventory == null)
                return true;

            if (playerInventory.ContainsItem(item) && containerInventory.HaveEmptySlot())
                containerInventory.MoveItemToThis(playerInventory, item);
            else if (containerInventory.ContainsItem(item) && playerInventory.HaveEmptySlot())
                playerInventory.MoveItemToThis(containerInventory, item);
            else
                itemMoved = false;

            return !itemMoved;
        }
    }
}