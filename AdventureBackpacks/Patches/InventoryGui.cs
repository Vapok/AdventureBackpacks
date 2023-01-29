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
    public static bool BackpackIsOpen = false;
    public static bool BackpackIsOpening = false;
    public static bool BackpackEquipped = false;
    
    //Upgrade Variables
    public static bool DoingUpgrade = false;

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.DoCrafting))]
    static class InventoryGuiDoCraftingPrefix
    {
        static void Prefix(InventoryGui __instance)
        {
            if (__instance == null)
                return;
            if (__instance.m_craftUpgradeItem != null || (__instance.m_craftRecipe != null && __instance.m_craftRecipe.m_item != null))
            {
                DoingUpgrade = true;
            }
        }

        static void Postfix(InventoryGui __instance)
        {
            if (__instance.m_craftUpgradeItem != null && __instance.m_craftUpgradeItem.IsBackpack())
            {
                var backpack = __instance.m_craftUpgradeItem.Data().Get<BackpackComponent>();
                backpack?.Load();
                Player.m_localPlayer.UpdateEquipmentStatusEffects();
            }
            DoingUpgrade = false;
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
                throw __exception;
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