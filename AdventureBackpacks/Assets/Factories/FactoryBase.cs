using Vapok.Common.Abstractions;
using Vapok.Common.Managers.Configuration;

namespace AdventureBackpacks.Assets.Factories;

public abstract class FactoryBase
{
    private ILogIt _logger;
    private ConfigSyncBase _config;
    
    internal ILogIt Log => _logger;
    internal ConfigSyncBase Config => _config;
    
    internal FactoryBase(ILogIt logger, ConfigSyncBase configs)
    {
        _logger = logger;
        _config = configs;
    }
}