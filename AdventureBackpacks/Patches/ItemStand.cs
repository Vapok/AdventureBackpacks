using HarmonyLib;

namespace AdventureBackpacks.Patches;

public static class ItemStandStandPatches
{
    [HarmonyPatch(typeof(ItemStand), nameof(ItemStand.UpdateAttach))]
    static class ItemStandUpdateAttachPatch
    {
        static void Prefix(ItemStand __instance)
        {
            AdventureBackpacks.BypassMoveProtection = true;
        }
        static void Postfix(ItemStand __instance)
        {
            AdventureBackpacks.BypassMoveProtection = false;
        }
    }

}