using AdventureBackpacks.Components;
using AdventureBackpacks.Extensions;
using HarmonyLib;

namespace AdventureBackpacks.Patches;

public static class DoorPatches
{
    [HarmonyPatch(typeof(Door), nameof(Door.HaveKey))]
    static class HaveDoorKeyPatch
    {
        [HarmonyPrepare]
        private static bool Prepare() => !PlayerExtensions.IsDedicatedOrHeadless();

        static void Postfix(Door __instance, Humanoid player, bool matchWorldLevel, ref bool __result)
        {
            if (__instance == null || __result)
                return;

            Player targetPlayer = (player as Player) ?? Player.m_localPlayer;
            if (targetPlayer == null || !targetPlayer.IsBackpackEquipped())
                return;

            if (__instance.m_keyItem == null)
            {
                __result = true;
                return;
            }

            string keyName = __instance.m_keyItem.m_itemData?.m_shared?.m_name;
            if (string.IsNullOrEmpty(keyName))
                return;

            BackpackComponent backpack = targetPlayer.GetEquippedBackpack();
            Inventory bpInventory = backpack?.GetInventory();
            if (bpInventory != null && bpInventory.HaveItem(keyName, matchWorldLevel))
            {
                __result = true;
            }
        }
    }
}