using AdventureBackpacks.Extensions;
using HarmonyLib;

namespace AdventureBackpacks.Patches;

public class FejdStartupPatches
{
    [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Start))]
    [HarmonyAfter("org.bepinex.helpers.LocalizationManager")]
    [HarmonyBefore("org.bepinex.helpers.ItemManager")]
    public static class FejdStartupAwakePatch
    {
        [HarmonyPrepare]
        private static bool Prepare() => !PlayerExtensions.IsDedicatedOrHeadless();

        static void Postfix()
        {
            AdventureBackpacks.Waiter.ValheimIsAwake(true);
        }
    }
}