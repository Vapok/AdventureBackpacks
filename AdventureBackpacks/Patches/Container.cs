using System.Collections.Generic;
using System.Linq;
using AdventureBackpacks.Extensions;
using HarmonyLib;
using UnityEngine;

namespace AdventureBackpacks.Patches;

public static class ContainerPatches
{
    public static bool IsBackpackProxy(this Container container)
    {
        if (container == null || container.gameObject == null) return false;
        return container.gameObject.name.StartsWith(PlayerExtensions.BackpackProxyName) ||
               container.gameObject.name.Equals("Player(Clone)") ||
               container.GetComponentInParent<Player>() != null;
    }

    [HarmonyPatch(typeof(Container), nameof(Container.TakeAll))]
    static class ContainerTakeAllPatch
    {
        static bool Prefix(Container __instance, Humanoid character, ref bool __result)
        {
            AdventureBackpacks.BypassMoveProtection = true;
            if (__instance.IsBackpackProxy())
            {
                var player = character as Player ?? Player.m_localPlayer;
                if (player != null && __instance.GetInventory() != null)
                {
                    player.GetInventory().MoveAll(__instance.GetInventory());
                }
                __result = true;
                return false;
            }
            return true;
        }

        static void Postfix(Container __instance)
        {
            AdventureBackpacks.BypassMoveProtection = false;
        }
    }

    [HarmonyPatch(typeof(Container), nameof(Container.Interact))]
    static class ContainerInteractPatch
    {
        static bool Prefix(Container __instance, ref bool __result)
        {
            if (__instance.IsBackpackProxy())
            {
                __result = false;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Container), nameof(Container.Save))]
    static class ContainerSavePatch
    {
        static bool Prefix(Container __instance)
        {
            if (__instance != null && __instance.IsBackpackProxy())
            {
                // Backpack items are saved via BackpackComponent/ItemData, not through the player ZDO's s_items field.
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Container), nameof(Container.Load))]
    static class ContainerLoadPatch
    {
        static bool Prefix(Container __instance, ref bool __result)
        {
            if (__instance != null && __instance.IsBackpackProxy())
            {
                // Backpack items are loaded via BackpackComponent/ItemData, not through the player ZDO's s_items field.
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Container), nameof(Container.Awake))]
    static class ContainerAwakePatch
    {
        static bool Prefix(Container __instance)
        {
            if (__instance.IsBackpackProxy())
            {
                // Suppress vanilla Container.Awake for the backpack UI proxy.
                // Prevents NullReferenceException on missing ZNetView, stops network RPC registrations,
                // and prevents CheckForChanges polling.
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Container), nameof(Container.IsOwner))]
    static class ContainerIsOwnerPatch
    {
        static bool Prefix(Container __instance, ref bool __result)
        {
            if (__instance.IsBackpackProxy())
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Container), nameof(Container.IsInUse))]
    static class ContainerIsInUsePatch
    {
        static bool Prefix(Container __instance, ref bool __result)
        {
            if (__instance.IsBackpackProxy())
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Container), nameof(Container.SetInUse))]
    static class ContainerSetInUsePatch
    {
        [HarmonyPriority(Priority.First)]
        static bool Prefix(Container __instance)
        {
            if (__instance != null && __instance.IsBackpackProxy())
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Container), nameof(Container.CheckAccess))]
    static class ContainerCheckAccessPatch
    {
        static bool Prefix(Container __instance, ref bool __result)
        {
            if (__instance.IsBackpackProxy())
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Container), nameof(Container.StackAll))]
    static class ContainerStackAllPatch
    {
        static bool Prefix(Container __instance)
        {
            if (__instance.IsBackpackProxy())
            {
                if (Player.m_localPlayer != null && __instance.GetInventory() != null)
                {
                    __instance.GetInventory().StackAll(Player.m_localPlayer.GetInventory());
                }
                return false;
            }
            return true;
        }
    }
}
