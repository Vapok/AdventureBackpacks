using AdventureBackpacks.Assets.Factories;
using BepInEx.Configuration;
using UnityEngine;
using Vapok.Common.Abstractions;
using Vapok.Common.Managers.Configuration;

namespace AdventureBackpacks.Assets.Items;

internal interface IBackpackItem : IAssetItem
{
}
internal abstract class BackpackItem : AssetItem, IBackpackItem
{
    
    
    private BackpackBiomes _backpackBiome;
    private static ConfigSyncBase _config;
    private static ILogIt _logger;
    
    //Config Settings
    internal ConfigEntry<Vector2> BackpackSize { get; private set;}
    internal ConfigEntry<float> WeightMultiplier { get; private set;}
    internal ConfigEntry<int> CarryBonus { get; private set;}
    internal ConfigEntry<float> SpeedMod { get; private set;}
    internal ConfigEntry<bool> EnableFreezing { get; private set;}
    
    
    internal ConfigSyncBase Config => _config;
    internal ILogIt Log => _logger;
        
    protected BackpackItem(string prefabName, string itemName) : base(prefabName,itemName)
    {
        
    }

    internal BackpackBiomes Biome
    {
        get => _backpackBiome;
        set => _backpackBiome = value;
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
    
    internal virtual void RegisterBackpackSize()
    {
        BackpackSize = ConfigSyncBase.SyncedConfig($"{ItemName} Settings", "Backpack Size", new Vector2(6, 3),
            new ConfigDescription("Backpack size (width, height).\nMax width is 8 unless you want to break things.",
                null,
                new ConfigAttributes { IsAdminOnly = true, Order = 1 }));
        
        BackpackSize!.SettingChanged += Backpacks.UpdateItemDataConfigValues;
    }

    internal virtual void RegisterWeightMultiplier()
    {
        WeightMultiplier = ConfigSyncBase.SyncedConfig($"{ItemName} Settings", "Weight Multiplier", 0.5f,
            new ConfigDescription("The weight of items stored in the backpack gets multiplied by this value.",
                new AcceptableValueRange<float>(0f, 1f), // range between 0f and 1f will make it display as a percentage slider
                new ConfigAttributes { IsAdminOnly = true, Order = 2 }));
        
        WeightMultiplier!.SettingChanged += Backpacks.UpdateItemDataConfigValues;
    }

    internal virtual void RegisterCarryBonus()
    {
        CarryBonus = ConfigSyncBase.SyncedConfig($"{ItemName} Settings", "Carry Bonus", 0,
            new ConfigDescription("Increases your carry capacity by this much while wearing the backpack.",
                new AcceptableValueRange<int>(0, 300),
                new ConfigAttributes { IsAdminOnly = true, Order = 3 }));
        
        CarryBonus!.SettingChanged += Backpacks.UpdateItemDataConfigValues;
    }

    internal virtual void RegisterSpeedMod()
    {
        SpeedMod = ConfigSyncBase.SyncedConfig($"{ItemName} Settings", "Speed Modifier", -0.15f,
            new ConfigDescription("Wearing the backpack slows you down by this much.",
                new AcceptableValueRange<float>(-1f, -0f),
                new ConfigAttributes { IsAdminOnly = true, Order = 4 }));
        
        SpeedMod!.SettingChanged += Backpacks.UpdateItemDataConfigValues;
    }

    internal virtual void RegisterEnableFreezing()
    {
        EnableFreezing = ConfigSyncBase.SyncedConfig($"{ItemName} Settings", "Prevent freezing/cold?", true,
            new ConfigDescription("Wearing the backpack protects you against freezing/cold, just like capes.",
                null,
                new ConfigAttributes { IsAdminOnly = true, Order = 5 }));
        
        EnableFreezing!.SettingChanged += Backpacks.UpdateItemDataConfigValues;
    }
}

