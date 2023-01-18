using AdventureBackpacks.Assets;
using BepInEx.Configuration;
using UnityEngine;
using Vapok.Common.Abstractions;
using Vapok.Common.Managers.Configuration;

namespace AdventureBackpacks.Configuration
{
    public class ConfigRegistry : ConfigSyncBase
    {
        //Configuration Entry Privates
        internal static ConfigEntry<KeyCode> HotKeyOpen { get; private set; }
        internal static ConfigEntry<KeyCode> HotKeyDrop { get; private set;}
        internal static ConfigEntry<bool> CloseInventory { get; private set;}
        internal static ConfigEntry<bool> OutwardMode { get; private set;}
        internal static ConfigEntry<Vector2> RuggedBackpackSize { get; private set;}
        internal static ConfigEntry<Vector2> ArcticBackpackSize { get; private set;}
        internal static ConfigEntry<float> WeightMultiplier { get; private set;}
        internal static ConfigEntry<int> CarryBonusRugged { get; private set;}
        internal static ConfigEntry<int> CarryBonusArctic { get; private set;}
        internal static ConfigEntry<float> SpeedModRugged { get; private set;}
        internal static ConfigEntry<float> SpeedModArctic { get; private set;}
        internal static ConfigEntry<bool> FreezingRugged { get; private set;}
        internal static ConfigEntry<bool> FreezingArctic { get; private set;}

        public ConfigRegistry(IPluginInfo mod): base(mod)
        {
            InitializeConfigurationSettings();
        }
        public sealed override void InitializeConfigurationSettings()
        {
            if (_config == null)
                return;
            
            //User Configs
            HotKeyOpen = _config.Bind("Local Config", "Open Backpack", KeyCode.I,
                new ConfigDescription("Hotkey to open backpack.", null, new ConfigAttributes{ Order = 3 }));
            
            HotKeyDrop = _config.Bind(
                "Local Config", "Quickdrop Backpack", KeyCode.Y,
                new ConfigDescription("Hotkey to quickly drop backpack while on the run.",
                    null,
                    new ConfigAttributes { Order = 2 }));
            
            CloseInventory = _config.Bind(
                "Local Config", "Close Inventory", true,
                new ConfigDescription("If set to true, both backpack and inventory will close with Open Backpack keybind is pressed while Inventory is open.",
                    null, new ConfigAttributes { Order = 1 }));
            
            OutwardMode = _config.Bind(
                "Local Config", "Outward Mode", false,
                new ConfigDescription("You can use a hotkey to quickly drop your equipped backpack in order to run faster away from danger.",
                    null, new ConfigAttributes { Order = 1 }));
            
            //Server Synced Configs
            RuggedBackpackSize = SyncedConfig(
                "Server-Synced and Enforced Config", "Rugged Backpack Size", new Vector2(6, 3),
                new ConfigDescription("Backpack size (width, height).\nMax width is 8 unless you want to break things.",
                    null,
                    new ConfigAttributes { IsAdminOnly = true, Order = 9 }));
            
            ArcticBackpackSize = SyncedConfig(
                        "Server-Synced and Enforced Config", "Arctic Backpack Size", new Vector2(6, 3),
                        new ConfigDescription("Backpack size (width, height).\nMax width is 8 unless you want to break things.",
                        null,
                        new ConfigAttributes { IsAdminOnly = true, Order = 8 }));

            
            WeightMultiplier = SyncedConfig(
                        "Server-Synced and Enforced Config", "Weight Multiplier", 0.5f,
                        new ConfigDescription("The weight of items stored in the backpack gets multiplied by this value.",
                        new AcceptableValueRange<float>(0f, 1f), // range between 0f and 1f will make it display as a percentage slider
                        new ConfigAttributes { IsAdminOnly = true, Order = 7 }));

            CarryBonusRugged = SyncedConfig(
                        "Server-Synced and Enforced Config", "Rugged Backpack: Carry Bonus", 0,
                        new ConfigDescription("Increases your carry capacity by this much while wearing the backpack.",
                        new AcceptableValueRange<int>(0, 300),
                        new ConfigAttributes { IsAdminOnly = true, Order = 6 }));

            SpeedModRugged = SyncedConfig(
                        "Server-Synced and Enforced Config", "Rugged Backpack: Speed Modifier", -0.15f,
                        new ConfigDescription("Wearing the backpack slows you down by this much.",
                        new AcceptableValueRange<float>(-1f, -0f),
                        new ConfigAttributes { IsAdminOnly = true, Order = 5 }));

            CarryBonusArctic = SyncedConfig(
                        "Server-Synced and Enforced Config", "Arctic Backpack: Carry Bonus", 0,
                        new ConfigDescription("Increases your carry capacity by this much while wearing the backpack.",
                        new AcceptableValueRange<int>(0, 300),
                        new ConfigAttributes { IsAdminOnly = true, Order = 4 }));

            SpeedModArctic = SyncedConfig(
                        "Server-Synced and Enforced Config", "Arctic Backpack: Speed Modifier", -0.15f,
                        new ConfigDescription("Wearing the backpack slows you down by this much.",
                        new AcceptableValueRange<float>(-1f, -0f),
                        new ConfigAttributes { IsAdminOnly = true, Order = 3 }));

            FreezingRugged = SyncedConfig(
                        "Server-Synced and Enforced Config", "Rugged: Prevent freezing/cold?", true,
                        new ConfigDescription("Wearing the backpack protects you against freezing/cold, just like capes.",
                        null,
                        new ConfigAttributes { IsAdminOnly = true, Order = 2 }));

            FreezingArctic = SyncedConfig(
                        "Server-Synced and Enforced Config", "Arctic: Prevent freezing/cold?", true,
                        new ConfigDescription("Wearing the backpack protects you against freezing/cold, just like capes.",
                        null,
                        new ConfigAttributes { IsAdminOnly = true, Order = 1 }));

            CarryBonusRugged!.SettingChanged += Backpacks.UpdateStatusEffectConfigValues;
            SpeedModRugged!.SettingChanged += Backpacks.UpdateItemDataConfigValues;
            CarryBonusArctic!.SettingChanged += Backpacks.UpdateStatusEffectConfigValues;
            SpeedModArctic!.SettingChanged += Backpacks.UpdateItemDataConfigValues;
            FreezingRugged!.SettingChanged += Backpacks.UpdateStatusEffectConfigValues;
            FreezingArctic!.SettingChanged += Backpacks.UpdateStatusEffectConfigValues;

        }
        
        
    }
}