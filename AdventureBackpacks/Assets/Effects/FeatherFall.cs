using System.Linq;
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

        public static void RegisterEffectConfiguration(ConfigFile config)
        {
            var _configSection = $"Effect: Feather Fall";
            
            EnabledEffect = ConfigSyncBase.SyncedConfig(_configSection, "Effect Enabled", true,
                new ConfigDescription("Enables the effect.",
                    null, // range between 0f and 1f will make it display as a percentage slider
                    new ConfigAttributes { IsAdminOnly = true, Order = 1 }));

            QualityLevel = ConfigSyncBase.SyncedConfig(_configSection, "Carry Bonus", 3,
                new ConfigDescription("Increases your carry capacity by this much (multiplied by item level) while wearing the backpack.",
                    new AcceptableValueRange<int>(1, 4),
                    new ConfigAttributes { IsAdminOnly = true, Order = 2 }));
            
            EffectBiome = ConfigSyncBase.SyncedConfig(_configSection, "Backpack Biome", BackpackBiomes.Mistlands,
                new ConfigDescription("The Backpack Biome this effect will apply it's effects on.",
                    null, 
                    new ConfigAttributes { IsAdminOnly = true, Order = 3 }));

        }

    }

    
    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UpdateEquipmentStatusEffects))]
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
                
                EquipmentEffectCache.Reset(player);

                var equippedBackpack = Player.m_localPlayer.GetEquippedBackpack();

                if (equippedBackpack == null || !Configuration.EnabledEffect.Value)
                    return;
            
                var itemData = equippedBackpack.Item;
            
                itemData.TryGetBackpackItem(out var backpack);

                var shouldHaveFeatherFall = backpack.BackpackBiome.Value == Configuration.EffectBiome.Value && itemData.m_quality >= Configuration.QualityLevel.Value;  
                var hasFeatherFall = player.m_eqipmentStatusEffects.Contains(slowFall);
                if (!hasFeatherFall && shouldHaveFeatherFall)
                {
                    player.m_eqipmentStatusEffects.Add(slowFall);
                    player.m_seman.AddStatusEffect(slowFall);
                }
            }
        }
    }
    
}


