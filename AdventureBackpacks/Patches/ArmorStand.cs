using HarmonyLib;

namespace AdventureBackpacks.Patches;

public static class ArmorStandPatches
{
    [HarmonyPatch(typeof(ArmorStand), nameof(ArmorStand.UpdateAttach))]
    static class ArmorStandUpdateAttachPatch
    {
        static void Prefix(ArmorStand __instance)
        {
            AdventureBackpacks.BypassMoveProtection = true;
        }
        static void Postfix(ArmorStand __instance)
        {
            AdventureBackpacks.BypassMoveProtection = false;
        }
    }

}