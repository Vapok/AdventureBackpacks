/* Adventure Backpacks by Vapok */

using System;
using System.Reflection;
using AdventureBackpacks.Assets;
using AdventureBackpacks.Assets.Factories;
using AdventureBackpacks.Configuration;
using AdventureBackpacks.Extensions;
using AdventureBackpacks.Features;
using AdventureBackpacks.Patches;
using APIManager;
using BepInEx;
using HarmonyLib;
using ItemManager;
using JetBrains.Annotations;
using Vapok.Common.Abstractions;
using Vapok.Common.Managers;
using Vapok.Common.Managers.Configuration;
using Vapok.Common.Managers.LocalizationManager;
using Vapok.Common.Tools;

namespace AdventureBackpacks
{
    [BepInPlugin(_pluginId, _displayName, _version)]
    [BepInIncompatibility("JotunnBackpacks")]
    [BepInDependency("com.chebgonaz.ChebsNecromancy",BepInDependency.DependencyFlags.SoftDependency)]
    public class AdventureBackpacks : BaseUnityPlugin, IPluginInfo
    {
        //Module Constants
        private const string _pluginId = "vapok.mods.adventurebackpacks";
        private const string _displayName = "Adventure Backpacks";
        private const string _version = "1.6.30";
        
        //Interface Properties
        public string PluginId => _pluginId;
        public string DisplayName => _displayName;
        public string Version => _version;
        public BaseUnityPlugin Instance => _instance;
        
        //Class Properties
        public static ILogIt Log => _log;
        public static bool ValheimAwake;
        public static bool PerformYardSale = false;
        public static bool QuickDropping = false;
        public static bool BypassMoveProtection = false;
        public static Waiting Waiter;
        public static ConfigSyncBase ActiveConfig => _config;
        
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
            
            Patcher.Patch();
            
            //Waiting For Startup
            Waiter = new Waiting();
            //Initialize Managers
            Initializer.LoadManagers(false, true, true, true, false, false, true, true);

            //Register Configuration Settings
            _config = new ConfigRegistry(_instance);

            //Register Logger
            LogManager.Init(PluginId,out _log);

            PrefabManager.Initalized = true;
            
            //Patch Harmony
            _harmony = new Harmony(Info.Metadata.GUID);
            _harmony.PatchAll(Assembly.GetExecutingAssembly());

            //???

            //Profit
        }

        private void Start()
        {
            Localizer.Waiter.StatusChanged += InitializeBackpacks;
            
            //Initialized Features
            QuickTransfer.FeatureInitialized = true;
        }

        private void Update()
        {
            if (!Player.m_localPlayer || !ZNetScene.instance)
                return;

            if (PerformYardSale)
            {
                var backpack = Player.m_localPlayer.GetEquippedBackpack();
                if (backpack != null)
                    Backpacks.PerformYardSale(Player.m_localPlayer, backpack.Item);
            }

            if ((ZInput.GetButton("Forward") || ZInput.GetButton("Backward") || ZInput.GetButton("Left") ||ZInput.GetButton("Right")) 
                && ZInput.GetKeyDown(ConfigRegistry.HotKeyDrop.Value.MainKey) &&  ConfigRegistry.OutwardMode.Value)
            {
                Player.m_localPlayer.QuickDropBackpack();
            }

            InventoryPatches.ProcessItemsAddedQueue();
        }

        public void InitializeBackpacks(object send, EventArgs args)
        {
            if (ValheimAwake)
                return;
            
            //Register Effects
            var effectsFactory = new EffectsFactory(_log, _config);
            effectsFactory.RegisterEffects();
            
            //Register Assets
            var backpackFactory = new BackpackFactory(_log, _config);
            backpackFactory.CreateAssets();
            
            //Setup Backpack Types
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