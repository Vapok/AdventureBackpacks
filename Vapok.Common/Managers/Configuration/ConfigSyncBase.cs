using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using ServerSync;
using Vapok.Common.Abstractions;
using Vapok.Common.Shared;

namespace Vapok.Common.Managers.Configuration;

public abstract class ConfigSyncBase
{
    private static ConfigSyncBase? _instance;
    protected static ConfigFile? _config;
    private static ConfigSync? _configSync;

    public static ConfigEntry<bool>?ServerEnforced { get; private set; }
    public static ConfigEntry<LogLevels>? LogLevel { get; private set;}
    
    public static ConfigEntry<bool>? LoggingEnabled { get; private set; }
    
    protected ConfigSyncBase(IPluginInfo _mod)
    {
        if (_instance == null)
            _instance = this;
        
        //Init Config File and Server Sync
        if (_config == null)
            _config = _mod.Instance.Config;
            
        if (_configSync == null)
            _configSync = new ConfigSync(_mod.PluginId)
            {
                DisplayName = _mod.DisplayName, CurrentVersion = _mod.Version, MinimumRequiredVersion = _mod.Version
            };
            
        //Save on Set
        _config.SaveOnConfigSet = true;
        
        LoggingEnabled = _config.Bind("Log Output Configuration", "Logging Enabled", true,
            new ConfigDescription("Toggles Log Output", null, new ConfigurationManagerAttributes{ IsAdvanced = true}));
            
        LogLevel = _config.Bind(
            "Log Output Configuration", "Log Level", LogLevels.Warning,
            new ConfigDescription("Minimum Log Level to Output",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true}));

        ServerEnforced = SyncedConfig("Server-Synced and Enforced Config", "Lock Config", false,
            new ConfigDescription(
                "[Server Only] The configuration is locked and may not be changed by clients once it has been synced from the server. Only valid for server config, will have no effect on clients.", null, new ConfigurationManagerAttributes { Order = 50 }));

        _configSync.AddLockingConfigEntry(ServerEnforced!);

    }

    public abstract void InitializeConfigurationSettings();

    public static ConfigEntry<T>? SyncedConfig<T>(string group, string configName, T value, string description, bool synchronizedSetting = true) => SyncedConfig(group, configName, value, new ConfigDescription(description), synchronizedSetting);
        
    public static ConfigEntry<T>? SyncedConfig<T>(string group, string configName, T value, ConfigDescription description, bool synchronizedSetting = true)
    {
        if (_config == null || _configSync == null)
            return null;
        
        var configEntry = _config.Bind(RemoveInvalidCharacters(group), RemoveInvalidCharacters(configName), value, description);

        var syncedConfigEntry = _configSync.AddConfigEntry(configEntry);
        syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

        return configEntry;
    }

    private static string RemoveInvalidCharacters(string sectionName)
    {
        var invalidCharacters = new List<char> { '\n', '\t', '"', '\'', '[', ']' };
        
        invalidCharacters.ForEach(c => sectionName = sectionName.Replace(c.ToString(),String.Empty));
        return sectionName;
    }

}