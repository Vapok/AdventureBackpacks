using AdventureBackpacks.Assets.Factories;
using AdventureBackpacks.Extensions;

namespace AdventureBackpacks.Assets.Effects;

public class FrostResistance : EffectsBase
{
    public static HitData.DamageModPair EffectMod = new() { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Resistant};
    public FrostResistance(string effectName, string effectDesc) : base(effectName, effectDesc)
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
            return IsEffectActive(itemData);
        }

        return false;
    }
    public override bool IsEffectActive(ItemDrop.ItemData itemData)
    {
        if (!EnabledEffect.Value)
            return false;

        if (itemData != null && itemData.TryGetBackpackItem(out var backpack))
        {
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