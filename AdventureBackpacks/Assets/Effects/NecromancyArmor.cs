using System;
using System.IO;
using AdventureBackpacks.Assets.Factories;
using AdventureBackpacks.Extensions;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using UnityEngine;
using Vapok.Common.Managers.Configuration;
using Vapok.Common.Shared;

namespace AdventureBackpacks.Assets.Effects;

public class NecromancyArmor : EffectsBase
{
    private string _effectName = "SetEffect_NecromancyArmor";
    private const string AssetName = "chebgonaz";
    private const string AssetFolderName = "Assets";
    private string _assetBundlePath = "";
    private AssetBundle _assetBundle;
    private const string NecromancySkillIdentifier = "friendlyskeletonwand_necromancy_skill";
    private Skills.SkillType SkillUID;
    
    public ConfigEntry<int> NecromancySkillBonus { get; private set;}

    public NecromancyArmor(string effectName, string effectDesc) : base(effectName, effectDesc)
    {
        if (Chainloader.PluginInfos.ContainsKey("com.chebgonaz.ChebsNecromancy"))
        {
            var pluginInfo = Chainloader.PluginInfos["com.chebgonaz.ChebsNecromancy"];
            _assetBundlePath = Path.Combine(BepInEx.Paths.PluginPath,Path.GetDirectoryName(pluginInfo.Location) ?? "", AssetFolderName, AssetName);
            _assetBundle = !File.Exists(_assetBundlePath) ? null : AssetBundle.LoadFromFile(_assetBundlePath);
            
            var num = Math.Abs(NecromancySkillIdentifier.GetStableHashCode());
            SkillUID = (Skills.SkillType)num;
        }
    }

    public override void AdditionalConfiguration(string configSection)
    {
        NecromancySkillBonus = ConfigSyncBase.SyncedConfig(configSection, "Spectral Shroud Skill Bonus", 10,
            new ConfigDescription("How much wearing the item should raise the Necromancy level (set to 0 to have no set effect at all).",
                null,
                new ConfigurationManagerAttributes { Order = 2 }));
        
        NecromancySkillBonus.SettingChanged += Backpacks.UpdateItemDataConfigValues;
    }

    public override StatusEffect GetStatusEffect(string effectName)
    {
        var seStat = _assetBundle.LoadAsset<SE_Stats>(effectName);
        seStat.m_skillLevel = SkillUID;
        seStat.m_skillLevelModifier = NecromancySkillBonus.Value;
        return seStat;
    }

    public override bool HasActiveStatusEffect(Humanoid human, out StatusEffect statusEffect)
    {
        statusEffect = GetStatusEffect(_effectName);
        return statusEffect != null && IsEffectActive(human);
    }

    public override bool HasActiveStatusEffect(ItemDrop.ItemData item, out StatusEffect statusEffect)
    {
        statusEffect = GetStatusEffect(_effectName);
        return statusEffect != null && IsEffectActive(item);
    }

    public override bool IsEffectActive(Humanoid human)
    {
        if (human is Player player)
        {
            var equippedBackpack = player.GetEquippedBackpack();
            
            if (equippedBackpack == null || !EnabledEffect.Value)
                return false;
            
            var itemData = equippedBackpack.Item;
            return IsEffectActive(itemData);
        }

        return false;
    }

    public override bool IsEffectActive(ItemDrop.ItemData itemData)
    {
        if (!EnabledEffect.Value)
            return false;

        if (itemData != null && itemData.TryGetBackpackItem(out var backpack))
        {
            var backpackBiome = backpack.BackpackBiome.Value;

            if (BiomeQualityLevels.ContainsKey(backpackBiome))
            {
                var configQualityForBiome = BiomeQualityLevels[backpackBiome].Value;

                if (configQualityForBiome == 0 || backpackBiome == BackpackBiomes.None)
                    return false;
                
                return itemData.m_quality >= configQualityForBiome;  
            }
        }
        
        return false;
    }
}