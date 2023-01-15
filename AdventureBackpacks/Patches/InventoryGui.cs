using AdventureBackpacks.Assets;
using AdventureBackpacks.Configuration;
using HarmonyLib;
using UnityEngine;
using Vapok.Common.Tools;

namespace AdventureBackpacks.Patches;

internal class InventoryGuiPatches
{
    // This patch is from Aedenthorn's BackpackRedux.
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