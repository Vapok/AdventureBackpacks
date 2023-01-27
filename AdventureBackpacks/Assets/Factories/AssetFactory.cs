using Vapok.Common.Abstractions;
using Vapok.Common.Managers.Configuration;

namespace AdventureBackpacks.Assets.Factories;

internal abstract class AssetFactory
{
    private ILogIt _logger;
    private ConfigSyncBase _config;
    
    internal ILogIt Log => _logger;
    internal ConfigSyncBase Config => _config;
    
    internal AssetFactory(ILogIt logger, ConfigSyncBase configs)
    {
        _logger = logger;
        _config = configs;
    }

    internal abstract void CreateAssets();
}