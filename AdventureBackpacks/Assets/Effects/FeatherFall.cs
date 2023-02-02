﻿using System;
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

public static class FeatherFall
{
    public static class Configuration
    {
        public static ConfigEntry<bool> EnabledEffect { get; private set;}
        public static Dictionary<BackpackBiomes,ConfigEntry<int>> BiomeQualityLevels { get; private set;}

        public static string _configSection = $"Effect: Feather Fall";

        public static void RegisterEffectConfiguration()
        {
            BiomeQualityLevels = new();
            
            EnabledEffect = ConfigSyncBase.SyncedConfig(_configSection, "Effect Enabled", true,
                new ConfigDescription("Enables the effect.",
                    null, // range between 0f and 1f will make it display as a percentage slider
                    new ConfigurationManagerAttributes { Order = 1 }));
            
            //Waiting For Startup
            ConfigRegistry.Waiter.StatusChanged += FillBiomeSettings;
        }

        private static void FillBiomeSettings(object sender, EventArgs e)
        {
            foreach (BackpackBiomes backpackBiome in Enum.GetValues(typeof(BackpackBiomes)))
            {
                RegisterEffectBiomeQuality(backpackBiome);
            }
        }
        public static void RegisterEffectBiomeQuality(BackpackBiomes biome, int defaultQuality = 0)
        {
            if (biome == BackpackBiomes.None)
                return;
            
            var qualityLevel = ConfigSyncBase.SyncedConfig(_configSection, $"Effective Quality Level: {biome.ToString()}", defaultQuality,
                new ConfigDescription("Quality Level needed to apply effect to backpack. Zero disables effect for Biome.",
                    new AcceptableValueRange<int>(0, 5),
                    new ConfigurationManagerAttributes { Order = 2 }));
            
            if (!BiomeQualityLevels.ContainsKey(biome))
            { 
                BiomeQualityLevels.Add(biome, qualityLevel);
            }
        }
    }
    
    public static bool ShouldHaveFeatherFall(Humanoid human)
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


