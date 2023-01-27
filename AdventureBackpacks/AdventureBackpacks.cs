/* Adventure Backpacks by Vapok
 *
 * This largely a new port of the original Jotunn Backpacks.  Only.. without the Jotunn, and other things.
 *
 * Planned Roadmap:
 * v0.0.0 - Pre-Releases. May be buggy. May not function. But public input is valuable.
 * v1.0.0 - Initial Release - Get existing functionality ported.
 * v1.3.0 - Mistlands Backpack - Introduce new Backpack model that when crafted will include the Feather Fall effect.
 * v1.5.0 - Adventure Begins - Revamping backpacks to be upgradable and offer new perks as upgrades happen. and offer backpacks at each stage of maturity.
 * v2.0.0 - To be determined!
 *
 * TODO: Build out a better configuration management
 * 
 */

using System.Linq;
using System.Reflection;
using AdventureBackpacks.Assets;
using AdventureBackpacks.Assets.Factories;
using AdventureBackpacks.Configuration;
using AdventureBackpacks.Extensions;
using BepInEx;
using HarmonyLib;
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
    public class AdventureBackpacks : BaseUnityPlugin, IPluginInfo
    {
        //Module Constants
        private const string _pluginId = "vapok.mods.adventurebackpacks";
        private const string _displayName = "AdventureBackpacks";
        private const string _version = "1.0.4";
        
        //Interface Properties
        public string PluginId => _pluginId;
        public string DisplayName => _displayName;
        public string Version => _version;
        public BaseUnityPlugin Instance => _instance;
        
        //Class Properties
        public static ILogIt Log => _log;
        
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
            
            //Initialize Managers
            Initializer.LoadManagers();

            //Register Configuration Settings
            _config = new ConfigRegistry(_instance);

            //Register Logger
            LogManager.Init(PluginId,out _log);
            
            //Load Assets
            var backpackFactory = new BackpackFactory(_log, _config);
            backpackFactory.CreateAssets();
            
            //Enable BoneReorder
            BoneReorder.ApplyOnEquipmentChanged(Info.Metadata.GUID);
            
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
        
        private void OnDestroy()
        {
            _instance = null;
            _harmony?.UnpatchSelf();
        }
    }
}