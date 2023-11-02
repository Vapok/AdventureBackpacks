using HarmonyLib;

namespace AdventureBackpacks.Patches;

public class FejdStartupPatches
{
    [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Start))]
    [HarmonyAfter("org.bepinex.helpers.LocalizationManager")]
    [HarmonyBefore("org.bepinex.helpers.ItemManager")]
    public static class FejdStartupAwakePatch
    {
        static void Postfix()
        {
            AdventureBackpacks.Waiter.ValheimIsAwake(true);
        }
    }
}