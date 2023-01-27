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
        }
    }
}