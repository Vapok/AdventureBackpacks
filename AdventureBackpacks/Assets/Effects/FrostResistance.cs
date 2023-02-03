using AdventureBackpacks.Assets.Factories;
using AdventureBackpacks.Extensions;
using HarmonyLib;
using JetBrains.Annotations;

namespace AdventureBackpacks.Assets.Effects;

public class FrostResistance : EffectsBase
{
    public FrostResistance(string effectName, string effectDesc) : base(effectName, effectDesc)
    {
    }

    public static bool ShouldHaveFrostResistance(ItemDrop.ItemData itemData)
    {
        var effect = EffectsFactory.EffectList[BackpackEffect.FrostResistance];
        
        if (!effect.EnabledEffect.Value)
            return false;

        if (itemData != null && itemData.TryGetBackpackItem(out var backpack))
        {
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
}