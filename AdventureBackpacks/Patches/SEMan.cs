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
    [HarmonyPatch(typeof(SEMan), nameof(SEMan.RemoveStatusEffect), new[] { typeof(int), typeof(bool) })]
    public static class RemoveStatusEffects
    {
        [HarmonyPrepare]
        private static bool Prepare() => !PlayerExtensions.IsDedicatedOrHeadless();

        [UsedImplicitly]
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(int nameHash, ref bool __result)
        {
            if (EquipmentEffectCache.ActiveEffects == null)
                return true;
        
            if (EquipmentEffectCache.ActiveEffects.Any(x => x.NameHash().Equals(nameHash)))
            {
                __result = false;
                return false;
            }

            return true;
        }
    }

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

            if (nameHash != SEMan.s_statusEffectWet)
                return true;

            if (__instance.m_character != Player.m_localPlayer)
                return true;

            if (!EffectsFactory.EffectList.TryGetValue(BackpackEffect.WaterResistance, out EffectsBase effect) || effect == null)
                return true;

            if (effect.IsEffectActive(Player.m_localPlayer))
            {
                __result = null;
                return false;
            }

            return true;
        }
    }
}