using System.Linq;
using AdventureBackpacks.Assets.Effects;
using AdventureBackpacks.Assets.Factories;
using AdventureBackpacks.Extensions;
using AdventureBackpacks.Features;
using HarmonyLib;
using JetBrains.Annotations;

namespace AdventureBackpacks.Patches;

public static class SEManPatches
{

    [HarmonyPatch(typeof(SEMan), nameof(SEMan.AddStatusEffect), new[] { typeof(int), typeof(bool), typeof(int), typeof(float), typeof(short) })]
    internal static class AddStatusEffectPatch
    {
        [HarmonyPrepare]
        private static bool Prepare() => !PlayerExtensions.IsDedicatedOrHeadless();

        [UsedImplicitly]
        private static bool Prefix(SEMan __instance, int nameHash, ref StatusEffect __result)
        {
            if (!PlayerPatches.PlayerUpdateEnvStatusEffectsPatch.IsUpdatingEnvStatusEffects)
                return true;

            if (__instance.m_character != Player.m_localPlayer)
                return true;

            if (nameHash == SEMan.s_statusEffectWet)
            {
                if (EffectsFactory.EffectList.TryGetValue(BackpackEffect.WaterResistance, out EffectsBase waterEffect) && waterEffect != null && waterEffect.IsEffectActive(Player.m_localPlayer))
                {
                    __result = null;
                    return false;
                }
            }
            else if (nameHash == SEMan.s_statusEffectCold)
            {
                if (EffectsFactory.EffectList.TryGetValue(BackpackEffect.ColdResistance, out EffectsBase coldEffect) && coldEffect != null && coldEffect.IsEffectActive(Player.m_localPlayer))
                {
                    __result = null;
                    return false;
                }
            }

            return true;
        }
    }
}