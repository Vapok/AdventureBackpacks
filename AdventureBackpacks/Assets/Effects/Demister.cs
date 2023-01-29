using System.Collections.Generic;
using System.Linq;
using AdventureBackpacks.Assets.Factories;
using AdventureBackpacks.Extensions;
using BepInEx.Configuration;
using HarmonyLib;
using JetBrains.Annotations;
using Vapok.Common.Managers.Configuration;

namespace AdventureBackpacks.Assets.Effects;

public static class Demister
{

    public static class Configuration
    {
        public static ConfigEntry<bool> EnabledEffect { get; private set;}
        public static ConfigEntry<int> QualityLevel { get; private set;}
        public static ConfigEntry<BackpackBiomes> EffectBiome { get; private set;}

        public static void RegisterEffectConfiguration(ConfigFile config)
        {
            var _configSection = $"Effect: Demister";
            
            EnabledEffect = ConfigSyncBase.SyncedConfig(_configSection, "Effect Enabled", true,
                new ConfigDescription("Enables the effect.",
                    null, // range between 0f and 1f will make it display as a percentage slider
                    new ConfigAttributes { IsAdminOnly = true, Order = 1 }));

            QualityLevel = ConfigSyncBase.SyncedConfig(_configSection, "Carry Bonus", 4,
                new ConfigDescription("Increases your carry capacity by this much (multiplied by item level) while wearing the backpack.",
                    new AcceptableValueRange<int>(1, 4),
                    new ConfigAttributes { IsAdminOnly = true, Order = 2 }));
            
            EffectBiome = ConfigSyncBase.SyncedConfig(_configSection, "Backpack Biome", BackpackBiomes.Mistlands,
                new ConfigDescription("The Backpack Biome this effect will apply it's effects on.",
                    null, 
                    new ConfigAttributes { IsAdminOnly = true, Order = 3 }));

        }

    }

    public static bool ShouldHaveDemister(Humanoid human)
    {
        if (human is Player player)
        {
            var equippedBackpack = player.GetEquippedBackpack();
            
            if (equippedBackpack == null || !Configuration.EnabledEffect.Value)
                return false;
            
            var itemData = equippedBackpack.Item;
            
            itemData.TryGetBackpackItem(out var backpack);
                
            return backpack.BackpackBiome.Value == Configuration.EffectBiome.Value && itemData.m_quality >= Configuration.QualityLevel.Value;  
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
                
                var shouldHaveDemister = ShouldHaveDemister(__instance) || EquipmentEffectCache.HasStatusEffect(deMister);
                
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