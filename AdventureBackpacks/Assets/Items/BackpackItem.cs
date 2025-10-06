using System.Collections.Generic;
using System.Timers;
using AdventureBackpacks.API;
using BepInEx.Configuration;
using UnityEngine;
using Vapok.Common.Abstractions;
using Vapok.Common.Managers.Configuration;
using Vapok.Common.Managers.LocalizationManager;
using Vapok.Common.Managers.StatusEffects;

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
    private Localization _english;
    
    
    private Localization english => _english ??= LocalizationCache.ForLanguage("English");

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
    internal ConfigEntry<float> SpeedMod;
    internal ConfigEntry<bool> EnableFreezing;
    internal ConfigEntry<bool> ShowBackpackStatusEffect;
    internal ConfigEntry<string> CustomStatusEffectName;
    internal ConfigEntry<BackpackBiomes> BackpackBiome;
    
    internal ConfigSyncBase Config => _config;
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
    
    protected BackpackItem(string prefabName, string itemName, string configSection = "", bool externalLocalize = false) : base(prefabName,itemName)
    {
        _configSection = string.IsNullOrEmpty(configSection) ? $"Backpack: {itemName}" : configSection;
        _englishSection = english.Localize(_configSection);

        if (externalLocalize)
            _localizedCategory = Localization.m_instance.Localize(_configSection);
        else
            _localizedCategory = Localization.instance.Localize(_configSection);
        
        SetupBackpackDef();
    }

    internal void SetupLocalization()
    {
        _englishSection = english.Localize(_configSection);
        _localizedCategory = Localization.m_instance.Localize(_configSection);
        if (_localizedCategory.Equals(_configSection))
            _localizedCategory = Localization.instance.Localize(_configSection);
    }
    
    private void SetupBackpackDef()
    {
        Item.SectionName = _configSection;
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
        //Blacksmithing from Blaxxun is allowing items to go higher than my original intent.
        //If quantity entering here is higher than 4, let's Clamp it at 4.
        quality = Mathf.Clamp(quality, 1, 4);
        
        return new Vector2i(Mathf.Clamp((int)BackpackSize[quality].Value.x,1,8),(int)BackpackSize[quality].Value.y);    
    }
    
    internal abstract void UpdateStatusEffects(int quality, CustomSE statusEffects, List<HitData.DamageModPair> modifierList, ItemDrop.ItemData itemData);
    
    internal virtual void RegisterBackpackSize(int quality = 1, int x = 6, int y = 3)
    {
        ConfigEntry<Vector2> newSize = null;
        BackpackSize.Add(quality, ConfigSyncBase.SyncedConfig(_englishSection, $"Backpack Size - Level {quality}", new Vector2(x, y),
            new ConfigDescription("Backpack size (width, height).\nMax width is 8 unless you want to break things.",
                null,
                new ConfigurationManagerAttributes { Category = _localizedCategory, Order = 3 }),ref newSize));
        
        BackpackSize[quality]!.SettingChanged += Backpacks.UpdateItemDataConfigValues;
    }

    internal virtual void RegisterWeightMultiplier(float defaultValue = 0.5f)
    {
        WeightMultiplier = ConfigSyncBase.SyncedConfig(_englishSection, "Weight Multiplier", defaultValue,
            new ConfigDescription("The weight of items stored in the backpack gets multiplied by this value.",
                new AcceptableValueRange<float>(0f, 1f), // range between 0f and 1f will make it display as a percentage slider
                new ConfigurationManagerAttributes { Category = _localizedCategory, Order = 4 }),ref WeightMultiplier);
        
        WeightMultiplier!.SettingChanged += Backpacks.UpdateItemDataConfigValues;
    }

    internal virtual void RegisterBackpackBiome(BackpackBiomes defaultValue = BackpackBiomes.None)
    {
        BackpackBiome = ConfigSyncBase.SyncedConfig(_englishSection, "Backpack Biome", defaultValue,
            new ConfigDescription("The Biome this bag will draw it's effects from.",
                null, 
                new ConfigurationManagerAttributes { Category = _localizedCategory, Order = 5 }), ref BackpackBiome);
        BackpackBiome.SettingChanged += Backpacks.UpdateItemDataConfigValues;
    }

    internal virtual void RegisterCarryBonus(int defaultValue = 0)
    {
        CarryBonus = ConfigSyncBase.SyncedConfig(_englishSection, "Carry Bonus", defaultValue,
            new ConfigDescription("Increases your carry capacity by this much (multiplied by item level) while wearing the backpack.",
                new AcceptableValueRange<int>(0, 300),
                new ConfigurationManagerAttributes { Category = _localizedCategory, Order = 6 }), ref CarryBonus);
        
        CarryBonus!.SettingChanged += Backpacks.UpdateItemDataConfigValues;
    }

    internal virtual void RegisterStatusEffectInfo(bool defaultShowStatus = true, string defaultEffectName = "")
    {
        ShowBackpackStatusEffect = ConfigSyncBase.SyncedConfig(_englishSection, "Show Status Effect", defaultShowStatus,
            new ConfigDescription("Toggles the visibility of the Backpack Status Effect",
                null,
                new ConfigurationManagerAttributes { Category = _localizedCategory, Order = 1 }), ref ShowBackpackStatusEffect);
        
        ShowBackpackStatusEffect!.SettingChanged += Backpacks.UpdateItemDataConfigValues;

        CustomStatusEffectName = ConfigSyncBase.SyncedConfig(_englishSection, "Custom Effect Name", defaultEffectName,
            new ConfigDescription("Set your own effect name. Leave Empty to use Default Effect name",
                null,
                new ConfigurationManagerAttributes { Category = _localizedCategory, Order = 2 }),ref CustomStatusEffectName);
        
        CustomStatusEffectName!.SettingChanged += Backpacks.UpdateItemDataConfigValues;
    }

    internal virtual void RegisterSpeedMod(float defaultValue = -0.15f)
    {
        SpeedMod = ConfigSyncBase.SyncedConfig(_englishSection, "Speed Modifier", defaultValue,
            new ConfigDescription("Wearing the backpack slows you down by this much.",
                new AcceptableValueRange<float>(-1f, -0f),
                new ConfigurationManagerAttributes { Category = _localizedCategory, Order = 7 }), ref SpeedMod);
        
        SpeedMod!.SettingChanged += Backpacks.UpdateItemDataConfigValues;
    }

    internal virtual void RegisterEnableFreezing(bool defaultValue = true)
    {
        EnableFreezing = ConfigSyncBase.SyncedConfig(_englishSection, "Prevent freezing/cold?", defaultValue,
            new ConfigDescription("Wearing the backpack protects you against freezing/cold, just like capes.",
                null,
                new ConfigurationManagerAttributes { Category = _localizedCategory, Order = 8 }),ref EnableFreezing);
        
        EnableFreezing!.SettingChanged += Backpacks.UpdateItemDataConfigValues;
    }
}