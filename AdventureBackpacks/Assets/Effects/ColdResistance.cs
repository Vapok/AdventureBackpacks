using AdventureBackpacks.Assets.Factories;
using AdventureBackpacks.Extensions;
using HarmonyLib;
using JetBrains.Annotations;

namespace AdventureBackpacks.Assets.Effects;

public class ColdResistance : EffectsBase
{
    public ColdResistance(string effectName, string effectDesc) : base(effectName, effectDesc)
    {
    }

    public static bool ShouldHaveColdResistance(Humanoid human)
    {
        var effect = EffectsFactory.EffectList[BackpackEffect.ColdResistance];
        if (human is Player player)
        {
            var equippedBackpack = player.GetEquippedBackpack();
            
            if (equippedBackpack == null || !effect.EnabledEffect.Value)
                return false;
            
            var itemData = equippedBackpack.Item;
            
            itemData.TryGetBackpackItem(out var backpack);

            var backpackBiome = backpack.BackpackBiome.Value;

            if (effect.BiomeQualityLevels.ContainsKey(backpackBiome))
            {
                var configQualityForBiome = effect.BiomeQualityLevels[backpackBiome].Value;

                if (configQualityForBiome == 0 || backpackBiome == BackpackBiomes.None)
                    return false;
                
                return itemData.m_quality >= configQualityForBiome;  
            }
        }

        return false;
    }
    
    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.IsCold))]
    public static class UpdateStatusEffects
    {
        [UsedImplicitly]
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(EnvMan __instance, ref bool __result)
        {
            if (Player.m_localPlayer == null)
                return true;
            
            if (ShouldHaveColdResistance(Player.m_localPlayer))
            {
                __result = false;
                return false;
            }
            
            return true;
        }
    }
}