using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using JetBrains.Annotations;

namespace AdventureBackpacks.Assets.Effects;

public static class BackpackEffects
{
    public static HitData.DamageModPair FrostResistance = new() { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Resistant};
}

public enum EquipmentEffects
{
    SlowFall,
}

public static class EquipmentEffectCache
{
    public static ConditionalWeakTable<Player, Dictionary<string, float?>> EquippedValues = new ConditionalWeakTable<Player, Dictionary<string, float?>>();

    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UnequipItem))]
    public static class EquipmentEffectCache_Humanoid_UnequipItem_Patch
    {
        [UsedImplicitly]
        public static void Prefix(Humanoid __instance)
        {
            if (__instance is Player player)
            {
                Reset(player);
            }
        }
    }

    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.EquipItem))]
    public static class EquipmentEffectCache_Humanoid_EquipItem_Patch
    {
        [UsedImplicitly]
        public static void Prefix(Humanoid __instance)
        {
            if (__instance is Player player)
            {
                Reset(player);
            }
        }
    }

    public static void Reset(Player player)
    {
        EquippedValues.Remove(player);
    }

    public static float? Get(Player player, string effect, Func<float?> calculate)
    {
        var values = EquippedValues.GetOrCreateValue(player);
        if (values.TryGetValue(effect, out float? value))
        {
            return value;
        }

        return values[effect] = calculate();
    }
}
