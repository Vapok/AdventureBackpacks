using System;
using System.IO;
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
    private StatusEffect _externalStatusEffect;
    
    public ConfigEntry<int> NecromancySkillBonus { get; private set;}

    public NecromancyArmor(string effectName, string effectDesc) : base(effectName, effectDesc)
    {
        if (Chainloader.PluginInfos.ContainsKey("com.chebgonaz.ChebsNecromancy"))
        {
            var pluginInfo = Chainloader.PluginInfos["com.chebgonaz.ChebsNecromancy"];
            _assetBundlePath = Path.Combine(BepInEx.Paths.PluginPath,Path.GetDirectoryName(pluginInfo.Location) ?? "", AssetFolderName, AssetName);
            
            if (!File.Exists(_assetBundlePath))
                _assetBundlePath = Path.Combine(BepInEx.Paths.PluginPath,Path.GetDirectoryName(pluginInfo.Location) ?? "", AssetName);
            
            _assetBundle = !File.Exists(_assetBundlePath) ? null : AssetBundle.LoadFromFile(_assetBundlePath);

            if (_assetBundle == null)
            {
                AdventureBackpacks.Log.Error($"Can't find Asset Bundle for Status Effect: {_effectName} - Disabling Status Effect");
                EnabledEffect.Value = false;
            }
            
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

    private void LoadExternalStatusEffect()
    {
        try
        {
            if (_externalStatusEffect == null)
            {
                var seStat = _assetBundle.LoadAsset<SE_Stats>(_effectName);
                if (seStat != null)
                {
                    seStat.m_skillLevel = SkillUID;
                    seStat.m_skillLevelModifier = NecromancySkillBonus.Value;
                }
                _externalStatusEffect = seStat;
            }
        }
        catch (Exception e)
        {
            AdventureBackpacks.Log.Error($"Can't Load External Status Effect: {_effectName} - Message: {e.Message}");
        }
    }

    public override bool HasActiveStatusEffect(Humanoid human, out StatusEffect statusEffect)
    {
        statusEffect = null;
        if (!EnabledEffect.Value)
            return false;
        
        LoadExternalStatusEffect();
        SetStatusEffect(_externalStatusEffect);
        return base.HasActiveStatusEffect(human, out statusEffect);
    }

    public override bool HasActiveStatusEffect(ItemDrop.ItemData item, out StatusEffect statusEffect)
    {
        statusEffect = null;
        if (!EnabledEffect.Value)
            return false;
        
        LoadExternalStatusEffect();
        SetStatusEffect(_externalStatusEffect);
        return base.HasActiveStatusEffect(item, out statusEffect);
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
}