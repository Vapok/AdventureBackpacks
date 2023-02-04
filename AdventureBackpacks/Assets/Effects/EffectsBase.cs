using System;
using System.Collections.Generic;
using AdventureBackpacks.Assets.Factories;
using AdventureBackpacks.Configuration;
using BepInEx.Configuration;
using Vapok.Common.Managers.Configuration;
using Vapok.Common.Shared;

namespace AdventureBackpacks.Assets.Effects;

public abstract class EffectsBase
{
    public string EffectName => _effectName;
    public ConfigEntry<bool> EnabledEffect { get; private set;}
    public Dictionary<BackpackBiomes,ConfigEntry<int>> BiomeQualityLevels { get; private set;}

    private string _configSection;
    private string _effectName;
    private string _description;

    public EffectsBase(string effectName, string effectDesc)
    {
        _effectName = effectName;
        _description = string.IsNullOrEmpty(effectDesc) ? "Enables the effect." : effectDesc;
        _configSection = $"Effect: {effectName}";
        

        RegisterEffectConfiguration();
    }

    public void RegisterEffectConfiguration()
    {
        BiomeQualityLevels = new();
        
        EnabledEffect = ConfigSyncBase.SyncedConfig(_configSection, "Effect Enabled", true,
            new ConfigDescription(_description,
                null,
                new ConfigurationManagerAttributes { Order = 1 }));
        
        //Waiting For Startup
        ConfigRegistry.Waiter.StatusChanged += FillBiomeSettings;
    }

    private void FillBiomeSettings(object sender, EventArgs e)
    {
        foreach (BackpackBiomes backpackBiome in Enum.GetValues(typeof(BackpackBiomes)))
        {
            RegisterEffectBiomeQuality(backpackBiome);
        }
    }
    public void RegisterEffectBiomeQuality(BackpackBiomes biome, int defaultQuality = 0)
    {
        if (biome == BackpackBiomes.None)
            return;
        
        var qualityLevel = ConfigSyncBase.SyncedConfig(_configSection, $"Effective Quality Level: {biome.ToString()}", defaultQuality,
            new ConfigDescription("Quality Level needed to apply effect to backpack. Zero disables effect for Biome.",
                new AcceptableValueRange<int>(0, 5),
                new ConfigurationManagerAttributes { Order = 2 }));
        
        if (!BiomeQualityLevels.ContainsKey(biome) && qualityLevel != null)
        { 
            BiomeQualityLevels.Add(biome, qualityLevel);
            qualityLevel.SettingChanged += Backpacks.UpdateItemDataConfigValues;
        }
    }
}