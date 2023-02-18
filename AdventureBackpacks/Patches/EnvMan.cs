using AdventureBackpacks.Assets.Factories;
using HarmonyLib;
using JetBrains.Annotations;

namespace AdventureBackpacks.Patches;

public class EnvManPatches
{
    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.IsCold))]
    public static class EnvManIsCold
    {
        [UsedImplicitly]
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(EnvMan __instance, ref bool __result)
        {
            if (Player.m_localPlayer == null)
                return true;
            var effect = EffectsFactory.EffectList[BackpackEffect.ColdResistance];
            if (effect.IsEffectActive(Player.m_localPlayer))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
    
    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.IsWet))]
    public static class EnvManIsWet
    {
        [UsedImplicitly]
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(EnvMan __instance, ref bool __result)
        {
            if (Player.m_localPlayer == null)
                return true;

            var waterResistEffect = EffectsFactory.EffectList[BackpackEffect.WaterResistance];
            if (waterResistEffect.IsEffectActive(Player.m_localPlayer))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}