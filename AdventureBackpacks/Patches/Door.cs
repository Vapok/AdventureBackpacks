using AdventureBackpacks.Extensions;
using HarmonyLib;

namespace AdventureBackpacks.Patches;

public static class DoorPatches
{
    [HarmonyPatch(typeof(Door), nameof(Door.HaveKey))]
    static class HaveDoorKeyPatch
    {
        static void Postfix(Door __instance, ref bool __result)
        {
            if (__instance == null || Player.m_localPlayer == null)
                return;

            if (Player.m_localPlayer.IsBackpackEquipped() && __result == false)
            {
                var backpack = Player.m_localPlayer.GetEquippedBackpack();
                __result = (__instance.m_keyItem == null || backpack.GetInventory().HaveItem(__instance.m_keyItem.m_itemData.m_shared.m_name));
            }
        }
    }
}