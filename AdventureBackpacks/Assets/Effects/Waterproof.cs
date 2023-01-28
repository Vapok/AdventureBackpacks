using AdventureBackpacks.Assets.Factories;
using AdventureBackpacks.Extensions;
using BepInEx.Configuration;
using HarmonyLib;
using Vapok.Common.Managers.Configuration;

namespace AdventureBackpacks.Assets.Effects;

public static class Waterproof
{
    public static int AddingStatusFromEnv;

    public static class Configuration
    {
        public static ConfigEntry<bool> EnabledEffect { get; private set;}
        public static ConfigEntry<int> QualityLevel { get; private set;}
        public static ConfigEntry<BackpackBiomes> EffectBiome { get; private set;}

        public static void RegisterEffectConfiguration(ConfigFile config)
        {
            var _configSection = $"Effect: Waterproof";
            
            EnabledEffect = ConfigSyncBase.SyncedConfig(_configSection, "Effect Enabled", true,
                new ConfigDescription("Enables the effect.",
                    null, // range between 0f and 1f will make it display as a percentage slider
                    new ConfigAttributes { IsAdminOnly = true, Order = 1 }));

            QualityLevel = ConfigSyncBase.SyncedConfig(_configSection, "Carry Bonus", 3,
                new ConfigDescription("Increases your carry capacity by this much (multiplied by item level) while wearing the backpack.",
                    new AcceptableValueRange<int>(1, 4),
                    new ConfigAttributes { IsAdminOnly = true, Order = 2 }));
            
            EffectBiome = ConfigSyncBase.SyncedConfig(_configSection, "Backpack Biome", BackpackBiomes.Swamp,
                new ConfigDescription("The Backpack Biome this effect will apply it's effects on.",
                    null, 
                    new ConfigAttributes { IsAdminOnly = true, Order = 3 }));

        }

    }


    
    //public void UpdateEnvStatusEffects(float dt)
    [HarmonyPatch(typeof(Player), nameof(Player.UpdateEnvStatusEffects))]
    public static class WaterproofPlayerUpdateEnvStatusEffectsPatch
    {
        public static bool Prefix()
        {
            AddingStatusFromEnv++;
            return true;
        }

        public static void Postfix(Player __instance)
        {
            AddingStatusFromEnv--;
        }
    }

    [HarmonyPatch(typeof(SEMan), nameof(SEMan.AddStatusEffect), typeof(string), typeof(bool), typeof(int), typeof(float))]
    public static class WaterproofSEManAddStatusEffectPatch
    {
        public static bool Prefix(SEMan __instance, string name)
        {
            if (!Configuration.EnabledEffect.Value)
                return true;
            
            if (AddingStatusFromEnv > 0 && __instance.m_character.IsPlayer() && name == "Wet")
            {
                var player = (Player) __instance.m_character;
                
                var equippedBackpack = player.GetEquippedBackpack();

                if (equippedBackpack == null)
                    return true;
            
                var itemData = equippedBackpack.Item;
            
                itemData.TryGetBackpackItem(out var backpack);

                var shouldBeWaterProof = backpack.BackpackBiome.Value == Configuration.EffectBiome.Value && itemData.m_quality >= Configuration.QualityLevel.Value;  

                if (shouldBeWaterProof)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
