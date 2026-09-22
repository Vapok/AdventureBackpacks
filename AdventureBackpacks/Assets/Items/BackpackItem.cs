using System.Collections.Generic;
using System.Timers;
using AdventureBackpacks.API;
using BepInEx.Configuration;
using UnityEngine;
using Vapok.Common.Abstractions;
using Vapok.Common.Managers.Configuration;
using Vapok.Common.Managers.LocalizationManager;
using Vapok.Common.Managers.StatusEffects;
using Vapok.Common.Shared;

namespace AdventureBackpacks.Assets.Items;

internal interface IBackpackItem : IAssetItem
{
}
internal abstract class BackpackItem : AssetItem, IBackpackItem
{
    private static ConfigSyncBase _config;
    private static ILogIt _logger;
    private string _configSection;
    private string _englishSection;
    private string _localizedCategory;

    public System.Timers.Timer InceptionTimer;
    public System.Timers.Timer YardSaleTimer;
    
    public int InceptionCounter
    {
        get => _inceptionCounter;
        set
        {
            _inceptionCounter = value;
            if (value <= 0) return;
            InceptionTimer.Stop();
            InceptionTimer.Start();
        }
    }


    private int _inceptionCounter;
    
    //Config Settings
    internal Dictionary<int,ConfigEntry<Vector2>> BackpackSize;
    internal ConfigEntry<float> WeightMultiplier;
    internal ConfigEntry<int> CarryBonus;
    internal ConfigEntry<float> HeatResistance;
    internal ConfigEntry<float> SpeedMod;
    internal ConfigEntry<bool> EnableFreezing;
    internal ConfigEntry<bool> ShowBackpackStatusEffect;
    internal ConfigEntry<string> CustomStatusEffectName;
    internal ConfigEntry<BackpackBiomes> BackpackBiome;
    
    internal ConfigSyncBase Config => _config;
    protected string EnglishSection => _englishSection;
    protected string LocalizedCategory => _localizedCategory;
    internal ILogIt Log => _logger;


    protected BackpackItem(ABAPI.BackpackDefinition definition, GameObject goItem)
        : base(goItem,definition.ItemName)
    {
        _configSection = string.IsNullOrEmpty(definition.ConfigSection) ? $"Backpack: {definition.ItemName}" : definition.ConfigSection;
        SetupLocalization();
        SetupBackpackDef();
    }

    protected BackpackItem(ABAPI.BackpackDefinition definition)
        : base(definition.AssetBundle,definition.PrefabName,definition.ItemName)
    {
        _configSection = string.IsNullOrEmpty(definition.ConfigSection) ? $"Backpack: {definition.ItemName}" : definition.ConfigSection;
        SetupLocalization();
        SetupBackpackDef();
    }
    
    protected BackpackItem(GameObject goItem, string itemName, string configSection = "") : base(goItem, itemName)
    {
        try
        {
            _configSection = string.IsNullOrEmpty(configSection) ? $"Backpack: {itemName}" : configSection;
            _englishSection = SafeGetTranslation("English", _configSection);
            _localizedCategory = Localization.instance?.Localize(_configSection) ?? Localization.m_instance?.Localize(_configSection) ?? _configSection;
            SetupBackpackDef();
        }
        catch (System.Exception ex)
        {
            AdventureBackpacks.Log?.Warning($"Error initializing BackpackItem '{itemName}': {ex.Message}");
            _englishSection = _configSection;
            _localizedCategory = _configSection;
        }
    }

    protected BackpackItem(string assetName, string prefabName, string itemName, string configSection = "", bool externalLocalize = false, string registerAs = null) : base(assetName, prefabName,itemName, registerAs)
    {
        try
        {
            _configSection = string.IsNullOrEmpty(configSection) ? $"Backpack: {itemName}" : configSection;
            _englishSection = SafeGetTranslation("English", _configSection);

            _localizedCategory = Localization.instance?.Localize(_configSection) ?? _configSection;
            
            SetupBackpackDef();
        }
        catch (System.Exception ex)
        {
            AdventureBackpacks.Log?.Warning($"Error initializing BackpackItem '{itemName}': {ex.Message}");
            _englishSection = _configSection;
            _localizedCategory = _configSection;
        }
    }

    internal void SetupLocalization()
    {
        try
        {
            _englishSection = SafeGetTranslation("English", _configSection);
            _localizedCategory = Localization.instance?.Localize(_configSection) ?? _configSection;
        }
        catch (System.Exception ex)
        {
            AdventureBackpacks.Log?.Warning($"Failed to localize section '{_configSection}': {ex.Message}");
            _englishSection = _configSection;
            _localizedCategory = _configSection;
        }
    }

    private static string SafeGetTranslation(string language, string text)
    {
        try
        {
            return Localizer.GetTranslation(language, text);
        }
        catch (System.Exception ex)
        {
            AdventureBackpacks.Log.Warning($"Failed to get translation for '{text}': {ex.Message}");
            return text;
        }
    }
    
    private void SetupBackpackDef()
    {
        if (Item != null)
        {
            Item.SectionName = _configSection;
        }
        BackpackSize = new();
        InceptionTimer = new System.Timers.Timer(10000);
        InceptionTimer.AutoReset = false;
        InceptionTimer.Enabled = false;
        InceptionTimer.Elapsed += InceptionCounterReset;

    }

    private void InceptionCounterReset(object source, ElapsedEventArgs e)
    {
        InceptionCounter = 0;
        Log.Message("Odin walks away.");
    }
    
    internal static void SetConfig(ConfigSyncBase configSync)
    {
        _config = configSync;
    }
    
    internal static void SetLogger(ILogIt logger)
    {
        _logger = logger;
    }

    internal abstract void RegisterConfigSettings();
    
    internal virtual Vector2i GetInventorySize(int quality)
    {
        quality = Mathf.Clamp(quality, 1, 4);
        
        if (!BackpackSize.TryGetValue(quality, out var sizeEntry) || sizeEntry?.Value == null)
        {
            if (BackpackSize.TryGetValue(1, out sizeEntry) && sizeEntry?.Value != null)
            {
                var fallback = new Vector2i(Mathf.Clamp((int)sizeEntry.Value.x, 1, 256), Mathf.Clamp((int)sizeEntry.Value.y, 1, 256));
                return Backpacks.ValidateMinMaxChestSizeInt(fallback.x, fallback.y);
            }
            return new Vector2i(6, 3);
        }

        var backpackSize = new Vector2i(Mathf.Clamp((int)sizeEntry.Value.x, 1, 256), Mathf.Clamp((int)sizeEntry.Value.y, 1, 256));

        return Backpacks.ValidateMinMaxChestSizeInt(backpackSize.x, backpackSize.y);
    }
    
    internal abstract void UpdateStatusEffects(int quality, CustomSE statusEffects, List<HitData.DamageModPair> modifierList, ItemDrop.ItemData itemData);
    
    internal virtual void RegisterBackpackSize(int quality = 1, int x = 6, int y = 3)
    {
        ConfigEntry<Vector2> newSize = null;
        ConfigSyncBase.SyncedConfig(_englishSection, $"Backpack Size - Level {quality}", new Vector2(x, y),
            new ConfigDescription("Backpack size (width, height).\nMax width is 8 unless you want to break things.",
                null,
                new ConfigurationManagerAttributes { Category = _localizedCategory, Order = 3 }), ref newSize);
        BackpackSize.Add(quality, newSize);
        
        if (newSize != null)
        {
            newSize.SettingChanged += Backpacks.UpdateItemDataConfigValues;
        }
    }

    internal virtual void RegisterWeightMultiplier(float defaultValue = 0.5f)
    {
        ConfigSyncBase.SyncedConfig(_englishSection, "Weight Multiplier", defaultValue,
            new ConfigDescription("The weight of items stored in the backpack gets multiplied by this value. Setting to 100% or 1.0 disables weight reduction.",
                new AcceptableValueRange<float>(0f, 1f), // range between 0f and 1f will make it display as a percentage slider
                new ConfigurationManagerAttributes { Category = _localizedCategory, Order = 4 }),ref WeightMultiplier);
        
        if (WeightMultiplier != null)
        {
            WeightMultiplier.SettingChanged += Backpacks.UpdateItemDataConfigValues;
        }
    }

    internal virtual void RegisterBackpackBiome(BackpackBiomes defaultValue = BackpackBiomes.None)
    {
        ConfigSyncBase.SyncedConfig(_englishSection, "Backpack Biome", defaultValue,
            new ConfigDescription("The Biome this bag will draw it's effects from.",
                null, 
                new ConfigurationManagerAttributes { Category = _localizedCategory, Order = 5 }), ref BackpackBiome);
        if (BackpackBiome != null)
        {
            BackpackBiome.SettingChanged += Backpacks.UpdateItemDataConfigValues;
        }
    }

    internal virtual void RegisterHeatResistance(float defaultValue = 0.1f)
    {
        ConfigSyncBase.SyncedConfig(_englishSection, "Heat Resistance", defaultValue,
            new ConfigDescription("Heat resistance per item level. Reduces lava damage and delays boiling water damage. Does nothing against burning.",
                new AcceptableValueRange<float>(0f, 0.25f),
                new ConfigurationManagerAttributes { Category = _localizedCategory, Order = 9 }), ref HeatResistance);

        if (HeatResistance != null)
        {
            HeatResistance.SettingChanged += Backpacks.UpdateItemDataConfigValues;
        }
    }

    internal virtual void RegisterCarryBonus(int defaultValue = 0)
    {
        ConfigSyncBase.SyncedConfig(_englishSection, "Carry Bonus", defaultValue,
            new ConfigDescription("Increases your carry capacity by this much (multiplied by item level) while wearing the backpack.",
                new AcceptableValueRange<int>(0, 300),
                new ConfigurationManagerAttributes { Category = _localizedCategory, Order = 6 }), ref CarryBonus);
        
        if (CarryBonus != null)
        {
            CarryBonus.SettingChanged += Backpacks.UpdateItemDataConfigValues;
        }
    }

    internal virtual void RegisterStatusEffectInfo(bool defaultShowStatus = true, string defaultEffectName = "")
    {
        ConfigSyncBase.SyncedConfig(_englishSection, "Show Status Effect", defaultShowStatus,
            new ConfigDescription("Toggles the visibility of the Backpack Status Effect",
                null,
                new ConfigurationManagerAttributes { Category = _localizedCategory, Order = 1 }), ref ShowBackpackStatusEffect);
        
        if (ShowBackpackStatusEffect != null)
        {
            ShowBackpackStatusEffect.SettingChanged += Backpacks.UpdateItemDataConfigValues;
        }

        ConfigSyncBase.SyncedConfig(_englishSection, "Custom Effect Name", defaultEffectName,
            new ConfigDescription("Set your own effect name. Leave Empty to use Default Effect name",
                null,
                new ConfigurationManagerAttributes { Category = _localizedCategory, Order = 2 }),ref CustomStatusEffectName);
        
        if (CustomStatusEffectName != null)
        {
            CustomStatusEffectName.SettingChanged += Backpacks.UpdateItemDataConfigValues;
        }
    }

    internal virtual void RegisterSpeedMod(float defaultValue = -0.15f)
    {
        ConfigSyncBase.SyncedConfig(_englishSection, "Speed Modifier", defaultValue,
            new ConfigDescription("Wearing the backpack slows you down by this much.",
                new AcceptableValueRange<float>(-1f, -0f),
                new ConfigurationManagerAttributes { Category = _localizedCategory, Order = 7 }), ref SpeedMod);
        
        if (SpeedMod != null)
        {
            SpeedMod.SettingChanged += Backpacks.UpdateItemDataConfigValues;
        }
    }

    internal virtual void RegisterEnableFreezing(bool defaultValue = true)
    {
        ConfigSyncBase.SyncedConfig(_englishSection, "Prevent freezing/cold?", defaultValue,
            new ConfigDescription("Wearing the backpack protects you against freezing/cold, just like capes.",
                null,
                new ConfigurationManagerAttributes { Category = _localizedCategory, Order = 8 }),ref EnableFreezing);
        
        if (EnableFreezing != null)
        {
            EnableFreezing.SettingChanged += Backpacks.UpdateItemDataConfigValues;
        }
    }
}