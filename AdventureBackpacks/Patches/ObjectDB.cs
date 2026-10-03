using AdventureBackpacks.Assets.Factories;
using HarmonyLib;

namespace AdventureBackpacks.Patches;

internal static class ObjectDBPatches
{
    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.CopyOtherDB))]
    static class ObjectDBCopyOtherDBPatch
    {
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Postfix(ObjectDB __instance)
        {
            BackpackFactory.ApplyUpgraderResources(__instance);
        }
    }

    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    static class ObjectDBAwakePatch
    {
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Postfix(ObjectDB __instance)
        {
            BackpackFactory.ApplyUpgraderResources(__instance);
        }
    }
}
