using BepInEx.Configuration;
using ServerSync;
using Vapok.Common.Abstractions;

namespace Vapok.Common.Managers.Configuration;

public abstract class ConfigSyncBase
{
    protected static ConfigSyncBase? _instance;
    protected static ConfigFile? _config;
    protected static ConfigSync? _configSync;

    internal static ConfigEntry<bool>? LoggingEnabled { get; private set; }
    internal static ConfigEntry<LogLevels>? LogLevel { get; private set;}

    
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
            new ConfigDescription("Toggles Log Output", null, new ConfigAttributes{ IsAdvanced = true}));
            
        LogLevel = _config.Bind(
            "Log Output Configuration", "Log Level", LogLevels.Warning,
            new ConfigDescription("Minimum Log Level to Output",
                null,
                new ConfigAttributes { IsAdvanced = true}));

    }

    public virtual void InitializeConfigurationSettings()
    {

    }

    public static ConfigEntry<T>? SyncedConfig<T>(string group, string configName, T value, string description, bool synchronizedSetting = true) => SyncedConfig(group, configName, value, new ConfigDescription(description), synchronizedSetting);
        
    public static ConfigEntry<T>? SyncedConfig<T>(string group, string configName, T value, ConfigDescription description, bool synchronizedSetting = true)
    {
        if (_config == null || _configSync == null)
            return null;
        
        var configEntry = _config.Bind(group, configName, value, description);

        var syncedConfigEntry = _configSync.AddConfigEntry(configEntry);
        syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

        return configEntry;
    }

}