using System;
using System.Collections.Generic;
using AdventureBackpacks.Assets.Factories;
using AdventureBackpacks.Configuration;
using AdventureBackpacks.Extensions;
using BepInEx.Configuration;
using HarmonyLib;
using JetBrains.Annotations;
using Vapok.Common.Managers.Configuration;
using Vapok.Common.Shared;

namespace AdventureBackpacks.Assets.Effects;

public class Waterproof: EffectsBase
{
    public Waterproof(string effectName, string effectDesc) : base(effectName, effectDesc)
    {
    }
    
    public static bool ShouldHaveWaterproof(Humanoid human)
    {
        var effect = EffectsFactory.EffectList[BackpackEffect.WaterResistance];
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
    
    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.IsWet))]
    public static class UpdateStatusEffects
    {
        [UsedImplicitly]
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(EnvMan __instance, ref bool __result)
        {
            if (Player.m_localPlayer == null)
                return true;
            
            if (ShouldHaveWaterproof(Player.m_localPlayer))
            {
                __result = false;
                return false;
            }
            
            return true;
        }
    }
}
