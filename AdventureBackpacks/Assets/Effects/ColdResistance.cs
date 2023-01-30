using System;
using System.Collections.Generic;
using AdventureBackpacks.Assets.Factories;
using AdventureBackpacks.Extensions;
using BepInEx.Configuration;
using HarmonyLib;
using JetBrains.Annotations;
using Vapok.Common.Managers.Configuration;

namespace AdventureBackpacks.Assets.Effects;


public static class ColdResistance
{
    public static class Configuration
    {
        public static ConfigEntry<bool> EnabledEffect { get; private set;}
        public static Dictionary<BackpackBiomes,ConfigEntry<int>> BiomeQualityLevels { get; private set;}
        public static ConfigEntry<BackpackBiomes> EffectBiome { get; private set;}
        
        public static string _configSection = $"Effect: Cold Resistance";

        public static void RegisterEffectConfiguration()
        {
            
            BiomeQualityLevels = new();
            
            EnabledEffect = ConfigSyncBase.SyncedConfig(_configSection, "Effect Enabled", true,
                new ConfigDescription("Enables the effect.",
                    null, // range between 0f and 1f will make it display as a percentage slider
                    new ConfigAttributes { IsAdminOnly = true, Order = 1 }));

            foreach (BackpackBiomes backpackBiome in Enum.GetValues(typeof(BackpackBiomes)))
            {
                RegisterEffectBiomeQuality(backpackBiome);
            }
        }

        public static void RegisterEffectBiomeQuality(BackpackBiomes biome, int defaultQuality = 1)
        {
            if (biome == BackpackBiomes.None)
                return;
            
            if (!BiomeQualityLevels.ContainsKey(biome))
            {
                var qualityLevel = ConfigSyncBase.SyncedConfig(_configSection, $"Effective Quality Level: {biome.ToString()}", defaultQuality,
                    new ConfigDescription("Quality Level needed to apply effect to backpack. Zero disables effect for Biome.",
                        new AcceptableValueRange<int>(0, 4),
                        new ConfigAttributes { IsAdminOnly = true, Order = 2 }));

                BiomeQualityLevels.Add(biome, qualityLevel);
            }
            else
            {
                BiomeQualityLevels[biome].Value = defaultQuality;
            }
                
        }

    }

    public static bool ShouldHaveColdResistance(Humanoid human)
    {
        if (human is Player player)
        {
            var equippedBackpack = player.GetEquippedBackpack();
            
            if (equippedBackpack == null || !Configuration.EnabledEffect.Value)
                return false;
            
            var itemData = equippedBackpack.Item;
            
            itemData.TryGetBackpackItem(out var backpack);

            var backpackBiome = backpack.BackpackBiome.Value;

            if (Configuration.BiomeQualityLevels.ContainsKey(backpackBiome))
            {
                var configQualityForBiome = Configuration.BiomeQualityLevels[backpackBiome].Value;

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