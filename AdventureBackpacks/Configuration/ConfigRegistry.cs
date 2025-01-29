﻿using System;
using BepInEx.Configuration;
using UnityEngine;
using Vapok.Common.Abstractions;
using Vapok.Common.Managers.Configuration;
using Vapok.Common.Shared;

namespace AdventureBackpacks.Configuration
{
    public class ConfigRegistry : ConfigSyncBase
    {
        //Configuration Entry Privates
        internal static ConfigEntry<KeyboardShortcut> HotKeyOpen { get; private set; }
        internal static ConfigEntry<KeyboardShortcut> HotKeyDrop { get; private set;}
        internal static ConfigEntry<bool> OpenWithInventory { get; private set;}
        internal static ConfigEntry<bool> OpenWithHoverInteract { get; private set;}
        internal static ConfigEntry<bool> CloseInventory { get; private set;}
        internal static ConfigEntry<bool> OutwardMode { get; private set;}
        
        internal static ConfigEntry<bool> ReplaceShader { get; private set;}
        
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
            HotKeyOpen = _config.Bind("Local Config", "Open Backpack", new KeyboardShortcut(KeyCode.I),
                new ConfigDescription("Hotkey to open backpack.", null, new ConfigurationManagerAttributes{ Order = 3 }));
            
            HotKeyDrop = _config.Bind(
                "Local Config", "Quickdrop Backpack", new KeyboardShortcut(KeyCode.Y),
                new ConfigDescription("Hotkey to quickly drop backpack while on the run.",
                    null,
                    new ConfigurationManagerAttributes { Order = 1 }));
            
            OpenWithInventory = _config.Bind(
                "Local Config", "Open with Inventory", false,
                new ConfigDescription("If enabled, both backpack and inventory will open when Inventory is opened.",
                    null, new ConfigurationManagerAttributes { Order = 3 }));
            
            OpenWithHoverInteract = _config.Bind(
                "Local Config", "Open with Interactive Hover", false,
                new ConfigDescription("If enabled, backpack will only open while hovering over equipped backpack and pressing hotkey.  This option overrides Open with Inventory.",
                    null, new ConfigurationManagerAttributes { Order = 3 }));
            
            CloseInventory = _config.Bind(
                "Local Config", "Close Inventory", true,
                new ConfigDescription("If enabled, both backpack and inventory will close with Open Backpack keybind is pressed while Inventory is open.",
                    null, new ConfigurationManagerAttributes { Order = 2 }));
            
            OutwardMode = _config.Bind(
                "Local Config", "Outward Mode", false,
                new ConfigDescription("You can use a hotkey to quickly drop your equipped backpack in order to run faster away from danger.",
                    null, new ConfigurationManagerAttributes { Order = 1 }));

            ReplaceShader = _config.Bind(
                "Local Config", "Replace Shader", true,
                new ConfigDescription("Toggle To use the Material Shader Replacer (Requires Game Restart)",
                    null, new ConfigurationManagerAttributes { Order = 1 }));

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