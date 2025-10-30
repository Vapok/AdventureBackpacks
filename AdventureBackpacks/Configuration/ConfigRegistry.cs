using System;
using BepInEx.Configuration;
using UnityEngine;
using Vapok.Common.Abstractions;
using Vapok.Common.Managers.Configuration;

namespace AdventureBackpacks.Configuration
{
    public class ConfigRegistry : ConfigSyncBase
    {
        //Configuration Entry Privates
        internal static ConfigEntry<KeyboardShortcut> HotKeyOpen = null;
        internal static ConfigEntry<KeyboardShortcut> HotKeyDrop = null;
        internal static ConfigEntry<KeyboardShortcut> WisplightKeyToggle = null;
        internal static ConfigEntry<bool> OpenWithInventory = null;
        internal static ConfigEntry<bool> OpenWithHoverInteract = null;
        internal static ConfigEntry<bool> CloseInventory = null;
        internal static ConfigEntry<bool> OutwardMode = null;
        internal static ConfigEntry<bool> ReplaceShader = null;
        
        public static Waiting Waiter;

        public ConfigRegistry(IPluginInfo mod): base(mod)
        {
            //Waiting For Startup
            Waiter = new Waiting();

            InitializeConfigurationSettings();
        }

        public sealed override void InitializeConfigurationSettings()
        {
            if (_config == null)
                return;
            
            //User Configs
            UnsyncedConfig("Local Config", "Open Backpack", new KeyboardShortcut(KeyCode.I),
                new ConfigDescription("Hotkey to open backpack.", null, new ConfigurationManagerAttributes{ Order = 3 }), ref HotKeyOpen);
            
            UnsyncedConfig("Local Config", "Quickdrop Backpack", new KeyboardShortcut(KeyCode.Y),
                new ConfigDescription("Hotkey to quickly drop backpack while on the run.",
                    null,
                    new ConfigurationManagerAttributes { Order = 1 }),ref HotKeyDrop);
            
            UnsyncedConfig("Local Config", "Wisplight Effect Toggle", new KeyboardShortcut(KeyCode.L),
                new ConfigDescription("Hotkey to turn Wisplight on and off",
                    null,
                    new ConfigurationManagerAttributes { Order = 1 }),ref WisplightKeyToggle);
            
            UnsyncedConfig("Local Config", "Open with Inventory", false,
                new ConfigDescription("If enabled, both backpack and inventory will open when Inventory is opened.",
                    null, new ConfigurationManagerAttributes { Order = 3 }),ref OpenWithInventory);
            
            UnsyncedConfig("Local Config", "Open with Interactive Hover", false,
                new ConfigDescription("If enabled, backpack will only open while hovering over equipped backpack and pressing hotkey.  This option overrides Open with Inventory.",
                    null, new ConfigurationManagerAttributes { Order = 3 }), ref OpenWithHoverInteract);
            
            UnsyncedConfig("Local Config", "Close Inventory", true,
                new ConfigDescription("If enabled, both backpack and inventory will close with Open Backpack keybind is pressed while Inventory is open.",
                    null, new ConfigurationManagerAttributes { Order = 2 }), ref CloseInventory);
            
            UnsyncedConfig("Local Config", "Outward Mode", false,
                new ConfigDescription("You can use a hotkey to quickly drop your equipped backpack in order to run faster away from danger.",
                    null, new ConfigurationManagerAttributes { Order = 1 }), ref OutwardMode);

            UnsyncedConfig("Local Config", "Replace Shader", true,
                new ConfigDescription("Toggle To use the Material Shader Replacer (Requires Game Restart)",
                    null, new ConfigurationManagerAttributes { Order = 1 }), ref ReplaceShader);
            

        }
    }
    
    public class Waiting
    {
        public void ConfigurationComplete(bool configDone)
        {
            if (configDone)
                StatusChanged?.Invoke(this, EventArgs.Empty);
        }
        public event EventHandler StatusChanged;            
    }

}