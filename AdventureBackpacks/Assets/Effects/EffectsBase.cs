using System;
using System.Collections.Generic;
using AdventureBackpacks.API;
using AdventureBackpacks.Configuration;
using AdventureBackpacks.Extensions;
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
    private StatusEffect _statusEffect;
    private bool _isSetItemStatusEffect;

    public EffectsBase(string effectName, string effectDesc, bool IsItemSetStatusEffect = false)
    {
        _effectName = effectName;
        _description = string.IsNullOrEmpty(effectDesc) ? "Enables the effect." : effectDesc;
        _configSection = $"Effect: {effectName}";
        _isSetItemStatusEffect = IsItemSetStatusEffect;

        AdventureBackpacks.Waiter.StatusChanged += (_,_)=>LoadStatusEffect();

        RegisterEffectConfiguration();
    }

    public abstract void LoadStatusEffect();

    public virtual StatusEffect GetStatusEffect()
    {
        return _statusEffect;
    }

    public virtual void SetStatusEffect(StatusEffect statusEffect)
    {
        _statusEffect = statusEffect;
    }

    public virtual void SetStatusEffect(string effectName)
    {
        _statusEffect = _statusEffect == null ? ObjectDB.instance.GetStatusEffect(effectName.GetHashCode()) : _statusEffect;
    }

    public virtual bool HasActiveStatusEffect(Humanoid human, out StatusEffect statusEffect)
    {
        statusEffect = _statusEffect;
        return (statusEffect != null && IsEffectActive(human)) && !_isSetItemStatusEffect;
    }
    public virtual bool HasActiveStatusEffect(ItemDrop.ItemData item, out StatusEffect statusEffect)
    {
        statusEffect = _statusEffect;
        return statusEffect != null && IsEffectActive(item);
    }

    public virtual bool IsEffectActive(Humanoid human)
    {
        if (human is Player player)
        {
            var equippedBackpack = player.GetEquippedBackpack();
            
            if (equippedBackpack == null || !EnabledEffect.Value)
                return false;
            
            var itemData = equippedBackpack.Item;
            
            itemData.TryGetBackpackItem(out var backpack);

            var backpackBiome = backpack.BackpackBiome.Value;

            var configQualityForBiome = 0;
            foreach (var enumKeyBit in BiomeQualityLevels.Keys)
            {
                if ((backpackBiome & enumKeyBit) != 0)
                {
                    configQualityForBiome = BiomeQualityLevels[enumKeyBit].Value > configQualityForBiome ? BiomeQualityLevels[enumKeyBit].Value : configQualityForBiome;
                }
            }
            
            if (configQualityForBiome == 0)
                return false;
                
            return itemData.m_quality >= configQualityForBiome;  

        }
        return false;
    }

    public virtual bool IsEffectActive(ItemDrop.ItemData itemData)
    {
        if (!EnabledEffect.Value)
            return false;

        if (itemData != null && itemData.TryGetBackpackItem(out var backpack))
        {
            var backpackBiome = backpack.BackpackBiome.Value;

            var configQualityForBiome = 0;
            foreach (var enumKeyBit in BiomeQualityLevels.Keys)
            {
                if ((backpackBiome & enumKeyBit) != 0)
                {
                    configQualityForBiome = BiomeQualityLevels[enumKeyBit].Value > configQualityForBiome ? BiomeQualityLevels[enumKeyBit].Value : configQualityForBiome;
                }
            }
            
            if (configQualityForBiome == 0)
                return false;
                
            return itemData.m_quality >= configQualityForBiome;  
        }
        return false;
    }
    
    public void RegisterEffectConfiguration()
    {
        BiomeQualityLevels = new();
        
        EnabledEffect = ConfigSyncBase.SyncedConfig(_configSection, "Effect Enabled", true,
            new ConfigDescription(_description,
                null,
                new ConfigurationManagerAttributes { Order = 1 }));
        
        //Waiting For Startup
        ConfigRegistry.Waiter.StatusChanged += (_,_) => AdditionalConfiguration(_configSection);
    }

    public virtual void AdditionalConfiguration(string configSection)
    {
        return;
    }
    private void FillBiomeSettings()
    {
        foreach (BackpackBiomes backpackBiome in Enum.GetValues(typeof(BackpackBiomes)))
        {
            RegisterEffectBiomeQuality(backpackBiome,0,false);
        }
    }
    public void RegisterEffectBiomeQuality(BackpackBiomes biome, int defaultQuality = 0, bool fillUp = true)
    {
        if (biome == BackpackBiomes.None)
            return;
        
        foreach (BackpackBiomes enumKeyBit in Enum.GetValues(typeof(BackpackBiomes)))
        {
            if ((biome & enumKeyBit) != 0)
            {
                var qualityLevel = ConfigSyncBase.SyncedConfig(_configSection, $"Effective Quality Level: {enumKeyBit.ToString()}", defaultQuality,
                    new ConfigDescription("Quality Level needed to apply effect to backpack. Zero disables effect for Biome.",
                        new AcceptableValueRange<int>(0, 5),
                        new ConfigurationManagerAttributes { Order = 2 }));

                if (!BiomeQualityLevels.ContainsKey(enumKeyBit) && qualityLevel != null)
                {
                    BiomeQualityLevels.Add(enumKeyBit, qualityLevel);
                    qualityLevel.SettingChanged += Backpacks.UpdateItemDataConfigValues;
                }
            }
        }

        if (fillUp)
        {
            FillBiomeSettings();
        }
    }
}