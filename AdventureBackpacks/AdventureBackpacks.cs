/* Adventure Backpacks by Vapok */

using System;
using System.Reflection;
using AdventureBackpacks.Assets;
using AdventureBackpacks.Assets.Factories;
using AdventureBackpacks.Compats;
using AdventureBackpacks.Configuration;
using AdventureBackpacks.Extensions;
using AdventureBackpacks.Features;
using AdventureBackpacks.Patches;
using APIManager;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using JetBrains.Annotations;
using Jotunn.Managers;
using Jotunn.Utils;
using Vapok.Common.Abstractions;
using Vapok.Common.Managers;
using Vapok.Common.Managers.Configuration;
using Vapok.Common.Managers.LocalizationManager;
using Vapok.Common.Tools;
using BoneReorder = Vapok.Common.Tools.BoneReorder;
using PrefabManager = ItemManager.PrefabManager;

namespace AdventureBackpacks
{
    [BepInPlugin(_pluginId, _displayName, _version)]
    [BepInIncompatibility("JotunnBackpacks")]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInDependency("com.ValheimModding.YamlDotNetDetector")]
    [BepInDependency("com.chebgonaz.ChebsNecromancy",BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.maxsch.valheim.contentswithin", BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Patch)]
    public class AdventureBackpacks : BaseUnityPlugin, IPluginInfo
    {
        //Module Constants
        private const string _pluginId = "vapok.mods.adventurebackpacks";
        private const string _displayName = "Adventure Backpacks";
        private const string _version = "1.9.7";
        
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
            
            Patcher.Patch(new []{"AdventureBackpacks.API"});
            
            //Waiting For Startup
            Waiter = new Waiting();
            
            //Jotunn Localization
            var localization = LocalizationManager.Instance.GetLocalization();

            //Register Logger
            LogManager.Init(PluginId,out _log);
            
            //Initialize Managers
            Initializer.LoadManagers(localization,false, true, true, true, false, false, true);

            //Register Configuration Settings
            _config = new ConfigRegistry(_instance);

            PrefabManager.Initalized = true;
           
            //Patch Harmony
            _harmony = new Harmony(Info.Metadata.GUID);
            _harmony.PatchAll(Assembly.GetExecutingAssembly());

            //Compatibilities
            if (Chainloader.PluginInfos.ContainsKey("com.chebgonaz.ChebsNecromancy"))
            {
                ChebsNecromancy.SetupNecromancyBackpackUsingApi();
            }

            if (Chainloader.PluginInfos.ContainsKey("com.maxsch.valheim.contentswithin"))
            {
                ContentsWithin.Awake(_harmony,"com.maxsch.valheim.contentswithin");
            }
            
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
            
            EffectsFactory.Instance.ToggleEffects();
            
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