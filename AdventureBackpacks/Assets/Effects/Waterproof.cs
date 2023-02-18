using AdventureBackpacks.Assets.Factories;
using AdventureBackpacks.Extensions;

namespace AdventureBackpacks.Assets.Effects;

public class Waterproof: EffectsBase
{
    public Waterproof(string effectName, string effectDesc) : base(effectName, effectDesc)
    {
    }
    
    public override bool IsEffectActive(Humanoid human)
    {
        if (human is Player player)
        {
            var equippedBackpack = player.GetEquippedBackpack();
            
            if (equippedBackpack == null || !EnabledEffect.Value)
                return false;
            
            var itemData = equippedBackpack.Item;
            
            itemData.TryGetBackpackItem(out var backpack);

            var backpackBiome = backpack.BackpackBiome.Value;

            if (BiomeQualityLevels.ContainsKey(backpackBiome))
            {
                var configQualityForBiome = BiomeQualityLevels[backpackBiome].Value;

                if (configQualityForBiome == 0 || backpackBiome == BackpackBiomes.None)
                    return false;
                
                return itemData.m_quality >= configQualityForBiome;  
            }
        }
        return false;
    }
}
