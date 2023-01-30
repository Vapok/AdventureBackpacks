using AdventureBackpacks.Assets.Factories;
using AdventureBackpacks.Extensions;
using BepInEx.Configuration;
using HarmonyLib;
using JetBrains.Annotations;
using Vapok.Common.Managers.Configuration;

namespace AdventureBackpacks.Assets.Effects;

public static class FeatherFall
{
    public static class Configuration
    {
        public static ConfigEntry<bool> EnabledEffect { get; private set;}
        public static ConfigEntry<int> QualityLevel { get; private set;}
        public static ConfigEntry<BackpackBiomes> EffectBiome { get; private set;}

        public static void RegisterEffectConfiguration()
        {
            var _configSection = $"Effect: Feather Fall";
            
            EnabledEffect = ConfigSyncBase.SyncedConfig(_configSection, "Effect Enabled", true,
                new ConfigDescription("Enables the effect.",
                    null, // range between 0f and 1f will make it display as a percentage slider
                    new ConfigAttributes { IsAdminOnly = true, Order = 1 }));

            QualityLevel = ConfigSyncBase.SyncedConfig(_configSection, "Effective Quality Level", 4,
                new ConfigDescription("Quality Level needed to apply effect to backpack.",
                    new AcceptableValueRange<int>(1, 4),
                    new ConfigAttributes { IsAdminOnly = true, Order = 2 }));
            
            EffectBiome = ConfigSyncBase.SyncedConfig(_configSection, "Backpack Biome", BackpackBiomes.Mistlands,
                new ConfigDescription("The Backpack Biome this effect will apply it's effects on.",
                    null, 
                    new ConfigAttributes { IsAdminOnly = true, Order = 3 }));

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
                
            return backpack.BackpackBiome.Value == Configuration.EffectBiome.Value && itemData.m_quality >= Configuration.QualityLevel.Value;  
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


