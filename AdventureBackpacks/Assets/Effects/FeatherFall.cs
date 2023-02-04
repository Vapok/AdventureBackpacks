using AdventureBackpacks.Assets.Factories;
using AdventureBackpacks.Extensions;
using HarmonyLib;
using JetBrains.Annotations;

namespace AdventureBackpacks.Assets.Effects;

public class FeatherFall : EffectsBase
{
    public FeatherFall(string effectName, string effectDesc) : base(effectName, effectDesc)
    {
    }
    
    public static bool ShouldHaveFeatherFall(Humanoid human)
    {
        var effect = EffectsFactory.EffectList[BackpackEffect.FeatherFall];
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
    [HarmonyBefore(new string[]{"randyknapp.mods.epicloot"})]
    public static class FeatherFall_Humanoid_UpdateEquipmentStatusEffects_Patch
    {
        [UsedImplicitly]
        public static void Postfix(Humanoid __instance)
        {
            if (__instance is Player player)
            {
                var slowFall = ObjectDB.instance.GetStatusEffect("SlowFall");
                if (slowFall == null)
                {
                    return;
                }

                var shouldHaveFeatherFall = ShouldHaveFeatherFall(__instance);  
                var hasFeatherFall = player.m_eqipmentStatusEffects.Contains(slowFall);
                
                if (hasFeatherFall && shouldHaveFeatherFall)
                    return;
                
                if (!hasFeatherFall && shouldHaveFeatherFall)
                {
                    player.m_eqipmentStatusEffects.Add(slowFall);
                    player.m_seman.AddStatusEffect(slowFall);
                }
            }
        }
    }
}


