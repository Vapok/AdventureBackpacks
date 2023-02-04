using AdventureBackpacks.Assets.Factories;
using AdventureBackpacks.Extensions;
using HarmonyLib;
using JetBrains.Annotations;

namespace AdventureBackpacks.Assets.Effects;

public class Demister: EffectsBase
{
    public Demister(string effectName, string effectDesc) : base(effectName, effectDesc)
    {
    }

    public static bool ShouldHaveDemister(Humanoid human)
    {
        var effect = EffectsFactory.EffectList[BackpackEffect.Demister];
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
    
    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UpdateEquipmentStatusEffects))]
    public static class DemisterHumanoidUpdateEquipmentStatusEffectsPatch
    {
        [UsedImplicitly]
        public static void Postfix(Humanoid __instance)
        {
            if (__instance is Player player)
            {
                var deMister = ObjectDB.instance.GetStatusEffect("Demister");
                if (deMister == null)
                {
                    return;
                }
                
                var shouldHaveDemister = ShouldHaveDemister(__instance);
                var hasDemister = player.m_eqipmentStatusEffects.Contains(deMister);
                
                if (hasDemister && shouldHaveDemister)
                    return;
                
                if (!hasDemister && shouldHaveDemister)
                {
                    player.m_eqipmentStatusEffects.Add(deMister);
                    player.m_seman.AddStatusEffect(deMister);
                }
            }
        }
    }
    
}