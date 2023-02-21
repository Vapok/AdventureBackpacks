using HarmonyLib;

namespace AdventureBackpacks.Patches;

public static class ContainerPatches
{
    [HarmonyPatch(typeof(Container), nameof(Container.TakeAll))]
    static class ContainerTakeAllPatch
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