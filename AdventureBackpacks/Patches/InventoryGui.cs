using System;
using AdventureBackpacks.Assets;
using AdventureBackpacks.Components;
using AdventureBackpacks.Configuration;
using AdventureBackpacks.Extensions;
using HarmonyLib;
using UnityEngine;
using Vapok.Common.Managers;
using Vapok.Common.Tools;

namespace AdventureBackpacks.Patches;

internal static class InventoryGuiPatches
{
    public static bool BackpackIsOpen;
    public static bool BackpackIsOpening;
    public static bool BackpackEquipped = false;

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.DoCrafting))]
    static class InventoryGuiDoCraftingPrefix
    {
        static void Postfix(InventoryGui __instance)
        {
            if (__instance.m_craftUpgradeItem != null && __instance.m_craftUpgradeItem.IsBackpack())
            {
                var backpack = __instance.m_craftUpgradeItem.Data().Get<BackpackComponent>();
                backpack?.Load();
                Player.m_localPlayer.UpdateEquipmentStatusEffects();
            }
        }
    }
    
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnSelectedItem))]
    static class InventoryGuiOnSelectedItem
    {
        static Exception Finalizer(Exception __exception, InventoryGrid grid, ItemDrop.ItemData item, Vector2i pos, InventoryGrid.Modifier mod, InventoryGui __instance)
        {

            if (__exception != null)
            {
                if (__exception is NullReferenceException)
                {
                    if (__instance != null && __instance.m_currentContainer == null && grid.GetInventory() != null && Player.m_localPlayer != null)
                    {
                        //Is item a backpack
                        if (Backpacks.BackpackTypes.Contains(item.m_shared.m_name))
                        {
                            Player.m_localPlayer.DropItem(Player.m_localPlayer.GetInventory(), item, 1);
                            BackpackIsOpen = false;
                            __instance.Hide();
                            Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "$vapok_mod_you_droped_bag");
                            
                            return null;
                        }
                    }
                }
                AdventureBackpacks.Log.Warning($"The following error was captured by Adventure Backpacks, but was caused by another mod. Advanced Backpacks is going to allow the operation to continue, but is going to replay the error below:");
                AdventureBackpacks.Log.Error($"External Mod Error Message: {__exception.Message}");
                AdventureBackpacks.Log.Error($"External Mod Error Source: {__exception.Source}");
                AdventureBackpacks.Log.Error($"External Mod Error Stack Trace: {__exception.StackTrace}");
                AdventureBackpacks.Log.Warning($"Please check with other mod authors listed above.");
            }
            return null;
        }
    }
    
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Update))]
    static class InventoryGuiUpdatePatch
    {
        static void Postfix(Animator ___m_animator, ref Container ___m_currentContainer)
        {
            if (!KeyPressTool.CheckKeyDown(ConfigRegistry.HotKeyOpen.Value) || !Player.m_localPlayer || !___m_animator.GetBool("visible"))
                return;

            if (BackpackIsOpening)
            {
                BackpackIsOpening = false;
                return;
            }
            
            if (BackpackIsOpen)
            {
                InventoryGui.instance.CloseContainer();
                BackpackIsOpen = false;
                
                if (ConfigRegistry.CloseInventory.Value)
                    InventoryGui.instance.Hide();
                
                return;
            }
            
            if (Player.m_localPlayer.CanOpenBackpack())
            {
                Player.m_localPlayer.OpenBackpack(false);
            }
        }
    }
}