using AdventureBackpacks.Extensions;
using AdventureBackpacks.Features;
using HarmonyLib;

namespace AdventureBackpacks.Patches;

internal static class HudPatches
{
    [HarmonyPatch(typeof(Hud), "SetupPieceInfo")]
    static class HudSetupPieceInfoPatch
    {
        [HarmonyPrepare]
        private static bool Prepare() => !PlayerExtensions.IsDedicatedOrHeadless();

        [HarmonyPrefix]
        [HarmonyPriority(900)]
        static void Prefix()
        {
            CraftingContext.Enter();
        }

        [HarmonyFinalizer]
        [HarmonyPriority(100)]
        static void Finalizer()
        {
            CraftingContext.Exit();
        }
    }
}
