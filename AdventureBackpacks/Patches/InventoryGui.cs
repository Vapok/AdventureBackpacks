using System;
using System.Runtime.InteropServices;
using AdventureBackpacks.Assets;
using AdventureBackpacks.Components;
using AdventureBackpacks.Configuration;
using HarmonyLib;
using UnityEngine;
using Vapok.Common.Tools;

namespace AdventureBackpacks.Patches;

internal class InventoryGuiPatches
{
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

            if (Backpacks.Opening)
            {
                Backpacks.Opening = false;
                return;
            }

            if (___m_currentContainer != null && ___m_currentContainer == Backpacks.BackpackContainer)
            {
                ___m_currentContainer = null;
            }

            else if (Backpacks.CanOpenBackpack())
            {
                Backpacks.OpenBackpack();
            }
        }
    }
}