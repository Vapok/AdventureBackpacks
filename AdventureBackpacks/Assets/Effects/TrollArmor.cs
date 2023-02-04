using AdventureBackpacks.Assets.Factories;

namespace AdventureBackpacks.Assets.Effects;

public class TrollArmor : EffectsBase
{
    public TrollArmor(string effectName, string effectDesc) : base(effectName, effectDesc)
    {
    }

    public static bool ShouldHaveTrollArmorSet(ItemDrop.ItemData itemData)
    {
        var effect = EffectsFactory.EffectList[BackpackEffect.TrollArmor];
        
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