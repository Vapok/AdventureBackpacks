/* Adventure Backpacks by Vapok
 *
 * This largely a new port of the original Jotunn Backpacks.  Only.. without the Jotunn, and other things.
 *
 * Planned Roadmap:
 * v1.5.0 - Adventure Begins - Revamping backpacks to be upgradable and offer new perks as upgrades happen. and offer backpacks at each stage of maturity. - DONE
 * v2.0.0 - To be determined! Possibly Bag Variants
 * 
 */

using System;
using System.Reflection;
using AdventureBackpacks.Assets;
using AdventureBackpacks.Assets.Factories;
using AdventureBackpacks.Configuration;
using AdventureBackpacks.Extensions;
using BepInEx;
using HarmonyLib;
using ItemManager;
using JetBrains.Annotations;
using Vapok.Common.Abstractions;
using Vapok.Common.Managers;
using Vapok.Common.Managers.Configuration;
using Vapok.Common.Tools;

namespace AdventureBackpacks
{
    [BepInPlugin(_pluginId, _displayName, _version)]
    [BepInIncompatibility("JotunnBackpacks")]
    public class AdventureBackpacks : BaseUnityPlugin, IPluginInfo
    {
        //Module Constants
        private const string _pluginId = "vapok.mods.adventurebackpacks";
        private const string _displayName = "AdventureBackpacks";
        private const string _version = "1.5.6";
        
        //Interface Properties
        public string PluginId => _pluginId;
        public string DisplayName => _displayName;
        public string Version => _version;
        public BaseUnityPlugin Instance => _instance;
        
        //Class Properties
        public static ILogIt Log => _log;
        public static bool ValheimAwake = false;
        public static Waiting Waiter;
        
        //Class Privates
        private static AdventureBackpacks _instance;
        private static ConfigSyncBase _config;
        private static ILogIt _log;
        private Harmony _harmony;
        
        [UsedImplicitly]
        // This the main function of the mod. BepInEx will call this.
        private void Awake()
        {
            //I'm awake!
            _instance = this;
            
            //Waiting For Startup
            Waiter = new Waiting();
            
            //Initialize Managers
            Initializer.LoadManagers();

            //Register Configuration Settings
            _config = new ConfigRegistry(_instance);

            //Register Logger
            LogManager.Init(PluginId,out _log);

            PrefabManager.Initalized = true;

            Waiter.StatusChanged += InitializeBackpacks;
            
            //Patch Harmony
            _harmony = new Harmony(Info.Metadata.GUID);
            _harmony.PatchAll(Assembly.GetExecutingAssembly());

            //???

            //Profit
        }
        
        private void Update()
        {
            if (!Player.m_localPlayer || !ZNetScene.instance)
                return;

            if (!KeyPressTool.IgnoreKeyPresses(true) && KeyPressTool.CheckKeyDown(ConfigRegistry.HotKeyOpen.Value) && Player.m_localPlayer.CanOpenBackpack())
            {
                Player.m_localPlayer.OpenBackpack();
            }

            if (ConfigRegistry.OutwardMode.Value && !KeyPressTool.IgnoreKeyPresses(true) && KeyPressTool.CheckKeyDown(ConfigRegistry.HotKeyDrop.Value) && Player.m_localPlayer.CanOpenBackpack())
            {
                Player.m_localPlayer.QuickDropBackpack();
            }

        }

        public void InitializeBackpacks(object send, EventArgs args)
        {
            if (ValheimAwake)
                return;
            
            //Load Assets
            var backpackFactory = new BackpackFactory(_log, _config);
            backpackFactory.CreateAssets();
            
            //Setup Types
            Backpacks.LoadBackpackTypes(BackpackFactory.BackpackTypes());
            
            //Enable BoneReorder
            BoneReorder.ApplyOnEquipmentChanged(Info.Metadata.GUID);
            
            ConfigRegistry.Waiter.ConfigurationComplete(true);

            ValheimAwake = true;
        }
        
        private void OnDestroy()
        {
            _instance = null;
            _harmony?.UnpatchSelf();
        }

        public class Waiting
        {
            public void ValheimIsAwake(bool awakeFlag)
            {
                if (awakeFlag)
                    StatusChanged?.Invoke(this, EventArgs.Empty);
            }
            public event EventHandler StatusChanged;            
        }
    }
}